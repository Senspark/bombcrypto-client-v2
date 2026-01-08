using System;
using System.Threading.Tasks;

using App;

using BomberLand.Component;

using Castle.Core.Internal;

using Constant;

using DG.Tweening;

using Engine.Entities;

using PvpMode.Manager;

using Services;

using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Tutorial {
    public class TutorialPvpMenu : MonoBehaviour {
        [SerializeField]
        private TutorialSelectEquipments heroChoose;

        [SerializeField]
        private GameObject[] heroStatBooster;

        [SerializeField]
        private GameObject dimBackground;

        [SerializeField]
        private GameObject objBoosterList;

        [SerializeField]
        private UnityEngine.UI.Button btFindMatch;

        public UnityEngine.UI.Button BtFindMatch => btFindMatch;

        private TutorialBoosterButton[] _boostersButton;

        private Action _onFindMathCallback;

        public void Initialized(Canvas canvas) {
            InitHeroChoose(canvas);
            _boostersButton = objBoosterList.GetComponentsInChildren<TutorialBoosterButton>();
            foreach (var b in _boostersButton) {
                var bt = b.GetComponent<UnityEngine.UI.Button>();
                bt.interactable = false;
            }
        }

        public void SetVisible(int index, bool value) {
            heroStatBooster[index].SetActive(value);
        }

        private void InitHeroChoose(Canvas canvas) {
            var playerData = new PlayerData() {
                itemId = (int) GachaChestProductId.Ninja,
                playerType = PlayerType.Ninja,
                playercolor = PlayerColor.HeroTr,
            };

            var skinList = new ISkinManager.Skin[] {
                BLTutorialGui.CreateSkin((int) InventoryItemType.BombSkin, 49, "", true),
                BLTutorialGui.CreateSkin((int) InventoryItemType.Avatar, 64, "", true),
                BLTutorialGui.CreateSkin((int) InventoryItemType.Fire, 76, "", true),
                BLTutorialGui.CreateSkin((int) InventoryItemType.Trail, 125, "", true),
            };

            heroChoose.Initialized(canvas, playerData, skinList, null);
        }

        public void SetDimSiblingIndex(int siblingIndex) {
            dimBackground.transform.SetSiblingIndex(siblingIndex);
        }

        public void ShowDim() {
            dimBackground.GetComponent<Image>().DOFade(125.0f / 255.0f, 0.3f);
        }

        public void HideDim() {
            dimBackground.GetComponent<Image>().DOFade(0, 0.3f);
        }

        public Task<bool> WaitChooseBooster(BoosterType boosterType, GameObject pointerHand,
            ISoundManager soundManager) {
            var task = new TaskCompletionSource<bool>();
            foreach (var b in _boostersButton) {
                var bt = b.GetComponent<UnityEngine.UI.Button>();
                if (b.BoosterType != boosterType) {
                    bt.interactable = false;
                } else {
                    bt.interactable = true;
                    bt.onClick.AddListener(() => {
                        soundManager.PlaySound(Audio.Tap);
                        bt.interactable = false;
                        pointerHand.SetActive(false);
                        task.SetResult(true);
                    });
                    pointerHand.SetActive(true);
                    pointerHand.transform.position = bt.transform.position;
                }
            }
            // b.SetHighlight(false);
            return task.Task;
        }

        public void SetBoosterHighlight(BoosterType boosterType, bool highlight) {
            _boostersButton.Find(it => it.BoosterType == boosterType).SetHighlight(highlight);
        }

        public void SetBoosterQuality(BoosterType boosterType, int quality) {
            _boostersButton.Find(it => it.BoosterType == boosterType).SetQuality(quality);
        }

        private void OnFindMathButtonClicked() {
            _onFindMathCallback?.Invoke();
        }

        public Task<bool> WaitFindMatch(ISoundManager soundManager) {
            var task = new TaskCompletionSource<bool>();
            btFindMatch.interactable = true;
            _onFindMathCallback = () => {
                soundManager.PlaySound(Audio.Tap);
                btFindMatch.interactable = false;
                task.SetResult(true);
            };
            return task.Task;
        }
    }
}