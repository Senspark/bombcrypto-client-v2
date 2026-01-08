using System;
using System.Collections.Generic;
using Constant;
using Cysharp.Threading.Tasks;
using Data;
using Game.UI;
using Scenes.AltarScene.Scripts;
using Scenes.MarketplaceScene.Scripts;
using Share.Scripts.Utils;
using UnityEngine;
using UnityEngine.Serialization;

public enum TaskGoAction {
    Adventure,
    MainMenu,
    Marketplace,
    AltarGrind,
    MainMenuHero,
}

[CreateAssetMenu(fileName = "DailyTaskConfig", menuName = "BomberLand/DailyTaskConfig")]
public class DailyTaskConfig : ScriptableObject
{
    [Serializable]
    public class TaskConfig {
        public Sprite taskIcon;
        public TaskGoAction taskGoAction;
    }
    
    public SerializableDictionary<int, TaskConfig> resource;

    public Sprite GetTaskIconById(int id) {
        if (resource.ContainsKey(id)) {
            return resource[id].taskIcon;
        }
        return null;
    }
    
    public Action GetTaskGoActionById(int id) {
        if (resource.ContainsKey(id)) {
            return GetTaskGoAction(resource[id].taskGoAction);
        }
        return null;
    }

    private Action GetTaskGoAction(TaskGoAction index) {
        Action action = null;
        switch (index) {
            case TaskGoAction.Adventure:
                action = () => {
                    const string sceneName = "AdventureMenuScene";
                    SceneLoader.LoadSceneAsync(sceneName).Forget();
                };
                break;
            case TaskGoAction.MainMenu:
                action = () => {
                    
                };
                break;
            case TaskGoAction.Marketplace:
                action = () => {
                    // Default tab is Heroes
                    MarketplaceScene.LoadScene();
                };
                break;
            case TaskGoAction.AltarGrind:
                action = () => {
                    void OnLoaded(GameObject obj) {
                        var altarScene = obj.GetComponent<AltarScene>();
                        altarScene.SetDefaultTab(BLTabType.Grind);
                    }
                    const string sceneName = "AltarScene";
                    SceneLoader.LoadSceneAsync(sceneName, OnLoaded).Forget();
                };
                break;
            case TaskGoAction.MainMenuHero:
                action = () => {
                    const string sceneName = "HeroesScene";
                    SceneLoader.LoadSceneAsync(sceneName).Forget();
                };
                break;
        }
        return action;
    }
}
