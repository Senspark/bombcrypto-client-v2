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

[AddComponentMenu("UI/ToJ Effects/Inner Outline", 20)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Text))]
public class InnerOutline : BaseMeshEffect, IMaterialModifier
{
	public enum ColorMode
	{
		Override,
		Additive,
		Multiply
	}

	[SerializeField]
	private ColorMode m_ColorMode = ColorMode.Override;

	[SerializeField]
	public Color m_OutlineColor = Color.black;

	[SerializeField]
	private float m_OutlineThickness = 1f;

	private bool m_NeedsToSetMaterialDirty = false;

	private Material m_ModifiedMaterial;


	protected InnerOutline () { }

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		colorMode = m_ColorMode;
		outlineColor = m_OutlineColor;
		outlineThickness = m_OutlineThickness;
		base.OnValidate();
	}
	#endif

	protected override void Start()
	{
		if (graphic != null)
		{
			graphic.SetMaterialDirty();
		}
	}

	public ColorMode colorMode
	{
		get { return m_ColorMode; }
		set
		{
			m_ColorMode = value;
			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	public Color outlineColor
	{
		get { return m_OutlineColor; }
		set
		{
			m_OutlineColor = value;
			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	public float outlineThickness
	{
		get { return m_OutlineThickness; }
		set
		{
			m_OutlineThickness = value;
			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		if (!IsActive())
		{
			return;
		}

		List<UIVertex> verts = new List<UIVertex>();
		vh.GetUIVertexStream(verts);

		int count = verts.Count;
		UIVertex uiVertex;

		for (int i = 0; i < count; i++)
		{
			uiVertex = verts[i];

			if (uiVertex.uv1 is { x: 0, y: 0 })
			{
				uiVertex.uv1 = new Vector2(1, 1);
			}

			verts[i] = uiVertex;
		}

		vh.Clear();
		vh.AddUIVertexTriangleStream(verts);
	}


	void Update()
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

	public virtual Material GetModifiedMaterial(Material baseMaterial)
	{
		if (!IsActive())
		{
			return baseMaterial;
		}

		if (baseMaterial.shader != Shader.Find("Text Effects/Fancy Text"))
		{
			Debug.Log("\"" + gameObject.name + "\"" + " doesn't have the \"Fancy Text\" shader applied. Please use it if you are using the \"Inner Outline\" effect.");
			return baseMaterial;
		}

		if (m_ModifiedMaterial == null)
		{
			m_ModifiedMaterial = new Material(baseMaterial);
		}
		m_ModifiedMaterial.CopyPropertiesFromMaterial(baseMaterial);
		m_ModifiedMaterial.name = baseMaterial.name + " with IO";
		m_ModifiedMaterial.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;


		m_ModifiedMaterial.shaderKeywords = baseMaterial.shaderKeywords;
		m_ModifiedMaterial.CopyPropertiesFromMaterial(baseMaterial);

		m_ModifiedMaterial.EnableKeyword("_USEOUTLINE_ON");
		m_ModifiedMaterial.SetColor("_OutlineColor", outlineColor);
		m_ModifiedMaterial.SetFloat("_OutlineThickness", outlineThickness / 250f);
		m_ModifiedMaterial.SetInt("_OutlineColorMode", (int)colorMode);

		m_NeedsToSetMaterialDirty = true;

		return m_ModifiedMaterial;
	}
}