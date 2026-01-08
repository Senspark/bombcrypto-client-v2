#if UNITY_EDITOR
using System;

using PvpMode.Entities;

using UnityEditor;

using UnityEngine;

namespace App.Editor {
    [CustomEditor(typeof(PlayerPvp))]
    public class PlayerPvPEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            var player = target as PlayerPvp ?? throw new Exception($"Could not cast to {typeof(PlayerPvp)}");
            if (GUILayout.Button("Increase speed")) {
                player.Movable.Speed++;
            }
        }
    }
}
#endif