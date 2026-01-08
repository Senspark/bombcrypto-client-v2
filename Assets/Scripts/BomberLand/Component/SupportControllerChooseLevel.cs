using System;
using System.Collections.Generic;

using App;

using BomberLand.Component;

using Engine.Input;

using Game.Dialog;

using Senspark;

using UnityEngine;

public class SupportControllerChooseLevel : MonoBehaviour {
    [SerializeField]
    private BLAdventureMap map1, map2, map3, map4, map5;

    private IStoryModeManager _storyModeManager;
    private IInputManager _inputManager;
    private IDialogManager _dialogManager;
    private int _currentIndex = 1;
    private int _maxIndex = 25;
    private List<BLAdventureMap> _maps;

    private const int LevelsPerStage = 5;

    private void Awake() {
        if (!AppConfig.IsWebGL()) {
            return;
        }
        _storyModeManager = ServiceLocator.Instance.Resolve<IStoryModeManager>();
        _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
        _dialogManager = ServiceLocator.Instance.Resolve<IDialogManager>();
        _maps = new List<BLAdventureMap> { map1, map2, map3, map4, map5 };
    }

    private async void Start() {
        if (!AppConfig.IsWebGL()) {
            return;
        }
        var adventureLevelDetail = await _storyModeManager.GetAdventureLevelDetail();
        InitializeIndices(adventureLevelDetail.CurrentStage, adventureLevelDetail.MaxLevel,
            adventureLevelDetail.MaxStage);
    }

    private void InitializeIndices(int currentStage, int maxLevel, int maxStage) {
        _currentIndex = CalculateIndex(currentStage, maxLevel);
        _maxIndex = CalculateIndex(maxStage, maxLevel);
    }

    private int CalculateIndex(int stage, int level) {
        return (stage - 1) * LevelsPerStage + level + 1;
    }

    private void Update() {
        if (!AppConfig.IsWebGL()) {
            return;
        }
        if (_dialogManager.IsAnyDialogOpened()) {
            return;
        }
        if (IsRightInputPressed()) {
            IncrementIndex();
        } else if (IsLeftInputPressed()) {
            DecrementIndex();
        }
    }

    private bool IsRightInputPressed() {
        return _inputManager.ReadButton(ControllerButtonName.LStickRight) || Input.GetKeyDown(KeyCode.RightArrow);
    }

    private bool IsLeftInputPressed() {
        return _inputManager.ReadButton(ControllerButtonName.LStickLeft) || Input.GetKeyDown(KeyCode.LeftArrow);
    }

    private void IncrementIndex() {
        _currentIndex++;
        if (_currentIndex > _maxIndex) {
            _currentIndex = _maxIndex;
        } else {
            UpdatePlayerPosition();
        }
    }

    private void DecrementIndex() {
        _currentIndex--;
        if (_currentIndex <= 0) {
            _currentIndex = 1;
        } else {
            UpdatePlayerPosition();
        }
    }

    private void UpdatePlayerPosition() {
        var (currentStage, maxLevel) = ConvertIndexToStageAndLevel(_currentIndex);
        if (currentStage > 0 && currentStage <= _maps.Count) {
            _maps[currentStage - 1].OnLevelButtonCallback?.Invoke(currentStage, maxLevel);
        }
    }

    private (int currentStage, int maxLevel) ConvertIndexToStageAndLevel(int index) {
        int currentStage = (index - 1) / LevelsPerStage + 1;
        int maxLevel = (index - 1) % LevelsPerStage + 1;
        return (currentStage, maxLevel);
    }
}