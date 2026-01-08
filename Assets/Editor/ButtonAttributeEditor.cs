using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Editor that displays buttons for methods marked with [Button] attribute.
/// Replacement for Odin Inspector's button functionality.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(MonoBehaviour), true)]
public class ButtonAttributeEditor : UnityEditor.Editor {
    private List<MethodInfo> _buttonMethods;
    private Dictionary<MethodInfo, object[]> _methodParameters;

    private void OnEnable() {
        _buttonMethods = new List<MethodInfo>();
        _methodParameters = new Dictionary<MethodInfo, object[]>();

        var targetType = target.GetType();
        var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods) {
            var buttonAttribute = method.GetCustomAttribute<ButtonAttribute>();
            if (buttonAttribute != null) {
                _buttonMethods.Add(method);

                var parameters = method.GetParameters();
                if (parameters.Length > 0) {
                    var defaultValues = new object[parameters.Length];
                    for (var i = 0; i < parameters.Length; i++) {
                        defaultValues[i] = GetDefaultValue(parameters[i].ParameterType);
                    }
                    _methodParameters[method] = defaultValues;
                }
            }
        }
    }

    public override void OnInspectorGUI() {
        if (_buttonMethods.Count > 0) {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Methods", EditorStyles.boldLabel);

            foreach (var method in _buttonMethods) {
                DrawButton(method);
            }

            EditorGUILayout.Space();
        }

        DrawDefaultInspector();
    }

    private void DrawButton(MethodInfo method) {
        var buttonAttribute = method.GetCustomAttribute<ButtonAttribute>();
        var buttonLabel = !string.IsNullOrEmpty(buttonAttribute.Label)
            ? buttonAttribute.Label
            : ObjectNames.NicifyVariableName(method.Name);

        var parameters = method.GetParameters();

        if (parameters.Length == 0) {
            if (GUILayout.Button(buttonLabel)) {
                InvokeMethod(method);
            }
        } else {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(buttonLabel, EditorStyles.boldLabel);

            if (!_methodParameters.ContainsKey(method)) {
                var defaultValues = new object[parameters.Length];
                for (var i = 0; i < parameters.Length; i++) {
                    defaultValues[i] = GetDefaultValue(parameters[i].ParameterType);
                }
                _methodParameters[method] = defaultValues;
            }

            var paramValues = _methodParameters[method];

            for (var i = 0; i < parameters.Length; i++) {
                var param = parameters[i];
                var paramLabel = ObjectNames.NicifyVariableName(param.Name);
                paramValues[i] = DrawParameterField(paramLabel, param.ParameterType, paramValues[i]);
            }

            if (GUILayout.Button("Invoke")) {
                InvokeMethod(method, paramValues);
            }

            EditorGUILayout.EndVertical();
        }
    }

    private object DrawParameterField(string label, Type type, object value) {
        if (type == typeof(int)) {
            return EditorGUILayout.IntField(label, value != null ? (int)value : 0);
        }
        if (type == typeof(float)) {
            return EditorGUILayout.FloatField(label, value != null ? (float)value : 0f);
        }
        if (type == typeof(double)) {
            return EditorGUILayout.DoubleField(label, value != null ? (double)value : 0.0);
        }
        if (type == typeof(string)) {
            return EditorGUILayout.TextField(label, value as string ?? "");
        }
        if (type == typeof(bool)) {
            return EditorGUILayout.Toggle(label, value != null && (bool)value);
        }
        if (type.IsEnum) {
            return EditorGUILayout.EnumPopup(label, value as Enum ?? (Enum)Enum.GetValues(type).GetValue(0));
        }
        if (typeof(UnityEngine.Object).IsAssignableFrom(type)) {
            return EditorGUILayout.ObjectField(label, value as UnityEngine.Object, type, true);
        }
        if (type == typeof(Vector2)) {
            return EditorGUILayout.Vector2Field(label, value != null ? (Vector2)value : Vector2.zero);
        }
        if (type == typeof(Vector3)) {
            return EditorGUILayout.Vector3Field(label, value != null ? (Vector3)value : Vector3.zero);
        }
        if (type == typeof(Vector4)) {
            return EditorGUILayout.Vector4Field(label, value != null ? (Vector4)value : Vector4.zero);
        }
        if (type == typeof(Color)) {
            return EditorGUILayout.ColorField(label, value != null ? (Color)value : Color.white);
        }

        EditorGUILayout.LabelField(label, $"Unsupported type: {type.Name}");
        return value;
    }

    private object GetDefaultValue(Type type) {
        if (type.IsValueType) {
            return Activator.CreateInstance(type);
        }
        if (type == typeof(string)) {
            return "";
        }
        return null;
    }

    private void InvokeMethod(MethodInfo method, params object[] parameters) {
        foreach (var t in targets) {
            try {
                method.Invoke(t, parameters);
            } catch (Exception ex) {
                Debug.LogError($"Error invoking method {method.Name} on {t.name}: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}