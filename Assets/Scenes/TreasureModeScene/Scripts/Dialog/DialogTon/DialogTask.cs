using System;
using System.Collections.Generic;
using System.Linq;
using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using TMPro;
using UnityEngine;

namespace Game.Dialog {
    public class DialogTask : Dialog {
        private readonly List<ICategoryTonData> _allCategory = new();

        private readonly List<TaskTonCategory> _currentCategory = new();
        private readonly List<TaskContentTon> _currentTask = new();

        [SerializeField]
        private GameObject prefabTask, prefabCategory, error, pageObject;

        [SerializeField]
        private Transform categoryParent;

        [SerializeField]
        private TMP_Text maxPageText;
        [SerializeField] private TMP_InputField inputPage;

        [SerializeField]
        private RectTransform dialogTransform;

        [SerializeField]
        private TouchScreenKeyboardWebgl keyboard;

        private ISoundManager _soundManager;
        private ITaskTonManager _taskTonManager;
        private ILogManager _logManager;
        private ObserverHandle _handle;
        private DialogWaiting _dialogWaiting;

        private int _currentPage, _maxPage;
        private readonly int _itemPerPage = 10;
        private Transform _targetChestIcon, _parent;
        private float _originalBottom;
        private readonly List<TaskTonCategory> _categoryObjectPool = new();


        public static UniTask<DialogTask> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogTask>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _taskTonManager = ServiceLocator.Instance.Resolve<ITaskTonManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            error.SetActive(false);
            if(!Application.isMobilePlatform)
                inputPage.onValueChanged.AddListener(OnPageChange);
            if (dialogTransform != null)
                _originalBottom = dialogTransform.offsetMin.y;
            InitEvent();
        }

        public async void Init(Transform targetChestIcon, Transform parent) {
            pageObject.SetActive(false);
            _dialogWaiting = await DialogWaiting.Create();
            _dialogWaiting.Show(DialogCanvas);
            //Đợi 0.5s cho ani fade của dialog xong mới instantiate tránh bị khựng mất anim fade
            await UniTask.Delay(500);
            try {
                error.SetActive(false);
                if (!_taskTonManager.IsInitialized) {
                    await UniTask.WaitUntil(() => _taskTonManager.IsInitialized).Timeout(TimeSpan.FromSeconds(5));
                }
                _dialogWaiting?.HideImmediately();
                _dialogWaiting = null;
                // Có lỗi gì đó ko load đc task nào => show ui error
                if (!_taskTonManager.IsHaveAnyTask()) {
                    _logManager.Log("CLIENT: Error when init task ui : no task found");
                    error.SetActive(true);
                    return;
                }
                _targetChestIcon = targetChestIcon;
                _parent = parent;
                InitAllCategory();
                OpenPage();
                pageObject.SetActive(_maxPage > 1);
            } catch (TimeoutException) {
                _logManager.Log("CLIENT: Task not initialize completely");
                _dialogWaiting?.Hide();
                _dialogWaiting = null;
                error.SetActive(true);
            } catch (Exception e) {
                _logManager.Log($"CLIENT: Error when init task ui: {e.Message}");
                _dialogWaiting?.Hide();
                _dialogWaiting = null;
            }
        }

        public void OpenPage() {
            _currentPage = Mathf.Clamp(_currentPage, 0, _maxPage);
            foreach (var category in _currentCategory) {
                ReturnCategoryObject(category);
            }
            _currentCategory.Clear();
            _currentTask.Clear();

            InitCategory();
            InitTask(_targetChestIcon, _parent);
        }
        

        /// <summary>
        /// Lưu tất cả category task để sử dụng cho việc phân trang
        /// </summary>
        private void InitAllCategory() {
            var listData = _taskTonManager.TaskCategoryTonDataDict;

            //Sort category ưu tiên các task mới và chưa hoàn thành
            listData = listData
                // Ưu tiên 1: task của công ty
                .OrderByDescending(task => task.TaskCategory == 1)

                // Uư tiên 2: Task mới thêm
                .ThenByDescending(task => task.IsNew)

                // Uư tiên 3: Task đang làm dỡ
                .ThenByDescending(task => {
                    var tasks = _taskTonManager.GetAllTaskFromCategory(task.TaskCategory);
                    return tasks.Count(t => t.IsCompleted) > 0 && tasks.Count(t => t.IsCompleted) < tasks.Count;
                })

                // Ưu tiên 4: Đã đã hoàn thành hết mà còn cái chưa claim
                .ThenByDescending(task => _taskTonManager.GetAllTaskFromCategory(task.TaskCategory)
                    .All(t => t.IsCompleted) && _taskTonManager.GetAllTaskFromCategory(task.TaskCategory)
                    .Any(t => !t.IsClaimed))

                //Ưu tiên 5: Các task còn lại (ko cần làm gì)

                //Ưu tiên 6: Các task đã hoàn thành và claim hết (Xếp cuối cùng)
                .ThenBy(task => _taskTonManager.GetAllTaskFromCategory(task.TaskCategory)
                    .All(t => t.IsCompleted && t.IsClaimed))
                .ToList();

            foreach (var data in listData) {
                if (!_taskTonManager.IsValidCategory(data.TaskCategory))
                    continue;
                _allCategory.Add(data);
            }
            _maxPage = CalculateMaxPage(_allCategory);
            _currentPage = 0;

            inputPage.text = $"{_currentPage + 1}";
            maxPageText.text = $"/{_maxPage}";
        }

        private void InitCategory() {
            var listData = GetItemsForPage(_currentPage);

            for (var i =0; i < listData.Count; i++) {
                var taskTonCategory = GetCategoryObject(i);
                taskTonCategory.SetData(listData[i]);
                _currentCategory.Add(taskTonCategory);
            }
        }

        private TaskTonCategory GetCategoryObject(int index) {
            if (_categoryObjectPool.Count -1 < index) {
                var obj = Instantiate(prefabCategory, categoryParent);
                obj.TryGetComponent<TaskTonCategory>(out var taskTonCategory);
                if (taskTonCategory == null)
                    return null;
                taskTonCategory.Init();
                _categoryObjectPool.Add(taskTonCategory);
                return taskTonCategory;
            }
            var objPool = _categoryObjectPool[index];
            objPool.gameObject.SetActive(true);
            return objPool;
        }

        private void ReturnCategoryObject(TaskTonCategory taskTonCategory) {
            taskTonCategory.ReturnAllTaskToPool();
            taskTonCategory.gameObject.SetActive(false);
            //_categoryObjectPool.Enqueue(taskTonCategory);
        }

        private void InitTask(Transform targetChestIcon, Transform parent) {
            foreach (var task in _currentCategory) {
                var listTask = task.InitTask(prefabTask, targetChestIcon.position,
                    _taskTonManager.GetTaskTonDataList(task.TaskCategory),
                    parent);

                _currentTask.AddRange(listTask);
            }
        }

        public void OnPrevPageClicked() {
            if (_currentPage <= 0) {
                return;
            }
            _soundManager.PlaySound(Audio.Tap);
            _currentPage -= 1;
            inputPage.text = $"{_currentPage + 1}";
            OpenPage();
        }

        public void OnNextPageClicked() {
            if (_currentPage >= _maxPage - 1) {
                return;
            }
            _soundManager.PlaySound(Audio.Tap);
            _currentPage += 1;
            inputPage.text = $"{_currentPage + 1}";
            OpenPage();
        }

        private void OnPageChange(string text) {
            if (!int.TryParse(text, out var amount)) {
                return;
            }
            var valid = Math.Clamp(amount, 1, _maxPage);
            inputPage.text = $"{valid}";
            if(_currentPage == valid - 1)
                return;
            _currentPage = valid - 1;
            OpenPage();
        }

        public void OpenTouchScreenKeyboard() {
            if (CanShowVirtualKeyboard() && keyboard != null) {
                keyboard.OpenKeyboard(true, false);
                keyboard.OnValueChanged += OnPageChange;
                keyboard.OnKeyboardClosed += CloseTouchScreenKeyboard;
                if (dialogTransform != null) {
                    dialogTransform.offsetMin = new Vector2(dialogTransform.offsetMin.x, 340);
                }
            }
        }

        private bool CanShowVirtualKeyboard() {
#if UNITY_EDITOR
            return true;
#endif
            return Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform &&
                   AppConfig.IsTon();
        }

        private void CloseTouchScreenKeyboard() {
            if (keyboard != null) {
                OnPageChange(keyboard.GetInput());
                keyboard.ClearInput();
                keyboard.OnValueChanged -= OnPageChange;
                if (dialogTransform != null) {
                    dialogTransform.offsetMin = new Vector2(dialogTransform.offsetMin.x, _originalBottom);
                }
            }
        }

        private int CalculateMaxPage(List<ICategoryTonData> items) {
            if (items == null || items.Count == 0) {
                return 0;
            }
            return (int)Math.Ceiling(items.Count / (double)_itemPerPage);
        }


        private List<ICategoryTonData> GetItemsForPage(int pageIndex) {
            if (_allCategory == null) {
                return new List<ICategoryTonData>();
            }
            var itemsForPage = _allCategory.Skip(pageIndex * _itemPerPage).Take(_itemPerPage).ToList();
            
            return itemsForPage;
        }

        private void InitEvent() {
            _handle = new ObserverHandle();
            _handle.AddObserver(_taskTonManager, new UserTonObserver() {
                OnCompleteTask = OnTaskComplete
            });
        }

        /// <summary>
        /// Task này đc hoàn thành từ chỗ khác => cần update UI
        /// </summary>
        /// <param name="id"></param>
        private void OnTaskComplete(int id) {
            var task = _currentTask.FirstOrDefault(t => t.Id == id);
            if (task) {
                task.DoTaskSuccess();
            }
        }

        protected override void OnDestroy() {
            _handle.Dispose();
            if(!Application.isMobilePlatform)
                inputPage.onValueChanged.RemoveListener(OnPageChange);
            base.OnDestroy();
        }

        public void OnCloseBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}