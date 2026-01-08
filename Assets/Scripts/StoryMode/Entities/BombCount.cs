using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Engine.Entities {
    public class BombCount : Bomb {
        [SerializeField]
        private TMP_Text count;

        protected override void OnUpdate(float delta) {
            base.OnUpdate(delta);
            count.text = $"{CountDown.TimeRemain:0}";
        }
    }
}