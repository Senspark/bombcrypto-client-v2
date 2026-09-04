using System;
using System.Collections.Generic;
using App;
using Constant;
using Game.Dialog;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using PvpMode.Utils;

namespace Scenes.MainMenuScene.Scripts
{
    public class BLDialogPvpWager : Dialog
    {
        [SerializeField] private Button playBtn;
        [SerializeField] private Button closeBtn;

        [Header("Token Selection")]
        [SerializeField] private Toggle bcoinBscToggle;
        [SerializeField] private Toggle bcoinPolyToggle;
        [SerializeField] private Toggle senBscToggle;
        [SerializeField] private Toggle senPolyToggle;

        [Header("Tier Selection")]
        [SerializeField] private Transform tierContainer;
        [SerializeField] private GameObject tierItemPrefab;

        private PvpWagerToken _selectedToken = PvpWagerToken.BCOIN_BSC;
        private PvpWagerTier _selectedTier = PvpWagerTier.TIER_1;
        private Action<PvpWagerToken, PvpWagerTier> _onPlay;

        private readonly List<GameObject> _tierItems = new List<GameObject>();

        public static async UniTask<BLDialogPvpWager> Create()
        {
            return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<BLDialogPvpWager>();
        }

        protected override void Awake()
        {
            base.Awake();
            playBtn.onClick.AddListener(OnPlayClick);
            closeBtn.onClick.AddListener(Hide);

            bcoinBscToggle.onValueChanged.AddListener(v => { if(v) _selectedToken = PvpWagerToken.BCOIN_BSC; });
            bcoinPolyToggle.onValueChanged.AddListener(v => { if(v) _selectedToken = PvpWagerToken.BCOIN_POLYGON; });
            senBscToggle.onValueChanged.AddListener(v => { if(v) _selectedToken = PvpWagerToken.SEN_BSC; });
            senPolyToggle.onValueChanged.AddListener(v => { if(v) _selectedToken = PvpWagerToken.SEN_POLYGON; });
            
            InitTiers();
        }

        public void Initialize(Action<PvpWagerToken, PvpWagerTier> onPlay)
        {
            _onPlay = onPlay;
        }

        private void InitTiers()
        {
            // Clear container
            foreach (Transform child in tierContainer) Destroy(child.gameObject);
            _tierItems.Clear();

            // Create buttons for each tier
            foreach (PvpWagerTier tier in Enum.GetValues(typeof(PvpWagerTier)))
            {
                if (tier == PvpWagerTier.NONE) continue;

                var item = Instantiate(tierItemPrefab, tierContainer);
                _tierItems.Add(item);
                
                var btn = item.GetComponent<Button>();
                var txt = item.GetComponentInChildren<Text>();
                
                string label = PvpWagerUtils.GetAmount(tier).ToString("N0");
                if (txt != null) txt.text = label;

                var t = tier;
                btn.onClick.AddListener(() => {
                    _selectedTier = t;
                    UpdateTierVisuals();
                });
            }
            UpdateTierVisuals();
        }

        private void UpdateTierVisuals()
        {
            int index = 0;
            foreach (PvpWagerTier tier in Enum.GetValues(typeof(PvpWagerTier)))
            {
                if (tier == PvpWagerTier.NONE) continue;
                
                if (index < _tierItems.Count)
                {
                    var highlight = _tierItems[index].transform.Find("Highlight");
                    if (highlight != null) highlight.gameObject.SetActive(tier == _selectedTier);
                }
                index++;
            }
        }

        private void OnPlayClick()
        {
            _onPlay?.Invoke(_selectedToken, _selectedTier);
            Hide();
        }

        protected override void OnYesClick() => OnPlayClick();
    }
}
