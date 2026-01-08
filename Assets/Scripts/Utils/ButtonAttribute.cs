using System;

/// <summary>
/// Attribute to display a button in the Unity Inspector for a method.
/// Replacement for Odin Inspector's [Button] attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ButtonAttribute : Attribute {
    public string Label { get; private set; }

    public ButtonAttribute() {
        Label = null;
    }

    public ButtonAttribute(string label) {
        Label = label;
    }
}