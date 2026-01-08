using System;
using System.Globalization;
using System.Linq;

using Game.UI;

using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

namespace App {
    [CustomEditor(typeof(LocalizeText))]
    [CanEditMultipleObjects]
    public class LocalizeTextEditor : UnityEditor.Editor {
        private LocalizeKey _selected;

        private SerializedProperty _upperCase;
        private SerializedProperty _key;

        private void OnEnable() {
            _upperCase = serializedObject.FindProperty("upperCase");
            _key = serializedObject.FindProperty("key");
        }

        public override void OnInspectorGUI() {
            EditorGUILayout.PropertyField(_upperCase, new GUIContent("Upper Case:"), GUILayout.Height(20));
            EditorGUILayout.PropertyField(_key, new GUIContent("Key:"), GUILayout.Height(20));
            EditorGUILayout.Space();
            
            _selected = SearchEnumLabel("Find:", _selected);
            
            EditorGUILayout.Space();
            
            if (_selected > 0) {
                _key.stringValue = _selected.ToString();
                _selected = 0;
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private string _searchEnumText = "";
        private bool _isSearchEnumLabelInSearch = false;

        private T SearchEnumLabel<T>(string label, T state) where T : struct, IConvertible {
            if (!typeof(T).IsEnum) {
                EditorGUILayout.LabelField("T must be an enumerated type");
                return state;
            }
            var states = Enum.GetValues(typeof(T)).Cast<object>().Select(o => o.ToString()).ToArray();
            if (string.IsNullOrWhiteSpace(_searchEnumText) && states.Length > 2) {
                _searchEnumText = state.ToString(CultureInfo.InvariantCulture);
            }

            var text = EditorGUILayout.TextField(label, _searchEnumText);
            if (text != _searchEnumText || _isSearchEnumLabelInSearch) {
                _searchEnumText = text;
                // var mach = states.Select((v, i) => new { value = v, index = i })
                //     .Where(a => a.value.ToLower().StartsWith(text.ToLower())).ToList();
                
                var mach = states.Select((v, i) => new { value = v, index = i })
                    .Where(a => a.value.ToLower().Contains(text.ToLower())).ToList();
                
                var targetState = state;
                if (mach.Any()) {
                    // many of results
                    targetState = (T)Enum.GetValues(typeof(T)).GetValue(mach[0].index);
                    EditorGUILayout.LabelField("Select closested: " + targetState);
                    Repaint();
                    var selected = GUILayout.SelectionGrid(-1, mach.Select(v => v.value).ToArray(), 4);
                    if (selected != -1) {
                        targetState = (T)Enum.GetValues(typeof(T)).GetValue(mach[selected].index);
                        _searchEnumText = targetState.ToString(CultureInfo.InvariantCulture);
                        _isSearchEnumLabelInSearch = false;
                        GUI.FocusControl("FocusAway");
                        Repaint();
                    }
                }
                
                state = targetState;
                _isSearchEnumLabelInSearch = !string.Equals(_searchEnumText,
                    targetState.ToString(CultureInfo.InvariantCulture), StringComparison.CurrentCultureIgnoreCase);

                return state;
            }
            // return state;
            return default(T);
        }
    }
}
#endif