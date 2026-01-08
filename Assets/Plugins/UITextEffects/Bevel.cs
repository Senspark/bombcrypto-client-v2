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

[AddComponentMenu("UI/ToJ Effects/Outer Bevel", 4)]
[RequireComponent(typeof(Text))]
public class Bevel : BaseMeshEffect
{
	[SerializeField]
	private Color m_HighlightColor = new Color(1f, 1f, 1f, 1f);

	[SerializeField]
	private Color m_ShadowColor = new Color(0f, 0f, 0f, 1f);

	[SerializeField]
	private Vector2 m_BevelDirectionAndDepth = new Vector2(1f, 1f);

	[SerializeField]
	private bool m_UseGraphicAlpha = true;


	protected Bevel () { }

	#if UNITY_EDITOR
	protected override void OnValidate ()
	{
		highlightColor = m_HighlightColor;
		shadowColor = m_ShadowColor;
		bevelDirectionAndDepth = m_BevelDirectionAndDepth;
		useGraphicAlpha = m_UseGraphicAlpha;
		base.OnValidate();
	}
	#endif

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

	public bool useGraphicAlpha
	{
		get { return m_UseGraphicAlpha; }
		set
		{
			m_UseGraphicAlpha = value;
			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	protected void ApplyShadowZeroAlloc(List<UIVertex> verts, Color32 color, int start, int end, float x, float y)
	{
		UIVertex vt;

		for (int i = start; i < end; ++i)
		{
			vt = verts[i];
			verts.Add(vt);

			Vector3 v = vt.position;
			v.x += x;
			v.y += y;
			vt.position = v;
			var newColor = color;
			if (useGraphicAlpha)
			{
				newColor.a = (byte)((newColor.a * verts[i].color.a) / 255);
			}
			vt.color = newColor;
			verts[i] = vt;
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

		int initialVertexCount = verts.Count;

        var start = 0;
        var end = 0;

		// shadow
		start = end;
		end = verts.Count;
		ApplyShadowZeroAlloc(verts, shadowColor, start, verts.Count, bevelDirectionAndDepth.x * 0.75f, -bevelDirectionAndDepth.y * 0.75f);

		start = end;
		end = verts.Count;
		ApplyShadowZeroAlloc(verts, shadowColor, start, verts.Count, bevelDirectionAndDepth.x, bevelDirectionAndDepth.y * 0.5f);

		start = end;
		end = verts.Count;
		ApplyShadowZeroAlloc(verts, shadowColor, start, verts.Count, -bevelDirectionAndDepth.x * 0.5f, -bevelDirectionAndDepth.y);

		// highlight
		start = end;
		end = verts.Count;
		ApplyShadowZeroAlloc(verts, highlightColor, start, verts.Count, -bevelDirectionAndDepth.x, bevelDirectionAndDepth.y * 0.5f);

		start = end;
		end = verts.Count;
		ApplyShadowZeroAlloc(verts, highlightColor, start, verts.Count, -bevelDirectionAndDepth.x * 0.5f, bevelDirectionAndDepth.y);


		if (GetComponent<Text>().material.shader == Shader.Find("Text Effects/Fancy Text"))
		{
			for (int i = 0; i < verts.Count - initialVertexCount; i++)
			{
				UIVertex vert = verts[i];
				vert.uv1 = new Vector2(0, 0);
				verts[i] = vert;
			}
		}

        vh.Clear();
        vh.AddUIVertexTriangleStream(verts);
    }
}