using System.Collections;
using System.Collections.Generic;
using Engine.Utils;
using Game.Dialog.BomberLand.BLGacha;
using Senspark;
using Services;
using TMPro;
using UnityEngine;

namespace Game.UI {
    public class AnimationPanel : MonoBehaviour {
        [SerializeField]
        private TextMeshProUGUI tooltipText;
        
        [SerializeField]
        private BLTooltip blTooltip;
        
        [SerializeField]
        private ImageAnimation imageAnim;
        
        [SerializeField]
        private BLLoadingAnim blLoadingAnim;

        [SerializeField]
        private TextMeshProUGUI loadingAnimText;

        private bool _isProcessing;
        private Coroutine _coroutine;

        private List<string> _processTextList = new List<string>() {
            "Processing transaction",
            "Processing transaction.",
            "Processing transaction..",
            "Processing transaction...",
        };

        private void Awake() {
            _isProcessing = true;
            tooltipText.SetText(blTooltip.GetRandomTooltip());
            var randomAnim = blLoadingAnim.GetRandomAnim();
            imageAnim.StartAni(randomAnim.sprites);

            if (_isProcessing) {
                _coroutine = StartCoroutine(ProcessingAnim());
            } else if (_coroutine != null) {
                StopCoroutine(_coroutine);
            }
        }

        IEnumerator ProcessingAnim() {
            while (_isProcessing) {
                for (int i = 0; i < _processTextList.Count; i++) {
                    if (!_isProcessing)
                        yield break;

                    loadingAnimText.text = _processTextList[i];
                    yield return new WaitForSecondsRealtime(0.5f);
                }
            }
        }
    }
}