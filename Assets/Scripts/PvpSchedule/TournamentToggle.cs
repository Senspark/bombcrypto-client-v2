using PvpSchedule;

using Scenes.PvpModeScene.Scripts;

using UnityEngine;
using UnityEngine.UI;

namespace Tournament {
    public class TournamentToggle : MonoBehaviour {
        [SerializeField]
        private TournamentTab tab;

        [SerializeField]
        private Image background;
        
        private System.Action<TournamentTab> _onSelectCallback;

        public void SetSelectCallback(System.Action<TournamentTab> callback) {
            _onSelectCallback = callback;
        }
        
        public void SetSelected(bool isSelect = true) {
            OnSelect(isSelect);
        }

        private void OnSelect(bool select) {
            background.color = select ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1);
            if (!select) {
                return;
            }
            _onSelectCallback?.Invoke(tab);
        }
    }
}