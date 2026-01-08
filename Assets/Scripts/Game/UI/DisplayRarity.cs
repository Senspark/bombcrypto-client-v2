using System;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class DisplayRarity : MonoBehaviour {
    public static List<NameColor> Rarity = new List<NameColor> {
        new NameColor("Common", new Color32(213, 213, 213, 255)),
        new NameColor("Rare", new Color32(7, 255, 36, 255)),
        new NameColor("Super Rare", new Color32(218, 144, 242, 255)),
        new NameColor("Epic", new Color32(255, 18, 241, 255)),
        new NameColor("Legend", new Color32(255, 194, 7, 255)),
        new NameColor("Super Legend", new Color32(255, 0, 84, 255)),
    };

    [SerializeField] private TMP_Text rareLbl;
    

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