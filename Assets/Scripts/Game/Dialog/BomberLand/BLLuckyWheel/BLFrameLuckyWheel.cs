using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using App;

using Constant;

using Data;

using Senspark;

using Game.Dialog.BomberLand.BLGacha;

using Scenes.StoryModeScene.Scripts;

using UnityEngine;

using Random = UnityEngine.Random;

namespace Game.Dialog.BomberLand.BLLuckyWheel {
    public class BLFrameLuckyWheel : MonoBehaviour {
        [SerializeField]
        private BLGachaRes gachaRes;

        [SerializeField]
        private BLPathLuckyWheel pathLuckyWheel;

        [SerializeField]
        private GameObject rot;

        [SerializeField]
        public AnimationCurve animationCurve;

        [SerializeField]
        private GameObject btSpin;

        private ISoundManager _soundManager;
        private IInputManager _inputManager;
        private BLPathLuckyWheel[] _paths;
        private bool _spinning;
        private float _anglePerItem;
        private int _idxSpinTo;
        private Action _onSpinFinish;
        private int _idSoundSpin = -1;
        private bool _isGetMysteryBox = false;
        public bool IsGetMysteryBox => _isGetMysteryBox;

        protected void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
            btSpin.SetActive(false);
            const int numSlot = 6;
            var items = (TypeBLLuckyWheel[])Enum.GetValues(typeof(TypeBLLuckyWheel));
            var paths = new BLPathLuckyWheel[numSlot];
            paths[0] = pathLuckyWheel;
            for (var i = 1; i < numSlot; i++) {
                var p = Instantiate(pathLuckyWheel, pathLuckyWheel.transform.parent, false);
                p.transform.eulerAngles = new Vector3(0, 0, 60 * i);
                paths[i] = p;
            }
            for (var idx = 0; idx < numSlot; idx++) {
                if (idx >= items.Length) {
                    continue;
                }
                var path = paths[idx];
                path.ForceSetUnknown();
            }
            _paths = paths;
            _spinning = false;
            _anglePerItem = 360.0f / numSlot;
        }

        public async void SetDataSpin(LuckyWheelRewardData[] data, IBonusRewardAdventureV2 reward) {
            _isGetMysteryBox = false;
            var idxSpinTo = -1;
            for (var idx = 0; idx < _paths.Length; idx++) {
                var path = _paths[idx];
                if (idx >= data.Length) {
                    path.SetNothing();
                    continue;
                }
                var d = data[idx];
                if (d.ItemId < 0) {
                    path.SetNothing();
                } else {
                    path.SetQuantity(d.Quantity);
                    var sprite = await gachaRes.GetSpriteByItemId(d.ItemId);
                    path.SetIcon(sprite);
                }
                if (reward.RewardCode != d.RewardCode) {
                    continue;
                }
                idxSpinTo = idx;
                if (d.ItemId == (int)GachaChestProductId.MysteryBox) {
                    _isGetMysteryBox = true;
                }
            }
            if (idxSpinTo == -1) {
                throw new Exception($"Could not find reward: {reward.RewardCode}");
            }
            _idxSpinTo = idxSpinTo;
            btSpin.SetActive(true);
        }

        public void SetOnSpinFinish(Action onSpinFinish) {
            _onSpinFinish = onSpinFinish;
        }

        public void OnBtSpin() {
            if (_spinning) {
                return;
            }
            btSpin.SetActive(false);
            var duration = 4.0f;
            var roundSpin = 4.0f;
            var maxAngle = 360 * roundSpin + (_idxSpinTo * _anglePerItem);
            _idSoundSpin = _soundManager.PlayLoopSound(Audio.WheelSpin);
            StartCoroutine(SpinTheWheel(duration, maxAngle));
        }

        private IEnumerator SpinTheWheel(float time, float maxAngle) {
            var rotTransform = rot.transform;
            _spinning = true;

            var timer = 0.0f;
            var startAngle = rotTransform.eulerAngles.z;
            maxAngle -= startAngle;

            while (timer < time) {
                //to calculate rotation
                var angle = maxAngle * animationCurve.Evaluate(timer / time);
                rotTransform.eulerAngles = new Vector3(0.0f, 0.0f, -(angle + startAngle));
                timer += Time.deltaTime;
                yield return 0;
            }

            rotTransform.eulerAngles = new Vector3(0.0f, 0.0f, -(maxAngle + startAngle));

            if (_idSoundSpin != -1) {
                _soundManager.StopLoopSound(_idSoundSpin);
                _idSoundSpin = -1;
            }
            _soundManager.PlaySound(Audio.WheelStop);

            yield return new WaitForSeconds(1.5f);

            _spinning = false;
            _onSpinFinish?.Invoke();
        }

        private void Update() {
            if (_inputManager.ReadButton(_inputManager.InputConfig.Enter)) {
                OnBtSpin();
            }
        }
    }
}