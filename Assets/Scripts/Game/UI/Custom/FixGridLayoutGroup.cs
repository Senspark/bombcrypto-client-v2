using System.Threading.Tasks;

using App;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Custom {
    public class FixGridLayoutGroup : MonoBehaviour, ILayoutGroup {
        public int minColInput = 2;
        public int minRowInput = 2;
        public int minSpaceInput = 4;

        public int ColMin => _colMin;
        public int RowMin => _rowMin;
        public bool IsLoad => _isLoad;

        public Task<bool> WaitLayoutDone {
            get {
                if (_isLoad) {
                    return Task.FromResult(true);
                }
                _waitLayoutDone ??= new TaskCompletionSource<bool>();
                return _waitLayoutDone.Task;
            }
        }

        private int _colMin = 0;
        private int _rowMin = 0;
        private bool _isLoad = false;
        private TaskCompletionSource<bool> _waitLayoutDone;

        public async Task<int> GetColumn() {
            await WaitLayoutDone;
            var gridLayoutGroup = GetComponent<GridLayoutGroup>();
            return gridLayoutGroup.constraintCount;
        }
        
        public void SetLayoutHorizontal() {
            if(!Application.isPlaying) {
                return;
            }
            var gridLayoutGroup = GetComponent<GridLayoutGroup>();
            if (!gridLayoutGroup) {
                return;
            }

            if (!AppConfig.IsTon() && ScreenUtils.IsIPadScreen()) {
                gridLayoutGroup.padding.left = 0;
            }
            
            var size = this.transform.parent.GetComponent<RectTransform>().rect.size;
            size.x -= gridLayoutGroup.padding.left;
            if (size.x <= 0) {
                return;
            }
            _colMin = Mathf.Max(Mathf.FloorToInt(size.x / (gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x)), 1);
            if (_colMin < minColInput) {
                _colMin = minColInput;
                var wRefer = (size.x / _colMin) - minSpaceInput;
                var hRefer = wRefer / gridLayoutGroup.cellSize.x * gridLayoutGroup.cellSize.y;
                gridLayoutGroup.spacing = new Vector2(minSpaceInput, minSpaceInput);
                gridLayoutGroup.cellSize = new Vector2(wRefer, hRefer);
            }
            _rowMin = Mathf.Max(Mathf.FloorToInt(size.y / (gridLayoutGroup.cellSize.y + gridLayoutGroup.spacing.y)), minRowInput);
            gridLayoutGroup.constraintCount = _colMin;
            _isLoad = true;
        }

        public void SetLayoutVertical() {
        }

        private void LateUpdate() {
            if (_waitLayoutDone == null || !_isLoad) {
                return;
            }
            _waitLayoutDone.SetResult(true);
            _waitLayoutDone = null;
        }
    }
}