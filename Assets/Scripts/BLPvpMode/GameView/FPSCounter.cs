using System;
using System.Collections.Generic;

using App;

using TMPro;

using UnityEngine;

namespace BLPvpMode.GameView {
    public class FPSCounter : MonoBehaviour {
        [SerializeField]
        private TMP_Text text;

        private readonly Dictionary<int, string> _cachedNumberStrings = new();
        private int[] _frameRateSamples;
        private readonly int _cacheNumbersAmount = 300;
        private readonly int _averageFromAmount = 30;
        private int _averageCounter = 0;
        private int _currentAveraged;

        private void Awake() {
            if (AppConfig.IsProduction) {
                gameObject.SetActive(false);
                return;
            }
            // Cache strings and create array
            {
                for (var i = 0; i < _cacheNumbersAmount; i++) {
                    _cachedNumberStrings[i] = i.ToString();
                }
                _frameRateSamples = new int[_averageFromAmount];
            }
        }

        private void Update() {
            // Sample
            {
                var currentFrame =
                    (int) Math.Round(1f /
                                     Time.smoothDeltaTime); // If your game modifies Time.timeScale, use unscaledDeltaTime and smooth manually (or not).
                _frameRateSamples[_averageCounter] = currentFrame;
            }

            // Average
            {
                var average = 0f;

                foreach (var frameRate in _frameRateSamples) {
                    average += frameRate;
                }

                _currentAveraged = (int) Math.Round(average / _averageFromAmount);
                _averageCounter = (_averageCounter + 1) % _averageFromAmount;
            }

            // Assign to UI
            {
                text.text = "FPS:" + (_currentAveraged < _cacheNumbersAmount && _currentAveraged > 0
                    ? _cachedNumberStrings[_currentAveraged]
                    : _currentAveraged < 0
                        ? "< 0"
                        : _currentAveraged > _cacheNumbersAmount
                            ? $"> {_cacheNumbersAmount}"
                            : "-1");
            }
        }
    }
}