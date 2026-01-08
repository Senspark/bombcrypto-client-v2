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

[AddComponentMenu("UI/ToJ Effects/Depth Effect", 2)]
[RequireComponent(typeof(Text))]
public class DepthEffect : BaseMeshEffect
{
	[SerializeField]
	private Color m_EffectColor = new Color(0f, 0f, 0f, 1f);

	[SerializeField]
	private Vector2 m_EffectDirectionAndDepth = new Vector2(-1f, 1f);

	[SerializeField]
	private Vector2 m_DepthPerspectiveStrength = new Vector2(0, 0);

	[SerializeField]
	private bool m_OnlyInitialCharactersGenerateDepth = true;

	[SerializeField]
	private bool m_UseGraphicAlpha = true;

	private Vector2 m_OverallTextSize = Vector2.zero;
	private Vector2 m_TopLeftPos = Vector2.zero;
	private Vector2 m_BottomRightPos = Vector2.zero;


	protected DepthEffect () { }

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		effectColor = m_EffectColor;
		effectDirectionAndDepth = m_EffectDirectionAndDepth;
		depthPerspectiveStrength = m_DepthPerspectiveStrength;
		onlyInitialCharactersGenerateDepth = m_OnlyInitialCharactersGenerateDepth;
		useGraphicAlpha = m_UseGraphicAlpha;
		base.OnValidate();
	}
	#endif

	public Color effectColor
	{
		get { return m_EffectColor; }
		set
		{
			m_EffectColor = value;
			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	public Vector2 effectDirectionAndDepth
	{
		get { return m_EffectDirectionAndDepth; }
		set
		{
			if (m_EffectDirectionAndDepth == value)
			{
				return;
			}

			m_EffectDirectionAndDepth = value;

			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	public Vector2 depthPerspectiveStrength
	{
		get { return m_DepthPerspectiveStrength; }
		set
		{
			if (m_DepthPerspectiveStrength == value)
			{
				return;
			}

			m_DepthPerspectiveStrength = value;

			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	public bool onlyInitialCharactersGenerateDepth
	{
		get { return m_OnlyInitialCharactersGenerateDepth; }
		set
		{
			m_OnlyInitialCharactersGenerateDepth = value;
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

	protected void ApplyShadowZeroAlloc(List<UIVertex> verts, Color32 color, int start, int end, float x, float y, float factor)
	{
		UIVertex vt;

		for (int i = start; i < end; ++i)
		{
			vt = verts[i];
			verts.Add(vt);

			Vector3 v = vt.position;
			v.x += x * factor;
			if (depthPerspectiveStrength.x != 0)
			{
				v.x -= depthPerspectiveStrength.x * ((v.x - m_TopLeftPos.x) / m_OverallTextSize.x - 0.5f) * factor;
			}

			v.y += y * factor;
			if (depthPerspectiveStrength.y != 0)
			{
				v.y += depthPerspectiveStrength.y * ((m_TopLeftPos.y - v.y) / m_OverallTextSize.y - 0.5f) * factor;
			}

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

		Text textComponent = GetComponent<Text>();

		List<UIVertex> neededVerts = new List<UIVertex>();
		//if (m_OnlyInitialCharactersGenerateDepth)
		//{
		//	neededVerts = verts.GetRange(verts.Count - textComponent.cachedTextGenerator.characterCountVisible * 6, textComponent.cachedTextGenerator.characterCountVisible * 6);
		//}
		//else
		//{
			neededVerts = verts;
		//}

		if (neededVerts.Count == 0)
		{
			return;
		}

		if ((depthPerspectiveStrength.x != 0) || (depthPerspectiveStrength.y != 0))
		{
			m_TopLeftPos = neededVerts[0].position;
			m_BottomRightPos = neededVerts[neededVerts.Count - 1].position;

			for (int i = 0; i < neededVerts.Count; i++)
			{
				if (neededVerts[i].position.x < m_TopLeftPos.x)
				{
					m_TopLeftPos.x = neededVerts[i].position.x;
				}
				if (neededVerts[i].position.y > m_TopLeftPos.y)
				{
					m_TopLeftPos.y = neededVerts[i].position.y;
				}

				if (neededVerts[i].position.x > m_BottomRightPos.x)
				{
					m_BottomRightPos.x = neededVerts[i].position.x;
				}
				if (neededVerts[i].position.y < m_BottomRightPos.y)
				{
					m_BottomRightPos.y = neededVerts[i].position.y;
				}
			}

			m_OverallTextSize = new Vector2(m_BottomRightPos.x - m_TopLeftPos.x, m_TopLeftPos.y - m_BottomRightPos.y);
		}

        var start = 0;
        var end = 0;

		start = end;
		end = neededVerts.Count;
		ApplyShadowZeroAlloc(neededVerts, effectColor, start, neededVerts.Count, effectDirectionAndDepth.x, effectDirectionAndDepth.y, 0.25f);

		start = end;
		end = neededVerts.Count;
		ApplyShadowZeroAlloc(neededVerts, effectColor, start, neededVerts.Count, effectDirectionAndDepth.x, effectDirectionAndDepth.y, 0.5f);

		start = end;
		end = neededVerts.Count;
		ApplyShadowZeroAlloc(neededVerts, effectColor, start, neededVerts.Count, effectDirectionAndDepth.x, effectDirectionAndDepth.y, 0.75f);

		start = end;
		end = neededVerts.Count;
		ApplyShadowZeroAlloc(neededVerts, effectColor, start, neededVerts.Count, effectDirectionAndDepth.x, effectDirectionAndDepth.y, 1f);
			

		if (onlyInitialCharactersGenerateDepth)
		{
			neededVerts.RemoveRange(neededVerts.Count - textComponent.cachedTextGenerator.characterCountVisible * 6, textComponent.cachedTextGenerator.characterCountVisible * 6);
			neededVerts.AddRange(verts);
		}


		if (textComponent.material.shader == Shader.Find("Text Effects/Fancy Text"))
		{
			for (int i = 0; i < neededVerts.Count - initialVertexCount; i++)
			{
				UIVertex vert = neededVerts[i];
				vert.uv1 = new Vector2(0, 0);
				neededVerts[i] = vert;
			}
		}

        vh.Clear();
		vh.AddUIVertexTriangleStream(neededVerts);
    }
}