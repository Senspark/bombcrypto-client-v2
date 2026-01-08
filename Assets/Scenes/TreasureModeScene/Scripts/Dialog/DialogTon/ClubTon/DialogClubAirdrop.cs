using System;
using System.Collections.Generic;
using System.Threading;
using App;
using Cysharp.Threading.Tasks;
using Engine.Utils;
using PvpMode.Services;
using Senspark;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using TMPro;

public enum ClubTab {
    JoinTab,
    InfoTab,
    PromoteClub,
    LeagueTab
}

namespace Game.Dialog {
    public class DialogClubAirdrop : Dialog {
        private enum CloseDialog {
            Default,
            FromJoinClub,
            FromPromotedClub
        }
        
        [SerializeField]
        private GameObject tabJoin;
        
        [SerializeField]
        private GameObject tabInfo;
        
        [SerializeField]
        private GameObject tabPromote;
        
        [SerializeField]
        private GameObject tabLeague;
        
        [SerializeField]
        private Transform content;
        
        [SerializeField]
        private TextMeshProUGUI seasonText;

        private List<GameObject> _displayObjs = new List<GameObject>();
        
        private IServerManager _serverManager;
        private ISoundManager _soundManager;
        private CancellationTokenSource _cancellationTokenSource;
        
        private ICoinLeaderboardConfigResult[] _clubConfig;
        private IClubInfo _clubInfo;
                
        public static UniTask<DialogClubAirdrop> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogClubAirdrop>();
        }
        
        protected override void Awake() {
            base.Awake();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _cancellationTokenSource = new CancellationTokenSource();
            OnWillHide(() => _cancellationTokenSource.Cancel());
        }

        private void Start() {
            UniTask.Void(async _ => {
                try {
                    var coinRankingResult = await _serverManager.Pvp.GetCoinRanking();
                    UpdateSeasonText(coinRankingResult.RemainTime);
                } catch (Exception e) {
                    DialogError.ShowError(DialogCanvas, e.Message);
                }
            }, _cancellationTokenSource.Token);
        }
        
        private void UpdateSeasonText(int remainTime) {
            if (seasonText == null) 
                return;

            string remainTimeText;
            if (remainTime < 0) {
                remainTimeText = Epoch.GetTimeStringDayHourMinute(-remainTime);
                seasonText.text = $"START SEASON: {remainTimeText}";
            } else {
                remainTimeText = Epoch.GetTimeStringDayHourMinute(remainTime);
                seasonText.text = $"END SEASON: {remainTimeText}";
            }
        }

        public void SetClubTab(ClubTab tab) {
            UniTask.Void(async () => {
                var dialog = await DialogWaiting.Create();
                dialog.Show(DialogCanvas);
                try {
                    SelectTab(tab);
                } catch (Exception ex) {
                    await DialogError.ShowError(DialogCanvas, ex.Message, Back);
                    Debug.LogError(ex);
                } finally {
                    dialog.Hide();
                }
            });
        }

        private void SelectTab(ClubTab tab) {
            switch (tab) {
                case ClubTab.JoinTab:
                    CreateJoinTab();
                    break;
                case ClubTab.InfoTab:
                    CreateInfoTab(_clubInfo.ClubId);
                    break;
            }
        }
        
        private void CreateJoinTab() {
            UniTask.Void(async () => {
                var dialog = await DialogWaiting.Create();
                dialog.Show(DialogCanvas);
                try {
                    if (_clubConfig == null) {
                        _clubConfig = await _serverManager.Pvp.GetCoinLeaderboardConfig();
                    }
                    var data = await _serverManager.General.GetTopBidClub();
                    var obj = Instantiate(tabJoin, content);
                    _displayObjs.Add(obj);
            
                    var item = obj.GetComponent<ClubJoinTab>();
                    item.SetCanvas(DialogCanvas);
                    item.SetDialogClub(this);
                    item.SetJoinTab();
                    item.SetInfo(data, _clubConfig, CreateInfoTab);
                    dialog.Hide();
                } catch (Exception ex) {
                   DialogError.ShowError(DialogCanvas, ex.Message);
                    Debug.LogError(ex);
                    dialog.Hide();
                }
            });
        }
        
        private void CreatePromotedClubsTab() {
            UniTask.Void(async () => {
                var dialog = await DialogWaiting.Create();
                dialog.Show(DialogCanvas);
                try {
                    if (_clubConfig == null) {
                        _clubConfig = await _serverManager.Pvp.GetCoinLeaderboardConfig();
                    }
                    var data = await _serverManager.General.GetTopBidClub();
                    var obj = Instantiate(tabJoin, content);
                    Back();
                    _displayObjs.Add(obj);
            
                    var item = obj.GetComponent<ClubJoinTab>();
                    item.SetPromotedClubsTab();
                    item.SetInfo(data, _clubConfig, CreateInfoTab);
                    dialog.Hide();
                } catch (Exception ex) {
                    DialogError.ShowError(DialogCanvas, ex.Message);
                    Debug.LogError(ex);
                    dialog.Hide();
                }
            });
        }

        public void CreateInfoTab(long clubId) {
            UniTask.Void(async () => {
                var dialog = await DialogWaiting.Create();
                dialog.Show(DialogCanvas);
                try {
                    if (_clubConfig == null) {
                        _clubConfig = await _serverManager.Pvp.GetCoinLeaderboardConfig();
                    }
                    var isCurrentClub = _clubInfo != null && _clubInfo.ClubId == clubId;
                    var data = isCurrentClub ? _clubInfo : await _serverManager.General.GetClubInfo(clubId);
                    var obj = Instantiate(tabInfo, content);
                    _displayObjs.Add(obj);

                    RemovePreviousDialogs();
                    var item = obj.GetComponent<ClubInfoTab>();
                    item.SetCanvas(DialogCanvas);
                    item.SetDialogClub(this);
                    item.SetInfo(data, _clubConfig);
                    item.SetAction(CreateLeagueTab, CreatePromoteTab);
                    dialog.Hide();
                } catch (Exception ex) {
                    DialogError.ShowError(DialogCanvas, ex.Message);
                    Debug.LogError(ex);
                    dialog.Hide();
                }
            });
        }
        
        private void CreatePromoteTab(long clubId) {
            UniTask.Void(async () => {
                var dialog = await DialogWaiting.Create();
                dialog.Show(DialogCanvas);
                try {
                    var data = await _serverManager.General.GetBidPrice(clubId);
                    var obj = Instantiate(tabPromote, content);
                    _displayObjs.Add(obj);
            
                    var item = obj.GetComponent<ClubPromoteTab>();
                    item.SetCanvas(DialogCanvas);
                    item.SetAction(Back, CreatePromotedClubsTab);
                    item.SetInfo(data, clubId);
                    dialog.Hide();
                } catch (Exception ex) {
                    DialogError.ShowError(DialogCanvas, ex.Message);
                    Debug.LogError(ex);
                    dialog.Hide();
                }
            });
        }
        
        private void CreateLeagueTab(long clubId = -1) {
            UniTask.Void(async () => {
                var dialog = await DialogWaiting.Create();
                dialog.Show(DialogCanvas);
                try {
                    if (_clubConfig == null) {
                        _clubConfig = await _serverManager.Pvp.GetCoinLeaderboardConfig();
                    }
                    var data = await _serverManager.General.GetAllClub();
                    var obj = Instantiate(tabLeague, content);
                    _displayObjs.Add(obj);
            
                    var item = obj.GetComponent<ClubLeagueTab>();
                    item.SetDialogClub(this);
                    item.SetAction(CreateInfoTab);
                    item.SetInfo(data, _clubConfig);
                    dialog.Hide();
                } catch (Exception ex) {
                    DialogError.ShowError(DialogCanvas, ex.Message);
                    Debug.LogError(ex);
                    dialog.Hide();
                }
            });
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _cancellationTokenSource.Dispose();
        }
        
        public void UpdateClubInfo(IClubInfo clubInfo) {
            _clubInfo = clubInfo;
            var count = _displayObjs.Count;
            if (count > 1) {
                for (var i = 0; i < count - 1; i++) {
                    Destroy(_displayObjs[i].gameObject);
                }
                _displayObjs.RemoveRange(0, count - 1);
            }
        }

        private void RemovePreviousDialogs() {
            var count = _displayObjs.Count;
            if (count > 1) {
                for (var i = 0; i < count - 1; i++) {
                    Destroy(_displayObjs[i].gameObject);
                }
                _displayObjs.RemoveRange(0, count - 1);
            }
        }

        public IClubInfo GetClubInfo() {
            return _clubInfo;
        }
        
        public void Back() {
            _soundManager.PlaySound(Audio.Tap);
            var count = _displayObjs.Count;
            if (count > 1) {
                Destroy(_displayObjs[count - 1].gameObject);
                _displayObjs.RemoveAt(count - 1);
            } else {
                Hide();
            }
        }
    }
}
