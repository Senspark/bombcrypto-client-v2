using System;
using App;
using Cysharp.Threading.Tasks;
using Game.Manager;
using Services.Server.Exceptions;
using Share.Scripts.Dialog;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class RepairShieldBomberLand : RepairShieldPolygon {
        [SerializeField]
        private Text senAmountTxt;

        protected override void OnAwake() {
            base.OnAwake();
            //Tắt tính năng repair shield bằng sens
            senBtn.gameObject.SetActive(false);
        }

        public async void OnResetShieldBySenBtnClicked() {
            // removed
        }
    }
}