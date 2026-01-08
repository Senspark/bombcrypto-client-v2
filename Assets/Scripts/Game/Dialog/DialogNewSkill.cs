using App;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogNewSkill : Dialog {
        [SerializeField]
        private Avatar avatar;

        [SerializeField]
        private Text heroIdLbl;

        [SerializeField]
        private GameObject[] abilityIcons;

        public static DialogNewSkill Create() {
            var prefab = Resources.Load<DialogNewSkill>("Prefabs/Dialog/DialogNewSkill");
            return Instantiate(prefab);
        }

        public void SetInfo(PlayerData player) {
            heroIdLbl.text = player.heroId.Id.ToString();
            avatar.ChangeImage(player);
            foreach (var icon in abilityIcons) {
                icon.SetActive(false);
            }
            foreach (var ability in player.abilities) {
                abilityIcons[(int) ability].SetActive(true);
            }
        }

        public void OnOkBtnClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Hide();
        }
    }
}