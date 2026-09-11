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
        private double _nativeBalance;
        private bool _inFlight;

        // Chừa lại cho gas. Ba hàm này đều là một tx đơn giản; con số này chỉ để chặn trước
        // thay vì để người chơi ký rồi fail vì hết tiền gas.
        private const double GasHeadroom = 0.002;

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

        // Đọc giá + số dư. Đây mới chỉ là báo giá, KHÔNG phải giá sẽ ký.
        protected void RefreshQuote() {
            if (Hero == null) {
                _quotedPriceWei = null;
                SetPriceText("--");
                SetInteractable(false);
                return;
            }

            UniTask.Void(async () => {
                SetInteractable(false);
                SetPriceText("...");
                try {
                    var priceWei = await ReadPriceWei();
                    if (!this) {
                        return;
                    }
                    _nativeBalance = await BlockchainManager.GetNativeWalletBalance(ChainName);
                    if (!this) {
                        return;
                    }
                    _quotedPriceWei = priceWei;
                    ApplyQuote(priceWei);
                } catch (Exception e) {
                    if (!this) {
                        return;
                    }
                    _quotedPriceWei = null;
                    SetPriceText("--");
                    SetInteractable(false);
                    Debug.LogException(e);
                }
            });
        }

        private void ApplyQuote(string priceWei) {
            if (IsZero(priceWei)) {
                SetPriceText("--");
                SetInteractable(false);
                OnQuoteApplied(false);
                return;
            }
            var price = WeiToCoin(priceWei);
            SetPriceText(BuildPriceText(price));
            // Tiền mua và tiền gas rút từ cùng một ví native, nên phải chừa gas. Không đủ thì
            // khoá nút, không giải thích — giống UpgradeShieldPolygon.
            SetInteractable(_nativeBalance >= price + GasHeadroom);
            OnQuoteApplied(true);
        }

        /// <summary>
        /// Texto do preco. Por padrao so o nativo. Upgrade sobrescreve para mostrar
        /// BCOIN + SEN + nativo nos niveis 6-10.
        /// </summary>
        protected virtual string BuildPriceText(double nativePrice) {
            return $"{FormatCoin(nativePrice)} {CoinSymbol}";
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
            // Bước xác nhận chạy TRƯỚC khi khoá nút: nếu người chơi đóng dialog mà không chọn gì
            // thì nút vẫn dùng được, không bị kẹt ở trạng thái disabled.
            RequestConfirmation(StartAction);
        }

        /// <summary>
        /// Cho lớp con chèn một bước xác nhận trước khi ký. Gọi <paramref name="onConfirmed"/> để
        /// tiếp tục; không gọi gì cả nghĩa là huỷ. Mặc định không hỏi gì.
        /// </summary>
        protected virtual void RequestConfirmation(Action onConfirmed) {
            onConfirmed();
        }

        private void StartAction() {
            if (_inFlight || !this || Hero == null || string.IsNullOrEmpty(_quotedPriceWei)) {
                return;
            }
            _inFlight = true;
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
                    if (freshPriceWei != _quotedPriceWei) {
                        _quotedPriceWei = freshPriceWei;
                        ApplyQuote(freshPriceWei);
                        throw new Exception("The price just changed. Please check the new price and try again.");
                    }

                    var result = await SendAction(freshPriceWei);
                    if (!this) {
                        return;
                    }
                    if (!result.success) {
                        throw new Exception($"{ActionName} failed.");
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
    }
}
