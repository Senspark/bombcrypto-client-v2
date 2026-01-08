using Engine.UI;

using Utils;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace StoryMode.UI {
    public class JoyPad : Joystick, IJoystick {
        public Image BlinkButton => blinkButton;
        public RectTransform Background => background;

        private readonly ClickHelper _clickHelper = new ClickHelper();

        private JoystickType _joystickType = JoystickType.Fixed;

        private Vector2 _fixedPosition = Vector2.zero;

        // private float _moveThreshold;
        private int _pointerId;

        public bool IsTapped { get; set; }
        public bool IsDoubleTapped { get; set; }

        public bool DoubleTapEnabled {
            get => _clickHelper.DoubleClickEnabled;
            set => _clickHelper.DoubleClickEnabled = value;
        }

        public bool IsPressing { get; private set; }

        public void SetMode(JoystickType type) {
            _joystickType = type;
        }

        private void Awake() {
            _clickHelper.SingleClickThreshold = 0.25f;
            _clickHelper.DoubleClickThreshold = 0.25f;
            _clickHelper.DoubleClickEnabled = false;
            _clickHelper.OnSingleClick(() => {
                if (Direction == Vector2.zero) {
                    IsTapped = true;
                }
            });
            _clickHelper.OnDoubleClick(() => IsDoubleTapped = true);
            // _moveThreshold = 1; // 0.1f;
            _joystickType = JoystickType.Fixed;
            _pointerId = -1;
        }

        protected override void Start() {
            base.Start();
            _fixedPosition = background.position;
            SetMode(_joystickType);
        }

        protected override void OnDisable() {
            base.OnDisable();
            UpdateButton();
        }

        public override void OnPointerDown(PointerEventData eventData) {
            if (_pointerId != -1) {
                // Fix khi image khác che joystick làm OnPointerUp không được gọi
                // Có thể có sự thay đổi của unity về các event Pointer này nên h cần thêm 2 dòng này để fix
                _pointerId = eventData.pointerId;
                OnPointerUp(eventData);
            }
            _pointerId = eventData.pointerId;

            if (_joystickType != JoystickType.Fixed) {
                background.position = eventData.position;
            }
            IsPressing = true;
            _clickHelper.Down();
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData) {
            if (_pointerId != eventData.pointerId) {
                return;
            }
            _pointerId = -1;
            background.position = _fixedPosition;

            IsPressing = false;
            _clickHelper.Up();
            _clickHelper.Update();
            base.OnPointerUp(eventData);
            UpdateButton();
        }

        public override void OnDrag(PointerEventData eventData) {
            base.OnDrag(eventData);
            UpdateButton();
        }

        private void UpdateButton() {
            down.SetActive(false);
            up.SetActive(false);
            left.SetActive(false);
            right.SetActive(false);

            if (Direction.x > 0) {
                right.SetActive(true);
            } else if (Direction.x < 0) {
                left.SetActive(true);
            } else if (Direction.y > 0) {
                up.SetActive(true);
            } else if (Direction.y < 0) {
                down.SetActive(true);
            }
        }
    }
}