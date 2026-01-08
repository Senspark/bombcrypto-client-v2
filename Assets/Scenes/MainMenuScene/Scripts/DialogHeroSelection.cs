using System;
using System.Collections.Generic;

using App;

using Cysharp.Threading.Tasks;

using Data;

using Engine.Input.ControllerNavigation;

using Game.Dialog;
using Game.Manager;

using JetBrains.Annotations;

using Senspark;

using Services.Server.Exceptions;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;

namespace Scenes.MainMenuScene.Scripts {
    public class DialogHeroSelection : Dialog {
        [SerializeField]
        private BLHeroesSelection heroSelection;

        [SerializeField]
        private BLHeroInfomation hero;
        

        private ISoundManager _soundManager;
        private IStorageManager _storageManager;
        private IServerRequester _serverRequester;

        private HeroId _pvpHeroId;
        private List<HeroGroupData> _pvpHeroes;
        private Dictionary<int, StatData[]> _equipSkinStats;
        private System.Action<HeroId> _onSelectedCallback;
        private bool _isClicked;

        public static UniTask<DialogHeroSelection> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogHeroSelection>();
        }
        
        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _serverRequester = ServiceLocator.Instance.Resolve<IServerRequester>();
        }

        public void SetInfo(HeroId pvpHeroId, List<HeroGroupData> pvpHeroes,
            Dictionary<int, StatData[]> equipSkinStats,
            System.Action<HeroId> callback) {
            _pvpHeroId = pvpHeroId;
            _pvpHeroes = pvpHeroes;
            _equipSkinStats = equipSkinStats;
            _onSelectedCallback = callback;
            heroSelection.SetOnSelectItem(ChoosePvpHero);
            heroSelection.LoadHeroes(pvpHeroes);
            heroSelection.SetSelectedItem(pvpHeroId);
            heroSelection.SetSelectItem(pvpHeroId);
        }

        private void ChoosePvpHero(int index, bool isSound) {
            if (isSound) {
                _soundManager.PlaySound(Audio.Tap);
            }
            var pvpHero = _pvpHeroes[index];
            UpdatePvpHero(pvpHero.PlayerData);
        }

        private void UpdatePvpHero(PlayerData pvpHero) {
            _pvpHeroId = pvpHero.heroId;
            hero.UpdateHero(pvpHero);
            hero.UpdateStatsFromSkinStats(_equipSkinStats);
        }

        public void OnSelectButtonClicked() {
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    await _serverRequester.ActiveTRHero(_pvpHeroId.Id);
                    _storageManager.SelectedHeroKey = _pvpHeroId.Id;
                    heroSelection.SetSelectedItem(_pvpHeroId);
                    heroSelection.SetSelectedItem(_pvpHeroId);
                    _onSelectedCallback(_pvpHeroId);
                    Hide();
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        await DialogError.ShowError(DialogCanvas, e.Message, ()=>{_isClicked = false;});    
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message, ()=>{_isClicked = false;});
                    }
                } finally {
                    waiting.End();
                }
            });
        }

        protected override void OnYesClick() {
            if(_isClicked) return;
            _isClicked = true;
            OnSelectButtonClicked();
        }
    }
}