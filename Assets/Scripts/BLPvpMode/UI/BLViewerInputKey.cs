using System;
using BLPvpMode.Data;
using PvpMode.Manager;
using StoryMode.UI;
using UnityEngine;
using UnityEngine.UI;

namespace BLPvpMode.UI {
    public class BLViewerInputKey : MonoBehaviour, IBLInputKey {
        public void SetupButtons(BoosterStatus boosterStatus, PlayerInputCallback callback) {
            throw new NotImplementedException();
        }

        public void UpdateButtons(BoosterStatus boosterStatus, float coolDown) {
            throw new NotImplementedException();
        }

        public void ResetButtons(BoosterStatus boosterStatus) {
            throw new NotImplementedException();
        }

        public Image GetBlinkButton() {
            throw new NotImplementedException();
        }

        public Vector2 GetDirection() {
            throw new NotImplementedException();
        }

        public void CheckInputKeyDown() {
            throw new NotImplementedException();
        }

        public void OnBombButtonClicked() {
            throw new NotImplementedException();
        }

        public void OnShieldButtonClicked() {
            throw new NotImplementedException();
        }

        public void OnKeyButtonClicked() {
            throw new NotImplementedException();
        }

        public void OnBackButtonClicked() {
            throw new NotImplementedException();
        }

        public void InitEmojiButtons(int[] itemId, Action<int> onEmojiChoose) {
            throw new NotImplementedException();
        }
        
        public bool GetMuteState() {
            throw new NotImplementedException();
        }

        public void BoosterButtonUsed(BoosterType type, BoosterStatus boosterStatus, Func<bool> isInJailChecker) {
            throw new NotImplementedException();
        }

        public void OnFailedToUseBooster() {
            throw new NotImplementedException();
        }

        public void OnPlayerInJail(BoosterStatus boosterStatus) {
            throw new NotImplementedException();
        }

        public void OnPlayerEndInJail(BoosterStatus boosterStatus) {
            throw new NotImplementedException();
        }

        public void SetEnableInputPlantBomb(bool isEnable) {
            throw new NotImplementedException();
        }

        public void SetEnableInputShield(bool isEnable) {
            throw new NotImplementedException();
        }

        public void SetEnableInputKey(bool isEnable) {
            throw new NotImplementedException();
        }

        public void SetQuantityBomb(int value) {
            throw new NotImplementedException();
        }

        public void SetVisible(ElementGui element, bool value) {
            throw new NotImplementedException();
        }

        public Transform ButtonBombTransform { get; }
        public Transform ButtonShieldTransform { get; }
        public Transform ButtonKeyTransform { get; }
        public JoyPad JoyPad { get; }
    }
}