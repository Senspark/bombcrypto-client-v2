using System;
using System.Threading.Tasks;
using App;
using DG.Tweening;
using Senspark;
using TMPro;
using UnityEngine;
using Utils;

public class TaskContentTon : MonoBehaviour {
    [SerializeField]
    private TMP_Text titleText, amountText;

    [SerializeField]
    private GameObject btnGo, btnClaim, btnCheck;

    [SerializeField]
    private RectTransform begin;

    [SerializeField]
    private GameObject starCorePrefab;

    private ITaskTonManager _taskTonManager;
    private ISoundManager _soundManager;

    private IUserTonManager _userTonManager;

    public int Id { get; private set; }

    private TaskCompletionSource<bool> _tcs;
    private Action<TaskCompletionSource<bool>> _onGo, _onClaim;
    private Vector3 _target;
    private bool _isClicked;
    private Transform _parent;

    private void Awake() {
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
    }

    public void SetData(ITaskTonData data, string nameTask, Vector3 target, Action<TaskCompletionSource<bool>> onGo,
        Action<TaskCompletionSource<bool>> onClaim, Transform parent) {
        titleText.text = data.Name == "" ? nameTask : data.Name;
        amountText.text = data.Amount.ToString();
        Id = data.Id;
        _onGo = onGo;
        _onClaim = onClaim;
        _target = target;
        _parent = parent;
        SetupButton(data);
        _taskTonManager = ServiceLocator.Instance.Resolve<ITaskTonManager>();
        _userTonManager = ServiceLocator.Instance.Resolve<IServerManager>().UserTonManager;
    }

    public async void OnBtnGoClicked() {
        if (_isClicked)
            return;
        _isClicked = true;
        _soundManager.PlaySound(Audio.Tap);

        _tcs = new TaskCompletionSource<bool>();
        _onGo?.Invoke(_tcs);
        var result = await _tcs.Task;

        //Logic cơ bản của task hoành thành
        if (result) {
            //Update trạng thái task lên server
            var isCompleteSuccess = await _userTonManager.CompleteTask(Id);
            if (isCompleteSuccess) {
                //Update trạng thái task cho client
                _taskTonManager.CompleteTask(Id);
                DoTaskSuccess();
            }
        }

        _isClicked = false;
    }

    public async void OnBtnClaimClicked() {
        if (_isClicked)
            return;
        _isClicked = true;
        _soundManager.PlaySound(Audio.Tap);

        _tcs = new TaskCompletionSource<bool>();
        _onClaim?.Invoke(_tcs);
        var result = await _tcs.Task;

        if (result) {
            _taskTonManager.ClaimTask(Id);
            ClaimSuccess();
            
            //Update trạng thái task lên server, claim ko cần đợi kết quả để update ui
            _userTonManager.ClaimTask(Id).Forget();
        }

        _isClicked = false;
    }
    
    private void DoStarCoreAnimation() {
        var cameraMain = Camera.main;
        if (cameraMain != null) {
            for (int i = 0; i < 12; i++) {
                // Instantiate the starCorePrefab at a random position near the 'begin' position
                var position = begin.position;
                Vector3 randomPos = position +
                                    new Vector3(UnityEngine.Random.Range(-40f, 40f),
                                        UnityEngine.Random.Range(-40f, 40f),
                                        0);
                GameObject starCore = Instantiate(starCorePrefab, _parent);
                starCore.transform.position = position;

                // Move the instantiated starCore to the '_target' position using DoTween
                starCore.transform.DOMove(randomPos, 0.5f).SetEase(Ease.InOutQuad).SetUpdate(true).OnComplete(
                    () => starCore.transform.DOMove(_target, 2f).SetEase(Ease.InOutQuad).SetUpdate(true)
                        .OnComplete(() => { Destroy(starCore); })
                );
            }
        }
    }

    private void SetupButton(ITaskTonData data) {
        DisableAllBtn();

        if (data.IsClaimed && data.IsCompleted) {
            btnCheck.SetActive(true);
        } else if (data.IsCompleted) {
            btnClaim.SetActive(true);
        } else {
            btnGo.SetActive(true);
        }
    }

    public void DoTaskSuccess() {
        DisableAllBtn();
        btnClaim.SetActive(true);
    }

    private void ClaimSuccess() {
        _soundManager.PlaySound(Audio.CollectBCoin);
        DisableAllBtn();
        btnCheck.SetActive(true);
        DoStarCoreAnimation();
    }

    private void DisableAllBtn() {
        btnGo.SetActive(false);
        btnCheck.SetActive(false);
        btnClaim.SetActive(false);
    }
}