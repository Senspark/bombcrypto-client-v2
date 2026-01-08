using Engine.UI;

using Utils;

using UnityEngine;
using UnityEngine.EventSystems;

namespace StoryMode.UI {
    public class CombineJoystick : Joystick, IJoystick {
        private readonly ClickHelper _clickHelper = new ClickHelper();

        private JoystickType _joystickType = JoystickType.Fixed;
        private Vector2 _fixedPosition = Vector2.zero;
        private float _moveThreshold;
        private int _pointerId;

        public bool IsTapped { get; set; }
        public bool IsDoubleTapped { get; set; }

        public bool DoubleTapEnabled {
            get => _clickHelper.DoubleClickEnabled;
            set => _clickHelper.DoubleClickEnabled = value;
        }

        public bool IsPressing { get; private set; }

        private void SetMode(JoystickType type) {
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
            _moveThreshold = 1; // 0.1f;
            _joystickType = JoystickType.Dynamic;
            _pointerId = -1;
        }

        protected override void Start() {
            base.Start();
            _fixedPosition = background.position;
            SetMode(_joystickType);
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
                var position = background.position;
                var offsetX = eventData.position.x - position.x;
                var offsetY = eventData.position.y - position.y;
                var offset = new Vector2(offsetX, offsetY);
                offset.Normalize();

                position = eventData.position - offset;
                background.position = position;
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
        }

        protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam) {
            if (magnitude > _moveThreshold) {
                var difference = normalised * (magnitude - _moveThreshold) * radius;
                background.anchoredPosition += difference;
            }
            base.HandleInput(magnitude, normalised, radius, cam);
        }
    }
}