using System;
using System.Collections.Generic;
using System.Linq;

using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.Events;

public interface IParentNavigate {
    void Init();
    void Navigate(Func<Vector2Int, Vector2Int, bool> moveOut, Vector2Int dir, Vector2Int preChildPosition);
    void Choose();
    void Process(Vector2Int dir);
    void ForceOut();
    Vector2Int Position { get; }
    void DisableAllHighLight();
    void SetActiveCurrentHighlight(bool value);
}

public interface IParentHelper {
    void Register(List<ChildrenNavigate> children);
}

public class ParentNavigate : MonoBehaviour, IParentNavigate, IParentHelper {
    [SerializeField]
    private Vector2Int position;

    [SerializeField]
    private List<ChildrenNavigate> childrenNavigatesList;

    protected IChildrenNavigate[][] ChildrenNavigates;
    private Func<Vector2Int, Vector2Int, bool> _onMoveOut;
    private Action<Vector2Int> _forceChoose;
    protected IChildrenNavigate CurrentChild;
    protected Vector2Int CurrentPosition;

    private Vector2Int _maxXY;

    public UnityEvent onChoose;

    private bool _isInit;

    public void Init() {
        if (_isInit) {
            return;
        }
        _isInit = true;

        if (childrenNavigatesList != null && childrenNavigatesList.Count > 0) {
            foreach (var child in childrenNavigatesList) {
                child.Init(ForceChoose);
            }
            ReCalculatePositionInMatrix();
        }
    }

    public void Navigate(Func<Vector2Int, Vector2Int, bool> moveOut, Vector2Int dir, Vector2Int preChildPosition) {
        _onMoveOut = moveOut;

        // tính toán vị trí phù hợp của item đầu tiên khi parent này đc navigate đến
        CurrentPosition = CalculateSuitableChild(dir, preChildPosition);
        CurrentChild = ChildrenNavigates[CurrentPosition.x][CurrentPosition.y];
        if (CurrentChild == null) {
            Debug.LogWarning("No current child found");
            return;
        }
        CurrentChild?.Navigate(dir);
    }

    public void Choose() {
        if (CurrentChild == null) {
            Debug.LogWarning("No current child found");
            return;
        }
        CurrentChild?.Choose(onChoose);
    }

    public void Process(Vector2Int dir) {
        if (ChildrenNavigates == null || ChildrenNavigates.Length == 0) {
            Debug.LogWarning("No children navigates found");
            var isGoToNewParent = _onMoveOut?.Invoke(dir, CurrentPosition);
            if (isGoToNewParent.HasValue && isGoToNewParent.Value) {
                CurrentChild?.Out();
                CurrentChild = null;
            }
            return;
        }

        IChildrenNavigate newChild = null;
        Vector2Int newPosition = CurrentPosition;

        while (newChild == null) {
            newPosition += dir;
            if (!IsValidPosition(newPosition) || !IsInRange(newPosition)) {
                LoopBack(dir);
                var isGoToNewParent = _onMoveOut?.Invoke(dir, CurrentPosition);
                if (isGoToNewParent.HasValue && isGoToNewParent.Value) {
                    CurrentChild?.Out();
                    CurrentChild = null;
                }
                return;
            }

            // 1 item có thể cho phép chiếm nhiều ô nên cần check nếu cùng item thì skip qua ô tiếp theo
            if (IsHaveItem(newPosition) && IsTheSameItem(newPosition)) {
                continue;
            }

            // Trường hợp đẹp nhất, vừa check đã có item thì chọn luôn
            if (IsInRange(newPosition) && IsHaveItem(newPosition)) {
                newChild = ChildrenNavigates[newPosition.x][newPosition.y];
                UpdateNewChild(newChild, newPosition, dir);
            } else {
                // Ô dó trống hoặc ko có item nên cần tìm ô gần nhất theo hàng hoặc cột
                var newPos = FindNearestItemPosition(newPosition, dir);
                if (IsInRange(newPos) && IsHaveItem(newPos)) {
                    newChild = ChildrenNavigates[newPos.x][newPos.y];
                    UpdateNewChild(newChild, newPos, dir);
                } else {
                    // Ko có item phù hợp, đây là giới hạn của hàng hoặc cột đó rồi, loop lại hoặc đổi parent
                    LoopBack(dir);
                    var isGoToNewParent = _onMoveOut?.Invoke(dir, CurrentPosition);
                    if (isGoToNewParent.HasValue && isGoToNewParent.Value) {
                        CurrentChild?.Out();
                        CurrentChild = null;
                    }
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Chọn dến item cuối cùng sẽ quay lại item đầu tiên theo cùng hàng hoặc cột
    /// </summary>
    /// <param name="dir"></param>
    protected virtual void LoopBack(Vector2Int dir) {
        //Do nothing
    }

    public void ForceOut() {
        CurrentChild?.Out();
    }

    /// <summary>
    /// Tìm item gần nhất trong cùng 1 parent, nếu ô đó trống thì sẽ tìm ô gần nhất theo hàng hoặc cột đó
    /// Ưu tiên tìm phía trái hoăc xuống truớc
    /// </summary>
    /// <param name="currentPosition"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    private Vector2Int FindNearestItemPosition(Vector2Int currentPosition, Vector2Int dir) {
        var newPosition = currentPosition;
        var offset = dir == Vector2Int.up || dir == Vector2Int.down ? Vector2Int.left : Vector2Int.down;
        while (IsValidPosition(newPosition) && !IsInRange(newPosition)) {
            newPosition += offset;
        }
        while (IsValidPosition(newPosition) && IsInRange(newPosition) && !IsHaveItem(newPosition)) {
            newPosition += offset;
        }
        // try opposite direction
        if (!IsValidPosition(newPosition) || !IsHaveItem(newPosition)) {
            offset = -offset;
            newPosition = currentPosition;
            var maxTry = Mathf.Max(ChildrenNavigates.Length, ChildrenNavigates[0].Length);
            while (IsValidPosition(newPosition) && !IsInRange(newPosition) && maxTry > 0) {
                newPosition += offset;
                maxTry--;
            }
            maxTry = Mathf.Max(ChildrenNavigates.Length, ChildrenNavigates[0].Length);
            while (IsValidPosition(newPosition) && IsInRange(newPosition) && !IsHaveItem(newPosition) && maxTry > 0) {
                newPosition += offset;
                maxTry--;
            }
        }
        return IsValidPosition(newPosition) ? newPosition : currentPosition;
    }

    private void UpdateNewChild(IChildrenNavigate newChild, Vector2Int newPosition, Vector2Int dir) {
        CurrentChild?.Out();
        CurrentChild = newChild;
        CurrentPosition = newPosition;
        CurrentChild?.Navigate(dir);
    }

    protected bool IsInRange(Vector2Int pos) {
        return pos.x >= 0 && pos.x < ChildrenNavigates.Length && pos.y >= 0 &&
               pos.y < ChildrenNavigates[0].Length;
    }

    protected bool IsHaveItem(Vector2Int pos) {
        return ChildrenNavigates[pos.x][pos.y] != null;
    }

    protected bool IsValidPosition(Vector2Int pos) {
        return pos is { x: >= 0, y: >= 0 };
    }

    private bool IsTheSameItem(Vector2Int pos) {
        if (CurrentChild == null) {
            return false;
        }
        return CurrentChild?.GetHashCode() == ChildrenNavigates[pos.x][pos.y].GetHashCode();
    }

    public Vector2Int Position => position;

    public void DisableAllHighLight() {
        foreach (var child in childrenNavigatesList) {
            child.SetActiveHighLight(false);
        }
    }

    public void SetActiveCurrentHighlight(bool value) {
        CurrentChild?.SetActiveHighLight(value);
    }

    /// <summary>
    /// Hàm này support các dialog cần navigate mà các item ko có sẵn ở editor mà tạo trong runtime
    /// Delay 2 frame là vì lúc mới tạo gameobject, component Rectransform chưa
    /// có đc vị trí đúng của nó mà phải đợi 2 frame
    /// </summary>
    /// <param name="children"></param>
    public async void Register(List<ChildrenNavigate> children) {
        await UniTask.DelayFrame(2);

        childrenNavigatesList = children;
        foreach (var child in childrenNavigatesList) {
            child.Init(ForceChoose);
        }
        ReCalculatePositionInMatrix();
        SetupFirstPosition();
        // Chọn luôn item đầu tiên
        CurrentChild?.Navigate(Vector2Int.zero);
    }

    private void ForceChoose(Vector2Int childPositionWantToChoose) {
        if (ChildrenNavigates == null || ChildrenNavigates.Length == 0) {
            Debug.LogWarning("No children navigates found");
            return;
        }

        if (!IsInRange(childPositionWantToChoose)) {
            Debug.LogWarning("Invalid child position");
            return;
        }

        if (!IsHaveItem(childPositionWantToChoose)) {
            Debug.LogWarning("No item found at the child position");
            return;
        }

        CurrentChild?.Out();
        CurrentPosition = childPositionWantToChoose;
        CurrentChild = ChildrenNavigates[CurrentPosition.x][CurrentPosition.y];
        CurrentChild?.Choose(onChoose);
    }

    private Vector2Int CalculateSuitableChild(Vector2Int dir, Vector2Int preChildPos) {
        // Đây là trường hợp lần đầu tiên mở lên, user chưa chọn gì, mặc định chọn item trên cùng bên trái
        if (dir == Vector2Int.zero) {
            SetupFirstPosition();
            return CurrentPosition;
        }
        
        // Parent này chỉ có 1 item, chọn luôn
        if (childrenNavigatesList.Count == 1) {
            CurrentChild = childrenNavigatesList[0];
            CurrentPosition = new Vector2Int(0, 0);
            return CurrentPosition;
        }

        var directionMap = new Dictionary<Vector2Int, (Vector2Int initialPos, Vector2Int increment)> {
            { Vector2Int.up, (new Vector2Int(preChildPos.x, 0), Vector2Int.left) },
            { Vector2Int.down, (new Vector2Int(preChildPos.x, _maxXY.y - 1), Vector2Int.left) },
            { Vector2Int.left, (new Vector2Int(_maxXY.x - 1, preChildPos.y), Vector2Int.down) },
            { Vector2Int.right, (new Vector2Int(0, preChildPos.y), Vector2Int.down) }
        };

        // Check item gần nhất trên 1 hàng hoặc cột đó
        if (directionMap.TryGetValue(dir, out var values)) {
            var (initialPos, increment) = values;

            while (IsValidPosition(initialPos) && !IsInRange(initialPos)) {
                initialPos += increment;
            }
            while (IsValidPosition(initialPos) && IsInRange(initialPos) && !IsHaveItem(initialPos)) {
                initialPos += increment;
            }

            // Hàng hoăc cột đó ko có item nào rồi, check theo hướng mà user chọn
            if (!IsValidPosition(initialPos) || !IsInRange(initialPos) || !IsHaveItem(initialPos)) {
                if (directionMap.TryGetValue(dir, out var values1)) {
                    var (initialPos2, _) = values1;
                    var maxTry = Mathf.Max(ChildrenNavigates.Length, ChildrenNavigates[0].Length);
                    while (IsValidPosition(initialPos2) && !IsInRange(initialPos2) && maxTry > 0) {
                        initialPos2 += dir;
                        maxTry--;
                    }
                    maxTry = Mathf.Max(ChildrenNavigates.Length, ChildrenNavigates[0].Length);
                    while (IsValidPosition(initialPos2) && IsInRange(initialPos2) && !IsHaveItem(initialPos2) &&
                           maxTry > 0) {
                        initialPos2 += dir;
                        maxTry--;
                    }

                    if (IsValidPosition(initialPos2) && IsInRange(initialPos2) && IsHaveItem(initialPos2))
                        return initialPos2;
                }
            }
            return initialPos;
        }

        return CurrentPosition;
    }

    private void SetupFirstPosition() {
        if (ChildrenNavigates == null || ChildrenNavigates.Length == 0) {
            Debug.LogWarning("No children navigates found");
            return;
        }

        if (ChildrenNavigates.Length == 1) {
            CurrentChild = ChildrenNavigates[0][0];
            CurrentPosition = new Vector2Int(0, 0);
            return;
        }

        IChildrenNavigate highestChild = null;
        Vector2Int highestPosition = Vector2Int.zero;

        for (int x = 0; x < ChildrenNavigates.Length; x++) {
            for (int y = 0; y < ChildrenNavigates[x].Length; y++) {
                if (ChildrenNavigates[x][y] != null) {
                    if (highestChild == null || y > highestPosition.y ||
                        (y == highestPosition.y && x < highestPosition.x)) {
                        highestChild = ChildrenNavigates[x][y];
                        highestPosition = new Vector2Int(x, y);
                    }
                }
            }
        }

        if (highestChild != null) {
            CurrentChild = highestChild;
            CurrentPosition = highestPosition;
        } else {
            Debug.LogWarning("No valid child navigate found");
        }
    }

    private bool IsChildHaveIndexPosition(List<ChildrenNavigate> child) {
        foreach (var c in child) {
            if (c.IndexPosition.Count == 0) {
                return false;
            }
        }
        return true;
    }

    private void ReCalculatePositionInMatrix() {
        // Clear the _childrenNavigates array
        ChildrenNavigates = null;

        if (childrenNavigatesList.Count == 0) {
            return;
        }

        if (childrenNavigatesList.Count == 1) {
            ChildrenNavigates = new IChildrenNavigate[1][];
            ChildrenNavigates[0] = new IChildrenNavigate[1];
            ChildrenNavigates[0][0] = childrenNavigatesList[0];
            _maxXY = new Vector2Int(1, 1);
            return;
        }

        if (IsChildHaveIndexPosition(childrenNavigatesList)) {
            _maxXY = new Vector2Int(0, 0);

            // Find the maximum x and y positions
            foreach (var child in childrenNavigatesList) {
                foreach (var cx in child.IndexPosition) {
                    if (cx.x > _maxXY.x) _maxXY.x = cx.x + 1;
                }
                foreach (var cy in child.IndexPosition) {
                    if (cy.y > _maxXY.y) _maxXY.y = cy.y + 1;
                }
            }
            
            // Initialize the _childrenNavigates array with the found dimensions
            ChildrenNavigates = new IChildrenNavigate[_maxXY.x + 1][];
            for (int i = 0; i <= _maxXY.x; i++) {
                ChildrenNavigates[i] = new IChildrenNavigate[_maxXY.y + 1];
            }

            // Insert the children into the array
            foreach (var child in childrenNavigatesList) {
                foreach (var indexPosition in child.IndexPosition) {
                    ChildrenNavigates[indexPosition.x][indexPosition.y] = child;
                }
            }
        } else {
            // Sort the _childrenNavigatesList by their positions
            childrenNavigatesList.Sort((a, b) => {
                int compareY = a.Position.y.CompareTo(b.Position.y);
                return compareY != 0 ? compareY : a.Position.x.CompareTo(b.Position.x);
            });

            // Calculate the maximum dimensions
            var xCounts = new Dictionary<float, int>();
            var yCounts = new Dictionary<float, int>();

            foreach (var child in childrenNavigatesList) {
                var position = child.Position;
                if (!xCounts.ContainsKey(position.x)) {
                    xCounts[position.x] = 0;
                }
                if (!yCounts.ContainsKey(position.y)) {
                    yCounts[position.y] = 0;
                }
                xCounts[position.x]++;
                yCounts[position.y]++;
            }

            _maxXY = new Vector2Int(xCounts.Count, yCounts.Count);

            // Initialize the _childrenNavigates array with the found dimensions
            ChildrenNavigates = new IChildrenNavigate[_maxXY.x][];
            for (int i = 0; i < _maxXY.x; i++) {
                ChildrenNavigates[i] = new IChildrenNavigate[_maxXY.y];
            }

            // Assign indices and insert items into _childrenNavigates based on their positions
            var xIndexMap = new Dictionary<float, int>();
            var yIndexMap = new Dictionary<float, int>();

            foreach (var child in childrenNavigatesList) {
                var position = child.Position;
                if (!xIndexMap.ContainsKey(position.x)) {
                    xIndexMap[position.x] = xIndexMap.Count;
                }
                if (!yIndexMap.ContainsKey(position.y)) {
                    yIndexMap[position.y] = yIndexMap.Count;
                }
                int xIndex = xIndexMap[position.x];
                int yIndex = yIndexMap[position.y];
                ChildrenNavigates[xIndex][yIndex] = child;
            }
        }

        // Fill in nulls for any missing items
        for (int i = 0; i < _maxXY.x; i++) {
            for (int j = 0; j < _maxXY.y; j++) {
                if (ChildrenNavigates[i][j] == null) {
                    ChildrenNavigates[i][j] = null;
                }
            }
        }
    }

    #region Editor

#if UNITY_EDITOR
    [Button]
    private void FindAndAddChildrenNavigates() {
        childrenNavigatesList = GetComponentsInChildren<ChildrenNavigate>().ToList();
        Debug.Log("Children found and added.");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    #endregion
}