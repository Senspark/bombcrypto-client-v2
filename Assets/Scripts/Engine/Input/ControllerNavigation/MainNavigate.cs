using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using Engine.Input;
using Engine.Input.ControllerNavigation;
using Engine.Manager;
using Senspark;
using UnityEngine;

public enum NavigateEvent {
    RequestPopupParent,
    ClosePopupParent,
}

public class MainNavigate : MonoBehaviour {
    [SerializeField]
    private ParentNavigate[] parentNavigates;

    private IInputManager _inputManager;

    private IParentNavigate[][] _parentNavigates;

    // Parent đc mở ra từ 1 parent bất kỳ, khi này flow của parent hiện tại sẽ tạm dừng để cho popup parent sử dụng
    private PopupParentNavigate _popUpParent;
    private Vector2Int _currentPosition;
    private IParentNavigate _currentParent;
    private IParentNavigate _tempParent;
    private bool _isInit, _isUsePopupParent;

    [SerializeField]
    private bool canLoopBack = true;

    private Vector2Int _maxXY;

    private void OnEnable() {
        if (!AppConfig.IsWebGL()) {
            return;
        }
        
        if (_isInit) {
            ResetToDefaultPosition();
        }
        EventManager<PopupParentNavigate>.Add(NavigateEvent.RequestPopupParent, RequestPopupParent);
        EventManager.Add(NavigateEvent.ClosePopupParent, ClosePopupParent);
        EventManager<InputType>.Add(InputEvent.OnChangeInput, OnChangeInput);
    }

    private async void Start() {
        if (!AppConfig.IsWebGL()) {
            return;
        }
        
        // Đợi 2 frame vì sau đó rectTransform mới được unity set position đúng
        await UniTask.DelayFrame(2);
        if (parentNavigates == null || parentNavigates.Length == 0) {
            Debug.LogWarning("No parent navigates found");
            return;
        }
        _isInit = true;

        FirstSetup();
        SecondSetup();
        ThirdSetup();
    }

    private void OnDisable() {
        if (!AppConfig.IsWebGL()) {
            return;
        }
        EventManager<PopupParentNavigate>.Remove(NavigateEvent.RequestPopupParent, RequestPopupParent);
        EventManager.Remove(NavigateEvent.ClosePopupParent, ClosePopupParent);
        EventManager<InputType>.Remove(InputEvent.OnChangeInput, OnChangeInput);
    }

    private bool OnMoveOut(Vector2Int dir, Vector2Int preChildPosition) {
        IParentNavigate newParent = null;
        Vector2Int newPosition = _currentPosition;

        if (_isUsePopupParent)
            return false;
        
        var maxLoop = Mathf.Max(_maxXY.x, _maxXY.y);

        while (maxLoop-- > 0) {
            newPosition += dir;
            if (!IsInRange(newPosition)) {
                if (canLoopBack) {
                    LoopBack(dir, preChildPosition);
                }
                return false;
            }

            // Có item rồi nên dừng lại update item mới
            if (IsHaveItem(newPosition)) {
                newParent = _parentNavigates[newPosition.x][newPosition.y];
                UpdateNewParent(newParent, newPosition, dir, preChildPosition);
                return true;
            }
        }
        return false;
    }

    private void UpdateNewParent(IParentNavigate newParent, Vector2Int newPosition, Vector2Int dir,
        Vector2Int preChildPosition) {
        _currentParent = newParent;
        _currentPosition = newPosition;
        _currentParent.Navigate(OnMoveOut, dir, preChildPosition);
    }

    private bool IsInRange(Vector2Int position) {
        return position.x >= 0 && position.x < _parentNavigates.Length && position.y >= 0 &&
               position.y < _parentNavigates[0].Length;
    }

    private bool IsValidRange(Vector2Int position) {
        return position.x >= 0 && position.y >= 0;
    }

    private bool IsHaveItem(Vector2 position) {
        return _parentNavigates[(int)position.x][(int)position.y] != null;
    }

    private void OnChangeInput(InputType type) {
        if (type == InputType.Controller) {
            _currentParent?.SetActiveCurrentHighlight(true);
            return;
        }
        if (type == InputType.Keyboard) {
            _currentParent?.SetActiveCurrentHighlight(false);
        }
    }

    private void Update() {
        if (_inputManager == null || !AppConfig.IsWebGL() || !_isInit)
            return;

        if (_inputManager.ReadButton(ControllerButtonName.LStickLeft)) {
            _currentParent.Process(Vector2Int.left);
            return;
        }
        if (_inputManager.ReadButton(ControllerButtonName.LStickRight)) {
            _currentParent.Process(Vector2Int.right);
            return;
        }
        if (_inputManager.ReadButton(ControllerButtonName.LStickUp)) {
            _currentParent.Process(Vector2Int.up);
            return;
        }
        if (_inputManager.ReadButton(ControllerButtonName.LStickDown)) {
            _currentParent.Process(Vector2Int.down);
            return;
        }

        if (_inputManager.ReadJoystick(_inputManager.InputConfig.Enter)) {
            _currentParent.Choose();
        }
        if (_isUsePopupParent) {
            if (_inputManager.ReadButton(_inputManager.InputConfig.Back)) {
                _popUpParent.ClosePopup();
            }
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            _currentParent.Process(Vector2Int.left);
            return;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            _currentParent.Process(Vector2Int.right);
            return;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            _currentParent.Process(Vector2Int.up);
            return;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            _currentParent.Process(Vector2Int.down);
            return;
        }

        if (Input.GetKeyDown(KeyCode.O)) {
            _currentParent.Choose();
        }
#endif
    }

    private void RequestPopupParent(PopupParentNavigate popupParent) {
        if (_popUpParent != null) {
            Debug.LogWarning("Popup parent is already active");
            return;
        }
        _tempParent = _currentParent;
        _currentParent = popupParent;
        _popUpParent = popupParent;
        _isUsePopupParent = true;
    }

    private void ClosePopupParent() {
        if (_popUpParent == null) {
            Debug.LogWarning("No popup parent to close");
            return;
        }
        _currentParent = _tempParent;
        _popUpParent = null;
        _isUsePopupParent = false;
    }

    private void LoopBack(Vector2Int dir, Vector2Int preChildPosition) {
        var loopBackPosition = GetStartLoopBackPosition(dir);
        var incrementDirection = GetIncrementDirection(dir);

        while (!IsInRange(loopBackPosition) && IsValidRange(loopBackPosition)) {
            loopBackPosition += incrementDirection;
        }
        while (IsInRange(loopBackPosition) && !IsHaveItem(loopBackPosition) && IsValidRange(loopBackPosition)) {
            loopBackPosition += incrementDirection;
        }
        if (!IsValidRange(loopBackPosition) || !IsInRange(loopBackPosition) || !IsHaveItem(loopBackPosition)) {
            // try opposite direction
            incrementDirection *= -1;
            loopBackPosition = GetStartLoopBackPosition(dir);
            var maxTry = Mathf.Max(_maxXY.x, _maxXY.y);
            while (maxTry-- > 0 && !IsInRange(loopBackPosition) && IsValidRange(loopBackPosition)) {
                loopBackPosition += incrementDirection;
            }
            maxTry = Mathf.Max(_maxXY.x, _maxXY.y);
            while (maxTry-- > 0 && IsInRange(loopBackPosition) && !IsHaveItem(loopBackPosition) && IsValidRange(loopBackPosition)) {
                loopBackPosition += incrementDirection;
            }
        }
        if (IsValidRange(loopBackPosition) && IsInRange(loopBackPosition) && !IsHaveItem(loopBackPosition)) {
            loopBackPosition = GetDefaultPosition(dir);
        }

        _currentParent.ForceOut();
        _currentPosition = loopBackPosition;
        _currentParent = _parentNavigates[_currentPosition.x][_currentPosition.y];
        _currentParent.Navigate(OnMoveOut, dir, preChildPosition);
    }

    private Vector2Int GetStartLoopBackPosition(Vector2Int dir) {
        if (dir == Vector2Int.right) {
            return new Vector2Int(0, _currentPosition.y);
        }
        if (dir == Vector2Int.left) {
            return new Vector2Int(_maxXY.x, _currentPosition.y);
        }
        if (dir == Vector2Int.up) {
            return new Vector2Int(_currentPosition.x, 0);
        }
        if (dir == Vector2Int.down) {
            return new Vector2Int(_currentPosition.x, _maxXY.y);
        }
        return Vector2Int.zero;
    }

    private Vector2Int GetIncrementDirection(Vector2Int dir) {
        if (dir == Vector2Int.right || dir == Vector2Int.left) {
            return Vector2Int.down;
        }
        if (dir == Vector2Int.up || dir == Vector2Int.down) {
            return Vector2Int.left;
        }
        return Vector2Int.zero;
    }

    private Vector2Int GetDefaultPosition(Vector2Int dir) {
        if (dir == Vector2Int.right || dir == Vector2Int.up) {
            return Vector2Int.zero;
        }
        if (dir == Vector2Int.left) {
            return _maxXY;
        }
        if (dir == Vector2Int.down) {
            return new Vector2Int(0, _maxXY.y);
        }
        return Vector2Int.zero;
    }

    private void ResetToDefaultPosition() {
        _currentParent.ForceOut();
        _currentPosition = Vector2Int.zero;
        _currentParent = parentNavigates[0];
        _currentParent.Navigate(OnMoveOut, Vector2Int.zero, Vector2Int.zero);
        _popUpParent = null;
        _isUsePopupParent = false;
        OnChangeInput(_inputManager.InputType);
    }

    #region Setup

    private void FirstSetup() {
        foreach (var parent in parentNavigates) {
            parent.Init();
        }

        _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
    }

    // init các data cần thiết
    private void SecondSetup() {
        // Find the maximum x and y positions
        int maxX = 0;
        int maxY = 0;
        foreach (var parentNavigate in parentNavigates) {
            var position = parentNavigate.Position;
            if (position.x > maxX) maxX = position.x;
            if (position.y > maxY) maxY = position.y;
        }

        // Initialize the _parentNavigates array with the found dimensions
        _parentNavigates = new IParentNavigate[maxX + 1][];
        for (int i = 0; i <= maxX; i++) {
            _parentNavigates[i] = new IParentNavigate[maxY + 1];
        }

        // Check for duplicate positions and assign the parentNavigate objects to their respective positions
        var positionsSet = new HashSet<Vector2>();
        foreach (var parentNavigate in parentNavigates) {
            var position = parentNavigate.Position;
            if (!positionsSet.Add(position)) {
                Debug.LogWarning($"Duplicate position found at ({position.x}, {position.y})");
                continue;
            }
            _parentNavigates[(int)position.x][(int)position.y] = parentNavigate;
        }
        _maxXY = new Vector2Int(_parentNavigates.Length - 1, _parentNavigates[0].Length - 1);
    }

    // Xác định vị trí ban đầu
    private void ThirdSetup() {
        IParentNavigate selectedParent = null;
        Vector2Int selectedPosition = Vector2Int.zero;

        foreach (var parentNavigate in parentNavigates) {
            var position = parentNavigate.Position;
            if (selectedParent == null ||
                position.y > selectedPosition.y ||
                (position.y == selectedPosition.y && position.x < selectedPosition.x)) {
                selectedParent = parentNavigate;
                selectedPosition = position;
            }
        }

        if (selectedParent != null) {
            _currentParent = selectedParent;
            _currentPosition = selectedPosition;
            _currentParent.Navigate(OnMoveOut, Vector2Int.zero, Vector2Int.zero);
        } else {
            Debug.LogWarning("No valid parent navigate found");
        }
        OnChangeInput(_inputManager.InputType);
    }

    #endregion

    #region Editor

#if UNITY_EDITOR
    [Button]
    private void FindAndAddParentNavigates() {
        parentNavigates = GetComponentsInChildren<ParentNavigate>();
        Debug.Log("ParentNavigates found and added.");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    #endregion
}