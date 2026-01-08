using System;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game.UI {
    public class ProgressBar : MonoBehaviour {
        [SerializeField]
        private Image progressBar;

        [SerializeField]
        private float maxLength;

        [SerializeField]
        private float minLength;

        public void SetProcess(float t) {
            t = Mathf.Clamp01(t);
            if (t == 0) {
                progressBar.enabled = false;
                return;
            }

            progressBar.enabled = true;
            var rectTransform = progressBar.rectTransform;
            // 4 corner stretch
            // Left rectTransform.offsetMin.x;
            // Right rectTransform.offsetMax.x;
            // Top rectTransform.offsetMax.y;
            // Bottom rectTransform.offsetMin.y;
            
            // Change Right only
            var size = rectTransform.sizeDelta;
            size.x = Mathf.Clamp(t * maxLength, minLength, maxLength);
            rectTransform.sizeDelta = size;
        }
    }
}