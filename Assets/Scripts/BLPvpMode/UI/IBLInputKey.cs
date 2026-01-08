using System;
using BLPvpMode.Data;
using PvpMode.Manager;
using StoryMode.UI;
using UnityEngine;
using UnityEngine.UI;

namespace BLPvpMode.UI {
    public struct PlayerInputCallback {
        public Action<BoosterType> OnBoosterButtonClicked;
        public Action OnBombButtonClicked;
        public Action OnBackButtonClicked;
    }

    public static class KeyBoardInputDefine {
        public const KeyCode FindMatch = KeyCode.Space;
        public const KeyCode Ready = KeyCode.Space;
        public const KeyCode PlantBomb = KeyCode.Space;
        public const KeyCode PlayPvp = KeyCode.Space;
        public const KeyCode LuckyWheel = KeyCode.Space;
        public const KeyCode Next = KeyCode.Space;
        public const KeyCode UseShield = KeyCode.D;
        public const KeyCode UseKey = KeyCode.D;
        public const KeyCode PlayPve = KeyCode.Space;
        public const KeyCode Login = KeyCode.Return;
        public const KeyCode LoginKeyPad = KeyCode.KeypadEnter;
        public const KeyCode SmallNext = KeyCode.N;
        public const KeyCode Emoji = KeyCode.E;
    }
    //Dùng tạm các biến này đến khi có design chính thức 
    //sẽ chuyển toàn bộ sang InputConfigData
    public static class ControllerInputDefine {
        public const string FindMatch = "R1";//R2
        public const string Ready = "R1";//R2
        public const string PlantBomb = "R1";//R2
        public const string PlayPvp = "R1";//R2
        public const string LuckyWheel = "R1";//R2
        public const string Next = "R1";//R2
        public const string UseShield = "L1";//L2
        public const string UseKey = "L1";//L2
        public const string PlayPve = "R1";//R2
        public const string Login = "Y";
        public const string LoginKeyPad = "B";
        public const string SmallNext = "A";
    }

    public interface IBLInputKey {
        void SetupButtons(BoosterStatus boosterStatus, PlayerInputCallback callback);
        void UpdateButtons(BoosterStatus boosterStatus, float coolDown);
        void ResetButtons(BoosterStatus boosterStatus);
        Image GetBlinkButton();
        Vector2 GetDirection();
        void CheckInputKeyDown();
        void OnBombButtonClicked();
        void OnShieldButtonClicked();
        void OnKeyButtonClicked();
        void OnBackButtonClicked();
        void InitEmojiButtons(int[] itemId, Action<int> onEmojiChoose);
        bool GetMuteState();

        void BoosterButtonUsed(BoosterType type, BoosterStatus boosterStatus,
            Func<bool> isInJailChecker);

        void OnFailedToUseBooster();
        void OnPlayerInJail(BoosterStatus boosterStatus);
        void OnPlayerEndInJail(BoosterStatus boosterStatus);
        void SetEnableInputPlantBomb(bool isEnable);
        void SetEnableInputShield(bool isEnable);
        void SetEnableInputKey(bool isEnable);
        void SetQuantityBomb(int value);
        void SetVisible(ElementGui element, bool value);
        Transform ButtonBombTransform { get; }
        Transform ButtonShieldTransform { get; }
        Transform ButtonKeyTransform { get; }
        JoyPad JoyPad { get; }
    }
}