using System;
using System.Collections;
using App;
using Cysharp.Threading.Tasks;
using Engine.Utils;
using Senspark;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class HeroDetailsDisplay : MonoBehaviour {
    [SerializeField]
    private Avatar avatar;

    [SerializeField]
    private Text charName;

    [SerializeField]
    private HeroRarityDisplay rarityDisplay;

    [SerializeField]
    private Text charLv;

    [SerializeField]
    private CharacterSkillPointDisplay charPower;

    [SerializeField]
    private CharacterSkillPointDisplay charSpeed;

    [SerializeField]
    private CharacterSkillPointDisplay charStamina;

    [SerializeField]
    private CharacterSkillPointDisplay charBombNum;

    [SerializeField]
    private CharacterSkillPointDisplay charBombRange;
    
    [SerializeField]
    private CharacterSkillPointDisplay charShield;

    [SerializeField]
    private HeroAbilitiesGroup abilitiesGroup;
    
    [SerializeField]
    private Animator rarityDiamondAnimator;

    [SerializeField]
    private Button repairBtn;
    
    [CanBeNull] [SerializeField] private InventoryHeroL inventoryHeroL;
    [CanBeNull] [SerializeField] private InventoryHeroS inventoryHeroS;
    [CanBeNull] [SerializeField]
    private GameObject lockObject;

    [SerializeField]
    private Text textTimeLock;

    [SerializeField] [CanBeNull]
    private GameObject groupButton;

    private PlayerData _playerData;
    private Canvas _dialogCanvas;
    private IRepairShieldManager _repairShieldManager;
    private static readonly int Rarity = Animator.StringToHash("Rarity");
    private Action _onRepairShieldButtonClick;
    private bool _enableRepair, _showGroupButton, _enableOpenStake = true;

    private void Awake() {
        _repairShieldManager = ServiceLocator.Instance.Resolve<IRepairShieldManager>();
    }

    public void Init(bool enableRepair, bool showGroupButton = true, bool enableOpenStake = true) {
        _enableRepair = enableRepair;
        _showGroupButton = showGroupButton;
        _enableOpenStake = enableOpenStake;
    }

    public void SetInfo(PlayerData player, Canvas dialogCanvas, Action onRepairShieldButtonClick = null) {
        _dialogCanvas = dialogCanvas;
        _playerData = player;
        _onRepairShieldButtonClick = onRepairShieldButtonClick;
        
        avatar.ChangeImage(player);
        avatar.gameObject.SetActive(true);

        //DevHoang: Add new airdrop
        switch (player.AccountType) {
            case HeroAccountType.Nft:
                charName.text = $"{player.heroId.Id}";
                break;
            case HeroAccountType.Sol:
                charName.text = $"SOL Hero {player.heroId.Id}";
                break;
            case HeroAccountType.Ron:
                charName.text = $"RON Hero {player.heroId.Id}";
                break;
            case HeroAccountType.Bas:
                charName.text = $"BASE Hero {player.heroId.Id}";
                break;
            case HeroAccountType.Vic:
                charName.text = $"VIC Hero {player.heroId.Id}";
                break;
            default:
                charName.text = $"TON Hero {player.heroId.Id}";
                break;
        }
        if (charLv) {
            charLv.text = "Lv" + player.level;
        }
        if (rarityDiamondAnimator) {
            rarityDiamondAnimator.SetInteger(Rarity, player.rare);
        }

        charPower.SetPoint(player.bombDamage, player.GetUpgradePower());
        charSpeed.SetPoint(player.speed);
        charStamina.SetPoint(player.stamina);
        charBombNum.SetPoint(player.bombNum);
        charBombRange.SetPoint(player.bombRange);
        var hasShield = player.Shield != null;
        charShield?.gameObject.SetActive(hasShield);
        if (hasShield && charShield) {
            charShield.SetPoint(player.Shield.CurrentAmount, player.Shield.TotalAmount);
        }
        abilitiesGroup.Show(player.abilities);
        rarityDisplay.Show(player.rare);

        inventoryHeroL?.Show(player, _dialogCanvas, _enableOpenStake);
        inventoryHeroS?.Show(player, _dialogCanvas, _enableOpenStake);


        if (repairBtn != null) {
            repairBtn.gameObject.SetActive(_enableRepair);
        }
        if(groupButton != null) {
            groupButton.SetActive(_showGroupButton);
        }    
    }
    public void HideInfo() {
        if (charLv) {
            charLv.text = null;
        }

        charName.text = null;
        charPower.Clear();
        charSpeed.Clear();
        charStamina.Clear();
        charBombNum.Clear();
        charBombRange.Clear();
        charShield.Clear();
        inventoryHeroL?.Clear();
        inventoryHeroS?.Clear();
        charShield.gameObject.SetActive(false);
        abilitiesGroup.Hide();
        rarityDisplay.Hide();
        avatar.gameObject.SetActive(false);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void OpenRepairShield() {
        _repairShieldManager.CreateDialog().ContinueWith(dialog => {
            dialog.Init(_playerData.heroId);
            dialog.Show(_dialogCanvas);
            _onRepairShieldButtonClick?.Invoke();
        });
    }

    public void SetLockHero() {
        if (!lockObject) {
            return;
        }
        // Snapshot: panel chi tiết chỉ hiện/ẩn icon lock theo data hiện tại, không đếm ngược.
        var locked = _playerData.IsLocked();
        if (groupButton) groupButton.SetActive(!locked);
        lockObject.SetActive(locked);
    }
}