using App;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class GameModeButton : MonoBehaviour {
        [SerializeField]
        private Image img;
    
        [SerializeField]
        private Button btn;
    
        [SerializeField]
        protected GameObject lockImg;

        [SerializeField]
        private LocalizeText lockTxt;

        [SerializeField]
        private GameModeType gameModeType;
        
        [SerializeField]
        private bool lockThisMode;

        private ILanguageManager _languageManager;

        private void Awake() {
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            if (lockThisMode) {
                SetLock(true, LocalizeKey.ui_coming_soon);
                return;
            }
            SetLock(false);
            Init();
        }

        protected virtual void Init() { }

        protected void SetLock(bool locked, LocalizeKey lockText) {
            var text = _languageManager.GetValue(lockText);
            SetLock(locked, text);
        }
        
        protected void SetLock(bool locked, string lockText) {
            SetLock(locked);
            if (locked) {
                lockTxt.SetNewText($"<b>{lockText}</b>");
            }
        }
        
        protected void SetLock(bool locked) {
            btn.enabled = !locked;
            img.color = locked ? Color.gray : Color.white;
            lockImg.SetActive(locked);
            lockTxt.enabled = locked;
        }
    }
}