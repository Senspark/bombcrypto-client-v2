//-------------------------------------------------------------------------------
// Copyright (c) 2016 Tag of Joy
// E-mail: info@tagofjoy.lt
// To use this, you must have purchased it on the Unity Asset Store (http://u3d.as/n57)
// Sharing or distribution are not permitted
//-------------------------------------------------------------------------------

using UnityEngine;

using System.Collections.Generic;

using UnityEngine.UI;

[AddComponentMenu("UI/ToJ Effects/Overlay Texture", 18)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Text))]
public class OverlayTexture : BaseMeshEffect, IMaterialModifier {
    public enum TextureMode {
        Local = 0,
        GlobalTextArea = 1,
        GlobalFullRect = 2
    }

    public enum ColorMode {
        Override,
        Additive,
        Multiply
    }

    [SerializeField]
    private TextureMode m_TextureMode = TextureMode.Local;

    [SerializeField]
    private ColorMode m_ColorMode = ColorMode.Override;

    [SerializeField]
    public Texture2D m_OverlayTexture;
    
    [SerializeField]
    public Color32 m_OverlayColor;

    private bool m_NeedsToSetMaterialDirty = false;

    private Material m_ModifiedMaterial;

    protected OverlayTexture() {
    }

#if UNITY_EDITOR
    protected override void OnValidate() {
        textureMode = m_TextureMode;
        colorMode = m_ColorMode;
        overlayTexture = m_OverlayTexture;
        base.OnValidate();
    }
#endif
    
    protected override void OnDisable() {
        base.OnDisable();
        if (graphic != null) {
            Text text = graphic as Text;
            if (text != null) {
                text.color = Color.white;
            }
            graphic.SetMaterialDirty();
        }
    }
    
    protected override void Start() {
        if (graphic != null) {
            graphic.SetMaterialDirty();
        }
    }

    public TextureMode textureMode {
        get {
            return m_TextureMode;
        }
        set {
            m_TextureMode = value;
            if (graphic != null)
                graphic.SetVerticesDirty();
        }
    }

    public ColorMode colorMode {
        get {
            return m_ColorMode;
        }
        set {
            m_ColorMode = value;
            if (graphic != null)
                graphic.SetVerticesDirty();
        }
    }

    public Texture2D overlayTexture {
        get {
            return m_OverlayTexture;
        }
        set {
            m_OverlayTexture = value;
            if (graphic != null)
                graphic.SetVerticesDirty();
        }
    }
    
    public Color overlayColor {
        get { return m_OverlayColor; }
        set {
            m_OverlayColor = value;
            if (graphic != null)
                graphic.SetMaterialDirty();
        }
    }

    public override void ModifyMesh(VertexHelper vh) {
        if (!IsActive()) {
            return;
        }

        List<UIVertex> verts = new List<UIVertex>();
        vh.GetUIVertexStream(verts);

        int count = verts.Count;
        UIVertex uiVertex;

        if (verts.Count == 0) {
            return;
        }

        if ((textureMode == TextureMode.GlobalTextArea) || (textureMode == TextureMode.GlobalFullRect)) {
            Vector2 topLeftPos = Vector2.zero;
            Vector2 bottomRightPos = Vector2.zero;
            if (textureMode == TextureMode.GlobalFullRect) {
                Rect rect = GetComponent<RectTransform>().rect;
                topLeftPos = new Vector2(rect.xMin, rect.yMax);
                bottomRightPos = new Vector2(rect.xMax, rect.yMin);
            } else {
                topLeftPos = verts[0].position;
                bottomRightPos = verts[verts.Count - 1].position;

                for (int i = 0; i < verts.Count; i++) {
                    if (verts[i].position.x < topLeftPos.x) {
                        topLeftPos.x = verts[i].position.x;
                    }
                    if (verts[i].position.y > topLeftPos.y) {
                        topLeftPos.y = verts[i].position.y;
                    }

                    if (verts[i].position.x > bottomRightPos.x) {
                        bottomRightPos.x = verts[i].position.x;
                    }
                    if (verts[i].position.y < bottomRightPos.y) {
                        bottomRightPos.y = verts[i].position.y;
                    }
                }
            }

            float overallHeight = topLeftPos.y - bottomRightPos.y;
            float overallWidth = bottomRightPos.x - topLeftPos.x;

            for (int i = 0; i < count; i++) {
                uiVertex = verts[i];

                uiVertex.uv1 = new Vector2(1 + (uiVertex.position.x - topLeftPos.x) / overallWidth, 1 + 1f - (topLeftPos.y - uiVertex.position.y) / overallHeight);

                verts[i] = uiVertex;
            }
        } else {
            for (int i = 0; i < count; i++) {
                uiVertex = verts[i];

                uiVertex.uv1 = new Vector2(1 + (((i % 6 == 0) || (i % 6 == 5) || (i % 6 == 4)) ? 0 : 1), 1 + (((i % 6 == 2) || (i % 6 == 3) || (i % 6 == 4)) ? 0 : 1));

                verts[i] = uiVertex;
            }
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(verts);
    }

    void Update() {
        if (!m_NeedsToSetMaterialDirty) {
            return;
        }

        if (graphic != null) {
            graphic.SetMaterialDirty();
        }
    }

    public virtual Material GetModifiedMaterial(Material baseMaterial) {
        if (!IsActive()) {
            return baseMaterial;
        }

        if (baseMaterial.shader != Shader.Find("Text Effects/Fancy Text")) {
            Debug.Log("\"" + gameObject.name + "\"" + " doesn't have the \"Fancy Text\" shader applied. Please use it if you are using the \"Overlay Texture\" effect.");
            return baseMaterial;
        }

        if (m_ModifiedMaterial == null) {
            m_ModifiedMaterial = new Material(baseMaterial);
        }
        m_ModifiedMaterial.CopyPropertiesFromMaterial(baseMaterial);
        m_ModifiedMaterial.name = baseMaterial.name + " with OT";
        m_ModifiedMaterial.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

        m_ModifiedMaterial.shaderKeywords = baseMaterial.shaderKeywords;
        m_ModifiedMaterial.CopyPropertiesFromMaterial(baseMaterial);

        m_ModifiedMaterial.EnableKeyword("_USEOVERLAYTEXTURE_ON");
        m_ModifiedMaterial.SetTexture("_OverlayTex", overlayTexture);
        m_ModifiedMaterial.SetInt("_OverlayTextureColorMode", (int)colorMode);
        m_ModifiedMaterial.SetColor("_Color", overlayColor);
        
        m_NeedsToSetMaterialDirty = true;

        return m_ModifiedMaterial;
    }
}