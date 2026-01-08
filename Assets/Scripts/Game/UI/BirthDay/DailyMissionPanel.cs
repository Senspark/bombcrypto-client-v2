using System;

using App;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class DailyMissionPanel : MonoBehaviour {
        [SerializeField]
        private GameObject check;

        [SerializeField]
        private Text missionName;

        [SerializeField]
        private Text completedRequest;

        [SerializeField]
        private Button buttonClaim;

        [SerializeField]
        private GameObject normalButton;

        [SerializeField]
        private GameObject disableButton;

        [SerializeField]
        private GameObject claimedButton;

        public string MissionCode { get; private set; }
        
        private Action<DailyMissionPanel> _onClaimButtonClicked;

        public void InitPanel(IDailyMission mission, Action<DailyMissionPanel> onClaimButtonClicked) {
            MissionCode = mission.MissionCode;
            _onClaimButtonClicked = onClaimButtonClicked;
            missionName.text = GetMissionName(mission);
            completedRequest.text = $"({mission.CompletedTimes}/{mission.RequestTimes})";
            buttonClaim.interactable = mission.Claimable;
            var missionDone = mission.CompletedTimes >= mission.RequestTimes;
            check.SetActive(missionDone);
            if (missionDone) {
                disableButton.SetActive(false);
                normalButton.SetActive(mission.Claimable);
                claimedButton.SetActive(!mission.Claimable);
            } else {
                disableButton.SetActive(true);
                normalButton.SetActive(false);
                claimedButton.SetActive(false);
            }
        }

        private string GetMissionName(IDailyMission mission) {
            var result = mission.Mission switch {
                "REPAIR_SHIELD" => "Complete Repair Shield",
                "COMPLETE_PVP_MATCH" => "Play Battle Mode",
                "WIN_PVP_MATCH" => "Win Battle Mode",
                _ => throw new Exception($"Unknown Mission: {mission.Mission}")
            };
            return result;
        }

        public void OnClaimButtonClicked() {
            _onClaimButtonClicked?.Invoke(this);
        }

        public void UpdateButtonClaimed() {
            disableButton.SetActive(false);
            normalButton.SetActive(false);
            claimedButton.SetActive(true);
            buttonClaim.interactable = false;
        }
    }
}