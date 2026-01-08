using System;
using System.Linq;
using App;

using Cysharp.Threading.Tasks;

using Senspark;
using Game.UI;

using Scenes.FarmingScene.Scripts;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class MaterialPolygon : MonoBehaviour {
        [SerializeField]
        private Text amountMaterials;

        [SerializeField]
        private Text amountHeroes;

        [SerializeField]
        private GameObject materialHero;

        [SerializeField]
        private GameObject materialHeroEmpty;

        [SerializeField]
        private Button btnExchange;

        [SerializeField]
        private Text rockText;

        private float _saveAmountMaterials;
        private int _saveAmountHero;
        private HeroId[] _lstHeroesIdBurn;
        private PlayerData _playerData;
        
        private Canvas _canvas;

        private ISoundManager _soundManager;
        private IPlayerStorageManager _playerStoreManager;
        private IBlockchainManager _blockchainManager;
        private IServerManager _serverManager;
        private IStorageManager _storeManager;
        private IChestRewardManager _chestRewardManager;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
        }

        private void Start() {
            UpdateUIMaterial(0,0);
        }

        public void SetInfo(Canvas canvas) {
            _canvas = canvas;
        }
        
        public async void ExchangeButtonPressed() {
            //Kiểm tra trước xem có hero có stake  nào bị burn ko
            var heroStakeCount =  CheckStakedBeforeBurn(_lstHeroesIdBurn);
            if (heroStakeCount > 0) {
               var confirm = await DialogConfirmBurnOrFusion.Create();
               confirm.SetInfo(
                   heroStakeCount,
                   PerformExchange,
                   () => { }
               );
              confirm.Show(_canvas);
              return;
            }
            PerformExchange();
        }

        private void PerformExchange() {
            UniTask.Void(async () => {
                var waiting = await DialogWaiting.Create();
                waiting.Show(_canvas);
                waiting.ShowLoadingAnim();
                
                try {
                    var ids = _lstHeroesIdBurn.Select(e => e.Id).ToArray();
                    var tx = await _blockchainManager.CreateRock(ids);
                    if (tx == "") {
                        throw new Exception("Burn Failed");
                    }
                    
                    _storeManager.LastBurnHeroData = new BurnHeroData {
                        LastListHeroIdBurn = _lstHeroesIdBurn,
                        LastTx = tx,
                    };
                    _playerStoreManager.RemoveBurnHeroes(_lstHeroesIdBurn);
                    //await _blockchainManager.GetRockAmount();
                    //await _serverManager.General.SyncHero(false);
                    
                    //destroy hero in map
                    LevelScene.Instance.AddNewPlayersOrRefresh(_lstHeroesIdBurn);
                    
                    BurnHero();
                } catch (Exception e) {
                    btnExchange.interactable = true;
                    DialogForge.ShowError(_canvas, e.Message);
                } finally {
                    waiting.Hide();
                }
            });
        }

        private async void BurnHero() {
            try {
                await _serverManager.General.BurnHero();
                OnSuccessExchange();
            } catch (Exception e) {
                DialogForge.ShowInfo(_canvas, "Error", "Burn Hero failed\nPlease try again", "RETRY" , () => {
                    BurnHero();
                });
            }
        }

        /// <summary>
        /// Trả về true nếu trong list có hero S fake
        /// </summary>
        /// <param name="heroIds"></param>
        /// <returns></returns>
        private int CheckStakedBeforeBurn(HeroId[] heroIds) {
            var heroStakeCount = 0;
            foreach (var heroId in heroIds) {
                var player = _playerStoreManager.GetPlayerDataFromId(heroId);
                if (player == null) 
                    continue;
                if (player.HaveAnyStaked()) {
                    heroStakeCount++;
                }
            }
            return heroStakeCount;
        }

        public void SelectHeroButtonPressed() {
            _soundManager.PlaySound(Audio.Tap);
            OpenInventoryDialogModeBurnHero();
        }

        private async void OpenInventoryDialogModeBurnHero() {
            var dialog = await DialogInventoryCreator.Create();
            var exclude = _playerStoreManager.GetPlayerDataList(HeroAccountType.Nft)
                .Where(e => !CanProcessThisHero(e))
                .Select(e => e.heroId).ToArray();
            dialog.SetChooseHeroForInventoryBurnHero(exclude, OnChangeHero);
            dialog.Show(_canvas);
        }

        private void OnChangeHero(PlayerData[] lstHeroesIdBurn) {
            _lstHeroesIdBurn = lstHeroesIdBurn.Where(e => e != null).Select(e => e.heroId).ToArray();
            _saveAmountMaterials = 0;
            foreach (var player in lstHeroesIdBurn) {
                var heroType = _playerStoreManager.GetHeroRarity(player);
                _saveAmountMaterials += RateExchangeHeroToMaterials(heroType, player.IsHeroS);
            }
            _saveAmountHero = _lstHeroesIdBurn.Length;
            UpdateUIMaterial(_saveAmountMaterials, _saveAmountHero);
        }

        private void UpdateUIMaterial(float totalMaterials, int heroes) {
            amountHeroes.text = $"{heroes}";
            amountMaterials.text = $"{totalMaterials}";
            amountMaterials.gameObject.SetActive(heroes != 0);
            materialHero.gameObject.SetActive(heroes != 0);
            materialHeroEmpty.gameObject.SetActive(heroes == 0);
            btnExchange.interactable = heroes != 0;
        }

        private void OnSuccessExchange() {
            var rockAmount = _chestRewardManager.GetChestReward(BlockRewardType.Rock);
            rockText.text = Math.Truncate(rockAmount).ToString("N0");

            //reset UI
            DialogForge.ShowInfo(_canvas, "CONFIRMATION", "Exchange Success", "OK", () => {
                UpdateUIMaterial(0, 0);
            });
        }
        
        private static bool CanProcessThisHero(PlayerData hero) {
            if (hero == null || hero.AccountType == HeroAccountType.Trial) {
                return false;
            }
            return true;
        }
        
        private float RateExchangeHeroToMaterials(HeroRarity heroType, bool isHeroS) {
            var burnHeroConfig = _storeManager.BurnHeroConfig;
            return isHeroS?  burnHeroConfig.Data[heroType].heroSRock : burnHeroConfig.Data[heroType].heroLRock;
        }
    }
}