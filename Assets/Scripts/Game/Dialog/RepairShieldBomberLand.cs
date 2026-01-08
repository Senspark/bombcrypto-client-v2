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

        protected override void UpdateUI() {
            base.UpdateUI();
            var hero = ResetThisHero;
            if (hero != null) {
                senBtn.interactable = Controller.CanProcessUsingSen(hero);
                senAmountTxt.text = Controller.SenFeeRepairShield(hero).ToString();
            } else {
                senBtn.interactable = false;
                senAmountTxt.text = "--";
            }
        }

        public async void OnResetShieldBySenBtnClicked() {
            SoundManager.PlaySound(Audio.Tap);
            var hero = ResetThisHero;

            void OnYes() {
                senBtn.interactable = false;
                var waiting = new WaitingUiManager(Canvas);
                waiting.Begin();
                UniTask.Void(async () => {
                    try {
                        var newData = await Controller.ProcessUsingSen(hero);
                        OnResetCompleted(newData);
                    } catch (Exception e) {
                        senBtn.interactable = Controller.CanProcessUsingSen(hero);
                        if (e is ErrorCodeException) {
                            DialogError.ShowError(Canvas, e.Message);    
                        } else {
                            DialogForge.ShowError(Canvas, e.Message);
                        }
                    } finally {
                        waiting.End();
                    }
                });
            }

            var fee = Controller.SenFeeRepairShield(hero);
            var info = _languageManager.GetValue(LocalizeKey.ui_info_buy_repair_shield);
            var str = string.Format(info, fee, "Sen");
            var dialog = await DialogConfirm.Create();
            dialog.SetInfo(str, "Yes", "No", OnYes, null).Show(Canvas);
        }
    }
}