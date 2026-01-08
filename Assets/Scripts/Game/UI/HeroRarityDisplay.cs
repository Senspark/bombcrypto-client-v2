using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroRarityDisplay : MonoBehaviour {
    public static List<NameColor> Rarity = new List<NameColor> {
        new NameColor("Common", new Color32(213, 213, 213, 255)),
        new NameColor("Rare", new Color32(7, 255, 36, 255)),
        new NameColor("Super Rare", new Color32(218, 144, 242, 255)),
        new NameColor("Epic", new Color32(255, 18, 241, 255)),
        new NameColor("Legend", new Color32(255, 194, 7, 255)),
        new NameColor("Super Legend", new Color32(255, 0, 84, 255)),
        new NameColor("Mega", new Color32(53, 160, 208, 255)),
        new NameColor("Super Mega", new Color32(7, 107, 212, 255)),
        new NameColor("Mystic", new Color32(107, 109, 230, 255)),
        new NameColor("Super Mystic", new Color32(200, 86, 142, 255)),
    };

    [SerializeField] private Text rareLbl;

    private void Awake() {
        Hide();
    }

    public int rarity;
    private void OnValidate() {
        Show(rarity);
    }

    public static NameColor GetRarityData(int rarity) {
        rarity = Mathf.Clamp(rarity, 0, Rarity.Count - 1);
        return Rarity[rarity];
    }

    public void Show(int rarity) {
        var d = GetRarityData(rarity);
        rareLbl.text = d.Name;
        rareLbl.color = d.Color;
    }

    public void Hide() {
        rareLbl.text = null;
    }

    public class NameColor {
        public string Name;
        public Color Color;

        public NameColor(string name, Color color) {
            Name = name;
            Color = color;
        }
    }
}