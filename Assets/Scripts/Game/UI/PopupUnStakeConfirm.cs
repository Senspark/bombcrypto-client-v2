using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using Constant;

using Game.Dialog;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class PopupUnStakeConfirm : MonoBehaviour {
    // [SerializeField]
    // private TMP_Text
    //     stakedText,
    //     unStakeText,
    //     feeText,
    //     totalUnstake,
    //     stakeRemain,
    //     minValue,
    //     proccessing;
    //
    // [SerializeField]
    // private PopupResult popupResult;
    //
    // [SerializeField]
    // private Button buttonUnStake;
    //
    // [SerializeField]
    // private GameObject info;
    //
    // private Action _callbackHide;
    // private IBlockchainManager _blockchainManager;
    // private IPlayerStorageManager _playerStorageManager;
    // private DialogLegacyHeroes _dialogLegacyHeroes;
    // private IServerManager _serverManager;
    // private INetworkConfig _networkConfig;
    // private DataUnStake _dataUnStake;
    //
    // private double _amountWantUnStake;
    // private int _heroId;
    // private RewardType _tokenType;
    //
    // public void Show(DataUnStake data, int heroId, Action callbackHide = null) {
    //     Reset();
    //     _dataUnStake = data;
    //     _callbackHide = callbackHide;
    //     _heroId = heroId;
    //     gameObject.SetActive(true);
    //
    //     _tokenType = data.TokenType;
    //     _amountWantUnStake = double.Parse(data.UnStake);
    //     UpdateText(data);
    // }
    //
    // public void Init(DialogLegacyHeroes dialogLegacyHeroes) {
    //     _dialogLegacyHeroes = dialogLegacyHeroes;
    //     _blockchainManager = _dialogLegacyHeroes.BlockchainManager;
    //     _serverManager = _dialogLegacyHeroes.ServerManager;
    //     _playerStorageManager = _dialogLegacyHeroes.PlayerStoreManager;
    //     _networkConfig = _dialogLegacyHeroes.NetworkConfig;
    // }
    //
    // private void UpdateText(DataUnStake data) {
    //     stakedText.text = data.Staked;
    //     unStakeText.text = data.UnStake;
    //     feeText.text = data.Fee + "%";
    //     totalUnstake.text = data.TotalUnStake;
    //     stakeRemain.text = data.StakeRemain;
    //     minValue.text = data.MinValue + " Bcoin";
    //     info.SetActive(!data.PlayerData.IsHeroS && data.TokenType == RewardType.BCOIN);
    // }
    //
    // private async Task<bool> UnStake() {
    //     var tokenAddress = _tokenType == RewardType.BCOIN
    //         ? _networkConfig.BlockchainConfig.CoinTokenAddress
    //         : _networkConfig.BlockchainConfig.SensparkTokenAddress;
    //     return await _blockchainManager.WithDrawFromHeroId(_heroId, _amountWantUnStake, tokenAddress);
    // }
    //
    // public async void OnBtnConfirm() {
    //     _dialogLegacyHeroes.SoundManager.PlaySound(Audio.Tap);
    //     _dialogLegacyHeroes.ShowPanelBlock(true);
    //     IsProcessing(true);
    //     var result = await UnStake();
    //     if (result) {
    //         //Đợi 30s trước khi gọi lên server
    //         await WebGLTaskDelay.Instance.Delay(30 * 1000);
    //         await _serverManager.Pve.CheckBomberStake(_dataUnStake.PlayerData.heroId);
    //         //sycn hero để cập nhật stake sen và bcoin cho client
    //         await _serverManager.General.SyncHero(false);
    //     }
    //
    //     var selectedHeroId = _playerStorageManager.GetPlayerDataFromId(_dataUnStake.PlayerData.heroId);
    //
    //     IsProcessing(false);
    //     _dialogLegacyHeroes.ShowPanelBlock(false);
    //     gameObject.SetActive(false);
    //
    //     _callbackHide += () => _dialogLegacyHeroes.UpdateHeroInfo(selectedHeroId);
    //     popupResult.Show(result, null, _callbackHide);
    // }
    //
    // public void Hide() {
    //     _callbackHide?.Invoke();
    //     gameObject.SetActive(false);
    // }
    //
    // #region Processing
    //
    // private Coroutine _coroutine;
    //
    // private void ChangeToProcessing(bool isProcess) {
    //     buttonUnStake.gameObject.SetActive(!isProcess);
    //
    //     proccessing.gameObject.SetActive(isProcess);
    //     _isProcessing = isProcess;
    //     if (_isProcessing) {
    //         _coroutine = StartCoroutine(ProcessingAnim());
    //     } else if (_coroutine != null) {
    //         StopCoroutine(_coroutine);
    //     }
    // }
    //
    // private bool _isProcessing;
    //
    // private List<string> _processTextList = new List<string>() {
    //     "Processing transaction",
    //     "Processing transaction.",
    //     "Processing transaction..",
    //     "Processing transaction...",
    // };
    //
    // IEnumerator ProcessingAnim() {
    //     while (_isProcessing) {
    //         for (int i = 0; i < _processTextList.Count; i++) {
    //             if (!_isProcessing)
    //                 yield break;
    //
    //             proccessing.text = _processTextList[i];
    //             yield return new WaitForSecondsRealtime(0.5f);
    //         }
    //     }
    // }
    //
    // private void IsProcessing(bool value) {
    //     ChangeToProcessing(value);
    //     _dialogLegacyHeroes.ShowPanelBlock(value);
    // }
    //
    // private void Reset() {
    //     IsProcessing(false);
    // }
    //
    // #endregion
}