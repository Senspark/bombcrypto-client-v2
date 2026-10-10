using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

using App;

using Cysharp.Threading.Tasks;

using Game.Manager;

using Scenes.FarmingScene.Scripts;

using Senspark;

using Server.Models;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    /// <summary>
    /// Khung chung cho ba tab Smithy trả bằng native coin (BNB / POL): Upgrade BHero Level,
    /// Reset BHero Skill, Reset BHero Skin.
    ///
    /// Ba ràng buộc của contract quyết định toàn bộ luồng dưới đây:
    /// - <c>require(msg.value == price)</c> và KHÔNG hoàn phần dư, nên giá phải đọc lại ngay trước
    ///   khi ký; nếu lệch so với giá đang hiển thị thì dừng lại hỏi người chơi chứ không tự ký.
    /// - Tiền mua và tiền gas rút từ CÙNG một ví native, nên phải chừa gas chứ không cho tiêu sạch.
    /// - Giá 0 nghĩa là tính năng đóng cho rarity đó (gọi vào sẽ revert).
    ///
    /// Giá native được neo theo BCOIN giống Quartz: contract tính <c>giá BCOIN × 5 × nativeRate</c>,
    /// và server cập nhật nativeRate on-chain theo giá thị trường. Vì vậy giá có thể đổi trong lúc
    /// dialog đang mở: báo giá được làm mới định kỳ, và một tx thất bại do rate vừa đổi được báo
    /// riêng cho người chơi thay vì lỗi chung chung.
    /// </summary>
    public abstract class NativeHeroActionPolygon : MonoBehaviour {
        [SerializeField]
        protected SmithyHeroSlot heroSlot;

        [SerializeField]
        protected Button actionBtn;

        [SerializeField]
        protected Text priceLbl;

        protected ISoundManager SoundManager;
        protected IBHeroManager PlayerStoreManager;
        protected IBlockchainManager BlockchainManager;
        protected IServerManager ServerManager;

        protected Canvas Canvas;
        protected PlayerData Hero;

        private Action<PlayerData> _chooseHeroCallBack;
        private UserAccount _userAccount;
        private string _quotedPriceWei;
        private string _quotedRateWei;
        private double _nativeBalance;
        private bool _inFlight;

        // Mỗi lần báo giá tăng số này; kết quả của lần báo giá cũ hơn (hero đã đổi, hoặc lần làm
        // mới định kỳ chạy chồng lên) bị bỏ qua thay vì ghi đè báo giá mới hơn.
        private int _quoteSeq;
        private float _nextAutoQuoteAt;

        // Chừa lại cho gas. Ba hàm này đều là một tx đơn giản; con số này chỉ để chặn trước
        // thay vì để người chơi ký rồi fail vì hết tiền gas.
        private const double GasHeadroom = 0.002;

        // Server cập nhật nativeRate on-chain vài phút một lần, nên làm mới báo giá theo nhịp này
        // là đủ để nút và số dư tối thiểu không bị lệch lâu.
        private const float AutoQuoteInterval = 60f;

        protected abstract string ActionName { get; }

        /// <summary>Giá hiện tại theo wei, hoặc "0" nếu tính năng đóng cho hero này.</summary>
        protected abstract Task<string> ReadPriceWei();

        protected abstract Task<HeroActionResult> SendAction(string priceWei);

        /// <summary>Hero này có hợp lệ cho tính năng không (chưa xét giá và số dư).</summary>
        protected abstract bool CanProcess(PlayerData hero);

        protected virtual void Awake() {
            SoundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            PlayerStoreManager = ServiceLocator.Instance.Resolve<IBHeroManager>();
            BlockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            ServerManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _userAccount = ServiceLocator.Instance.Resolve<IUserAccountManager>().GetRememberedAccount();
            if (actionBtn) {
                actionBtn.interactable = false;
            }
        }

        public void SetInfo(Canvas canvas, Action<PlayerData> chooseHeroCallBack) {
            Canvas = canvas;
            _chooseHeroCallBack = chooseHeroCallBack;
        }

        public void Init(PlayerData hero) {
            Hero = CanProcess(hero) ? hero : null;
            if (heroSlot) {
                heroSlot.Show(Hero);
            }
            OnHeroChanged();
            RefreshQuote();
        }

        /// <summary>Cho lớp con dựng lại phần UI riêng của nó khi hero đổi.</summary>
        protected virtual void OnHeroChanged() { }

        protected string ChainName => _userAccount != null && _userAccount.network == NetworkType.Polygon
            ? DialogBridgeAmount.ChainPolygon
            : DialogBridgeAmount.ChainBsc;

        protected string CoinSymbol => ChainName == DialogBridgeAmount.ChainPolygon ? "POL" : "BNB";

        protected int RarityOf(PlayerData hero) => hero.rare;

        /// <summary>Nút chọn hero trên prefab trỏ vào đây. Cả ba tab đều dùng chung.</summary>
        public void OnChooseHeroBtnClicked() {
            ChooseHero();
        }

        // Mở picker chọn 1 hero, loại sẵn những con không dùng được cho tính năng này.
        protected async void ChooseHero() {
            SoundManager.PlaySound(Audio.Tap);
            var inventory = await DialogInventoryCreator.Create();
            if (!this) {
                return;
            }
            var exclude = PlayerStoreManager.GetPlayerDataList(HeroAccountType.Nft)
                .Where(e => !CanProcess(e))
                .Select(e => e.heroId)
                .ToArray();
            inventory.SetChooseHeroForResetRoi(exclude, heroId => {
                var picked = PlayerStoreManager.GetPlayerDataFromId(heroId);
                Init(picked);
                _chooseHeroCallBack?.Invoke(picked);
            });
            inventory.Show(Canvas);
        }

        // Làm mới báo giá định kỳ khi đang có hero, để giá hiển thị theo kịp nativeRate on-chain.
        protected virtual void Update() {
            if (Hero == null || _inFlight || Time.unscaledTime < _nextAutoQuoteAt) {
                return;
            }
            RefreshQuote(true);
        }

        // Quay lại tab (thường là sau khi mở ví) thì làm mới ngay, không chờ hết nhịp.
        private void OnApplicationFocus(bool hasFocus) {
            if (hasFocus && Hero != null && !_inFlight) {
                RefreshQuote(true);
            }
        }

        // Đọc giá + số dư. Đây mới chỉ là báo giá, KHÔNG phải giá sẽ ký.
        // silent = true: giữ nguyên giá đang hiện trong lúc đọc (làm mới định kỳ), không nháy "...".
        protected void RefreshQuote(bool silent = false) {
            var seq = ++_quoteSeq;
            _nextAutoQuoteAt = Time.unscaledTime + AutoQuoteInterval;
            if (Hero == null) {
                _quotedPriceWei = null;
                SetPriceText("--");
                SetInteractable(false);
                return;
            }

            UniTask.Void(async () => {
                if (!silent || string.IsNullOrEmpty(_quotedPriceWei)) {
                    SetInteractable(false);
                    SetPriceText("...");
                }
                try {
                    var priceWei = await ReadPriceWei();
                    if (!this || seq != _quoteSeq) {
                        return;
                    }
                    var rateWei = await ReadRateWei();
                    if (!this || seq != _quoteSeq) {
                        return;
                    }
                    _nativeBalance = await BlockchainManager.GetNativeWalletBalance(ChainName);
                    if (!this || seq != _quoteSeq) {
                        return;
                    }
                    _quotedPriceWei = priceWei;
                    _quotedRateWei = rateWei;
                    ApplyQuote(priceWei);
                } catch (Exception e) {
                    if (!this || seq != _quoteSeq) {
                        return;
                    }
                    _quotedPriceWei = null;
                    SetPriceText("--");
                    SetInteractable(false);
                    Debug.LogException(e);
                }
            });
        }

        // nativeRate chỉ dùng để hiện giá quy đổi ra BCOIN; đọc lỗi thì vẫn bán được, chỉ mất dòng
        // quy đổi.
        private async Task<string> ReadRateWei() {
            try {
                return await BlockchainManager.GetNativeRate();
            } catch (Exception e) {
                Debug.LogException(e);
                return null;
            }
        }

        private void ApplyQuote(string priceWei) {
            if (IsZero(priceWei)) {
                SetPriceText("--");
                SetInteractable(false);
                OnQuoteApplied(false);
                return;
            }
            var price = WeiToCoin(priceWei);
            var bcoin = BcoinEquivalent(priceWei, _quotedRateWei);
            SetPriceText(bcoin > 0
                ? $"{FormatCoin(price)} {CoinSymbol}\n<size=16>≈ {FormatBcoin(bcoin)} BCOIN</size>"
                : $"{FormatCoin(price)} {CoinSymbol}");
            // Tiền mua và tiền gas rút từ cùng một ví native, nên phải chừa gas. Không đủ thì
            // khoá nút, không giải thích — giống UpgradeShieldPolygon.
            SetInteractable(_nativeBalance >= price + GasHeadroom);
            OnQuoteApplied(true);
        }

        /// <summary>
        /// Chạy sau khi giá đã áp lên UI. Lớp con dùng để chặn thêm điều kiện riêng của nó
        /// (ví dụ Upgrade còn phải chọn xong nguyên liệu). <paramref name="hasPrice"/> false nghĩa
        /// là tính năng đóng cho hero này.
        /// </summary>
        protected virtual void OnQuoteApplied(bool hasPrice) { }

        // Người chơi bấm nút hành động. Đọc lại giá ngay trước khi ký; contract không hoàn tiền
        // thừa nên lệch một wei là mất tiền gas vô ích.
        public void OnActionBtnClicked() {
            if (_inFlight || Hero == null || string.IsNullOrEmpty(_quotedPriceWei)) {
                return;
            }
            SoundManager.PlaySound(Audio.Tap);
            // Chốt giá người chơi đang thấy lúc bấm: báo giá định kỳ có thể đổi _quotedPriceWei trong
            // lúc dialog xác nhận còn mở, và giá đem đi ký phải là giá đã thấy, không phải giá mới.
            var shownPriceWei = _quotedPriceWei;
            // Bước xác nhận chạy TRƯỚC khi khoá nút: nếu người chơi đóng dialog mà không chọn gì
            // thì nút vẫn dùng được, không bị kẹt ở trạng thái disabled.
            RequestConfirmation(() => StartAction(shownPriceWei));
        }

        /// <summary>
        /// Cho lớp con chèn một bước xác nhận trước khi ký. Gọi <paramref name="onConfirmed"/> để
        /// tiếp tục; không gọi gì cả nghĩa là huỷ. Mặc định không hỏi gì.
        /// </summary>
        protected virtual void RequestConfirmation(Action onConfirmed) {
            onConfirmed();
        }

        private void StartAction(string shownPriceWei) {
            if (_inFlight || !this || Hero == null || string.IsNullOrEmpty(shownPriceWei)) {
                return;
            }
            _inFlight = true;
            // Huỷ báo giá định kỳ đang chạy dở, nếu không nó có thể bật lại nút giữa lúc ký.
            ++_quoteSeq;
            SetInteractable(false);

            UniTask.Void(async () => {
                var waiting = await DialogWaiting.Create();
                if (!this) {
                    // Panel đã bị huỷ trong lúc chờ prefab load -> đóng luôn, đừng để dialog treo.
                    waiting.Hide();
                    return;
                }
                waiting.Show(Canvas);
                waiting.ShowLoadingAnim();
                try {
                    var freshPriceWei = await ReadPriceWei();
                    if (!this) {
                        return;
                    }
                    if (IsZero(freshPriceWei)) {
                        throw new Exception($"{ActionName} is not available for this hero.");
                    }
                    if (freshPriceWei != shownPriceWei) {
                        _quotedPriceWei = freshPriceWei;
                        ApplyQuote(freshPriceWei);
                        throw new Exception(PriceChangedMessage);
                    }

                    var result = await SendAction(freshPriceWei);
                    if (!this) {
                        return;
                    }
                    if (!result.success) {
                        throw new Exception(await DescribeFailure(freshPriceWei));
                    }

                    // details là state thật ngay sau receipt -> vẽ lại được luôn, không phải chờ
                    // server. SyncHero chỉ để server hội tụ theo sau.
                    await ServerManager.General.SyncHero(false, false, true);
                    if (!this) {
                        return;
                    }
                    var updated = BuildUpdatedHero(result);
                    OnActionSucceeded(updated);
                    _chooseHeroCallBack?.Invoke(updated);
                    Init(ClearSlotAfterSuccess ? null : updated);
                    ShowSuccess(updated);
                } catch (Exception e) {
                    if (this) {
                        DialogForge.ShowError(Canvas, e.Message);
                    }
                } finally {
                    _inFlight = false;
                    if (this) {
                        RefreshQuote();
                    }
                    waiting.Hide();
                }
            });
        }

        private const string PriceChangedMessage =
            "The price was just updated (it follows the BCOIN market rate). " +
            "Please check the new price and try again.";

        // Bridge chỉ trả success=false, không kèm lý do. Nguyên nhân hay gặp nhất sau khi neo
        // theo BCOIN là nativeRate đổi giữa lúc ký và lúc tx vào block (require(msg.value == price)
        // revert), nên đọc lại giá: lệch so với giá đã gửi thì báo đúng lý do đó.
        private async Task<string> DescribeFailure(string sentPriceWei) {
            try {
                var currentPriceWei = await ReadPriceWei();
                if (!IsZero(currentPriceWei) && currentPriceWei != sentPriceWei) {
                    return PriceChangedMessage;
                }
            } catch (Exception e) {
                Debug.LogException(e);
            }
            return $"{ActionName} failed.";
        }

        // details word là state thật ngay sau receipt. Không đọc lại từ store ở đây: SyncHero V4
        // trả cache trước rồi mới đẩy push, nên store lúc này vẫn là chỉ số CŨ.
        //
        // Ba tính năng này không có push riêng báo tx cho server, nên nếu chỉ dựng PlayerData để
        // hiện dialog thì store vẫn giữ bản cũ và Inventory sẽ hiện skin/skill cũ cho tới khi push
        // đồng bộ tới. Vì vậy ghi luôn details mới vào store; push của server tới sau sẽ ghi đè
        // bằng dữ liệu authoritative.
        private PlayerData BuildUpdatedHero(HeroActionResult result) {
            if (string.IsNullOrEmpty(result.details) || !BigInteger.TryParse(result.details, out _)) {
                return PlayerStoreManager.GetPlayerDataFromId(Hero.heroId);
            }
            // details word chỉ mang state on-chain; state runtime phải giữ lại từ bản đang có,
            // nếu không hero đang active sẽ thành inactive trong inventory.
            var fresh = new HeroDetails(result.details) {
                IsActive = Hero.active,
                Stage = Hero.stage,
                Energy = Mathf.RoundToInt(Hero.hp),
                Shield = Hero.Shield,
                StoryIsPlayed = Hero.storyIsPlayed,
                StakeBcoin = Hero.stakeBcoin,
                StakeSen = Hero.stakeSen,
                TimeSync = DateTime.Now.ToBinary(),
            };
            PlayerStoreManager.ReplaceOneHero(fresh);
            return BHeroManager.GeneratePlayerData(fresh);
        }

        /// <summary>Cho lớp con xử lý kết quả (ví dụ preload skin mới) trước khi UI dựng lại.</summary>
        protected virtual void OnActionSucceeded(PlayerData updated) { }

        /// <summary>Xoá ô chọn hero sau khi thành công.</summary>
        protected virtual bool ClearSlotAfterSuccess => false;

        protected virtual void ShowSuccess(PlayerData updated) {
            DialogForge.ShowInfo(Canvas, "Successfully");
        }

        protected void SetInteractable(bool value) {
            if (actionBtn) {
                actionBtn.interactable = value;
            }
        }

        protected void SetPriceText(string value) {
            if (priceLbl) {
                priceLbl.text = value;
            }
        }

        protected static bool IsZero(string wei) {
            return string.IsNullOrEmpty(wei) || BigInteger.TryParse(wei, out var v) && v.IsZero;
        }

        // Chỉ để HIỂN THỊ. Giá đem đi ký luôn là chuỗi wei nguyên vẹn, không bao giờ qua double.
        protected static double WeiToCoin(string wei) {
            if (!BigInteger.TryParse(wei, out var value)) {
                return 0;
            }
            var whole = BigInteger.DivRem(value, BigInteger.Pow(10, 18), out var rest);
            return (double) whole + (double) rest / 1e18;
        }

        protected static string FormatCoin(double value) {
            return value.ToString("0.######", CultureInfo.InvariantCulture);
        }

        // Giá quy đổi ra BCOIN = giá native / nativeRate (cả hai đều 18 số lẻ, nên tỉ số không cần
        // đổi đơn vị). Chỉ để HIỂN THỊ; 0 nghĩa là không có rate để quy đổi.
        protected static double BcoinEquivalent(string priceWei, string rateWei) {
            if (!BigInteger.TryParse(priceWei, out var price) || !BigInteger.TryParse(rateWei, out var rate) ||
                rate.Sign <= 0) {
                return 0;
            }
            // Nhân 100 trước khi chia để giữ 2 số lẻ mà không qua double.
            return (double) BigInteger.Divide(price * 100, rate) / 100;
        }

        protected static string FormatBcoin(double value) {
            return value.ToString("#,0.##", CultureInfo.InvariantCulture);
        }
    }
}
