using System;
using BLPvpMode.UI;

using Scenes.PvpModeScene.Scripts;
using Scenes.StoryModeScene.Scripts;

using StoryMode.UI;
using UnityEngine;

namespace BomberLand.InGame {
    public enum GuiType {
        Unknown,
        Pve,
        PvpPlay,
        PvpView,
        Testing,
        PadPvpPlay
    }

    [CreateAssetMenu(fileName = "BLGuiRes", menuName = "BomberLand/GuiRes")]
    public class BLGuiResource : ScriptableObject {
        [Serializable]
        public class ResourcePicker {
            public GameObject prefabGui;
            public GameObject prefabInput;
        }

        public SerializableDictionaryEnumKey<GuiType, ResourcePicker> resourceGui;

        public IBLGui CreateBLGui(GuiType type, Transform parent) {
            if (resourceGui.ContainsKey(type)) {
                var prefabGui = resourceGui[type].prefabGui;
                var prefabInput = resourceGui[type].prefabInput;
                switch (type) { 
                    case GuiType.Pve:
                        var guiPve = Instantiate(prefabGui, parent).GetComponent<BLGuiPve>();
                        var inputPve = Instantiate(prefabInput, parent).GetComponent<BLPlayerInputKey>();
                        guiPve.Initialized(inputPve);
                        return guiPve;
                    case GuiType.PvpPlay:
                        var guiPvp = Instantiate(prefabGui, parent).GetComponent<BLGuiPvp>();
                        var inputPvp = Instantiate(prefabInput, parent).GetComponent<BLPlayerInputKey>();
                        guiPvp.Initialized(inputPvp);
                        return guiPvp;
                    case GuiType.PvpView:
                        var guiView = Instantiate(prefabGui, parent).GetComponent<BLGuiPvp>();
                        guiView.Initialized(Instantiate(prefabInput, parent).GetComponent<BLPlayerInputKey>());
                        return guiView;
                    case GuiType.Testing:
                        var guiTesting = Instantiate(prefabGui, parent).GetComponent<BLGuiPve>();
                        var inputTesting = Instantiate(prefabInput, parent).GetComponent<BLPlayerInputKey>();
                        guiTesting.Initialized(inputTesting);
                        return guiTesting;
                    case GuiType.PadPvpPlay:
                        var padGuiPvp = Instantiate(prefabGui, parent).GetComponent<BLGuiPvp>();
                        var padInputPvp = Instantiate(prefabInput, parent).GetComponent<BLPlayerInputKey>();
                        padGuiPvp.Initialized(padInputPvp);
                        return padGuiPvp;
                }
            }
            return null;
        }
    }
}