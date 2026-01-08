using System;
using System.Threading.Tasks;

using App;

using DG.Tweening;

using UnityEngine;

namespace BomberLand.Tutorial {
    public class TutorialEquipment {
        public string Title { get; }
        public int ItemId { get; }
        public string Footer { get; }

        public TutorialEquipment(string title, int itemId, string footer) {
            Title = title;
            ItemId = itemId;
            Footer = footer;
        }
    }

    public class TutorialNewEquipments : MonoBehaviour {
        [SerializeField]
        private Transform equipmentContainer;

        [SerializeField]
        private TutorialEquipmentItem prefabEquipment;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private UnityEngine.UI.Button btClose;

        private Action _onClaimCallback;

        private void Awake() {
            btClose.interactable = false;
        }

        public void Initialized(TutorialEquipment[] equipments, System.Action claimCallback) {
            _onClaimCallback = claimCallback;
            foreach (var equip in equipments) {
                CreateItem(equip);
            }
        }

        private void CreateItem(TutorialEquipment equipment) {
            var equip = Instantiate(prefabEquipment, equipmentContainer);
            equip.SetInfo(equipment);
        }

        public void OnClaimButtonClicked() {
            _onClaimCallback?.Invoke();
        }

        public Task<bool> CloseAsync(ISoundManager soundManager) {
            var task = new TaskCompletionSource<bool>();
            btClose.interactable = true;
            _onClaimCallback = () => {
                soundManager.PlaySound(Audio.Tap);
                canvasGroup.DOFade(0.0f, 0.3f).OnComplete(() => {
                    Destroy(gameObject);
                    task.SetResult(true);
                });
            };
            return task.Task;
        }
    }
}