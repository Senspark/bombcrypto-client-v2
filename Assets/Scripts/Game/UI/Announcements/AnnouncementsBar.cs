using System;
using System.Collections.Generic;

using App;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class AnnouncementsBar : MonoBehaviour {
        [SerializeField]
        private Image announcementsBar;
        
        [SerializeField]
        private Text contentText;

        private static AnnouncementsBar _instance;
        private INewsManager _newsManager;
        private AnnouncementsMessage _currentAnnouncements;

        private bool _isShowAnnouncements;
        private bool _isWaitForNextShow;
        private float _announceInterval;
        private float _waitAnnounceProgress;
        private float _positionX;
        private float _lenText;
        private float _timeStep;

        private void Awake() {
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            var enableNews = featureManager.EnableNews;
            if (!enableNews) {
                gameObject.SetActive(false);
                return;
            }
            
            if (_instance != null) {
                _instance.SyncAnnouncements();
                Destroy(gameObject);
            } else {
                _instance = this;
                _isShowAnnouncements = false;
                _isWaitForNextShow = false;
                _newsManager = ServiceLocator.Instance.Resolve<INewsManager>();
                DontDestroyOnLoad(gameObject);
                SyncAnnouncements();
            }
        }

        private void SyncAnnouncements() {
            UniTask.Void(async () => {
                await _newsManager.SyncData();
                OnAfterSyncAnnouncements();
            });
        }

        private void OnAfterSyncAnnouncements() {
            var list = _newsManager.GetAnnouncements();
            var newAnnouncements = GetValidAnnouncements(list);
            if (newAnnouncements == null) {
                return;
            }

            var resetScroll = false;
            if (_currentAnnouncements == null || _currentAnnouncements != newAnnouncements) {
                _currentAnnouncements = newAnnouncements;
                resetScroll = true;
            }
            _isShowAnnouncements = _currentAnnouncements != null;
            if (resetScroll) {
                StartScrolling();
            }
        }

        private static AnnouncementsMessage GetValidAnnouncements(List<AnnouncementsMessage> list) {
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            foreach (var iter in list) {
                if (iter.DateStart <= currentTime && currentTime <= iter.DateEnd) {
                    return iter;
                }
            }
            return null;
        }
        
        private void Update() {
            var delta = Time.deltaTime;
            if (!_isShowAnnouncements) {
                return;
            }
            if (_isWaitForNextShow) {
                _waitAnnounceProgress += delta;
                if (!(_waitAnnounceProgress >= _announceInterval)) {
                    return;
                }
                StartScrolling();
            } else {
                HorizontalScrolling(delta);
            }
        }

        private void StartScrolling() {
            contentText.text = _currentAnnouncements.Content;
            _announceInterval = _currentAnnouncements.Repeat;
            _positionX = Screen.width;
            _lenText = contentText.preferredWidth;
            
            announcementsBar.gameObject.SetActive(true);
            var fadeOut = announcementsBar.DOFade(0.0f, 0.0f);
            var fadeIn = announcementsBar.DOFade(0.6f, 0.5f).SetEase(Ease.InOutSine);
            DOTween.Sequence()
                .Append(fadeOut)
                .AppendCallback(() => announcementsBar.gameObject.SetActive(true))
                .Append(fadeIn);
            _isWaitForNextShow = false;
            SetContentPositionX();
        }

        private void StopScrolling() {
            var fadeOut = announcementsBar.DOFade(0.0f, 0.5f).SetEase(Ease.InOutSine);
            DOTween.Sequence()
                .Append(fadeOut)
                .AppendCallback(() => announcementsBar.gameObject.SetActive(false));
            _waitAnnounceProgress = 0;
            _isWaitForNextShow = true;
        }
        
        private void HorizontalScrolling(float delta) {
            _timeStep += delta;
            if (_timeStep < 0.002f) {
                return;
            }
            
            _timeStep = 0;
            _positionX -= 1.5f;
            if (_positionX <= 0 - _lenText) {
                StopScrolling();
                return;
            }
            SetContentPositionX();
        }

        private void SetContentPositionX() {
            var trans = contentText.transform;
            var position = trans.localPosition;
            position.x = _positionX;
            trans.localPosition = position;
        }
    }
}
