//-------------------------------------------------------------------------------
// Copyright (c) 2016 Tag of Joy
// E-mail: info@tagofjoy.lt
// To use this, you must have purchased it on the Unity Asset Store (http://u3d.as/n57)
// Sharing or distribution are not permitted
//-------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;

[AddComponentMenu("UI/ToJ Effects/Inner Bevel", 19)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Text))]
public class InnerBevel : BaseMeshEffect, IMaterialModifier
{
	public enum ColorMode
	{
		Override,
		Additive,
		Multiply
	}

	[SerializeField]
	private ColorMode m_HighlightColorMode = ColorMode.Override;

	[SerializeField]
	public Color m_HighlightColor = Color.white;

	[SerializeField]
	private ColorMode m_ShadowColorMode = ColorMode.Override;

	[SerializeField]
	public Color m_ShadowColor = Color.black;

	[SerializeField]
	private Vector2 m_BevelDirectionAndDepth = new Vector2(1f, 1f);

	private bool m_NeedsToSetMaterialDirty = false;

	private Material m_ModifiedMaterial;


	protected InnerBevel () { }

	#if UNITY_EDITOR
	protected override void OnValidate ()
	{
		highlightColorMode = m_HighlightColorMode;
		highlightColor = m_HighlightColor;
		shadowColorMode = m_ShadowColorMode;
		shadowColor = m_ShadowColor;
		bevelDirectionAndDepth = m_BevelDirectionAndDepth;
		base.OnValidate();
	}
	#endif

	protected override void Start ()
	{
		if (graphic != null)
		{
			graphic.SetMaterialDirty();
		}
	}

	public ColorMode highlightColorMode
	{
		get { return m_HighlightColorMode; }
		set
		{
			m_HighlightColorMode = value;
			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	public Color highlightColor
	{
		get { return m_HighlightColor; }
		set
		{
			m_HighlightColor = value;
			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	public ColorMode shadowColorMode
	{
		get { return m_ShadowColorMode; }
		set
		{
			m_ShadowColorMode = value;
			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	public Color shadowColor
	{
		get { return m_ShadowColor; }
		set
		{
			m_ShadowColor = value;
			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	public Vector2 bevelDirectionAndDepth
	{
		get { return m_BevelDirectionAndDepth; }
		set
		{
			if (m_BevelDirectionAndDepth == value)
			{
				return;
			}

			m_BevelDirectionAndDepth = value;

			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	public override void ModifyMesh (VertexHelper vh)
	{
		if (!IsActive())
		{
			return;
		}

		List<UIVertex> verts = new List<UIVertex>();
		vh.GetUIVertexStream(verts);

		int count = verts.Count;
		UIVertex uiVertex;

		for (int i = 0; i < count; i += 6)
		{
			uiVertex = verts[i];

			Vector2 right = (verts[i + 1].uv0 - verts[i].uv0).normalized;
			Vector2 up = (verts[i + 1].uv0 - verts[i + 2].uv0).normalized;
			Vector4 rightUp = (Vector4)right;
			rightUp.z = up.x;
			rightUp.w = up.y;
			uiVertex.tangent = rightUp;

			if (uiVertex.uv1 is { x: 0, y: 0 })
			{
				uiVertex.uv1 = new Vector2(1, 1);
			}

			verts[i] = uiVertex;

			for (int j = 1; j < 6; j++)
			{
				uiVertex = verts[i + j];
				uiVertex.tangent = verts[i].tangent;
				if (uiVertex.uv1 is { x: 0, y: 0 })
				{
					uiVertex.uv1 = new Vector2(1, 1);
				}
				verts[i + j] = uiVertex;
			}
		}

		vh.Clear();
		vh.AddUIVertexTriangleStream(verts);
	}


	void Update ()
	{
		if (!m_NeedsToSetMaterialDirty)
		{
			return;
		}

		if (graphic != null)
		{
			graphic.SetMaterialDirty();
		}
	}

	public virtual Material GetModifiedMaterial (Material baseMaterial)
	{
		if (!IsActive())
		{
			return baseMaterial;
		}

		if (baseMaterial.shader != Shader.Find("Text Effects/Fancy Text"))
		{
			Debug.Log("\"" + gameObject.name + "\"" + " doesn't have the \"Fancy Text\" shader applied. Please use it if you are using the \"Inner Bevel\" effect.");
			return baseMaterial;
		}

		if (m_ModifiedMaterial == null)
		{
			m_ModifiedMaterial = new Material(baseMaterial);
		}
		m_ModifiedMaterial.CopyPropertiesFromMaterial(baseMaterial);
		m_ModifiedMaterial.name = baseMaterial.name + " with IB";
		m_ModifiedMaterial.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;


		m_ModifiedMaterial.shaderKeywords = baseMaterial.shaderKeywords;
		m_ModifiedMaterial.CopyPropertiesFromMaterial(baseMaterial);

		m_ModifiedMaterial.EnableKeyword("_USEBEVEL_ON");
		m_ModifiedMaterial.SetColor("_HighlightColor", highlightColor);
		m_ModifiedMaterial.SetColor("_ShadowColor", shadowColor);
		m_ModifiedMaterial.SetVector("_HighlightOffset", bevelDirectionAndDepth / 500f);
		m_ModifiedMaterial.SetInt("_HighlightColorMode", (int)highlightColorMode);
		m_ModifiedMaterial.SetInt("_ShadowColorMode", (int)shadowColorMode);

		m_NeedsToSetMaterialDirty = true;

		return m_ModifiedMaterial;
	}
}