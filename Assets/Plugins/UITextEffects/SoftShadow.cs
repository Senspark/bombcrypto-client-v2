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

[AddComponentMenu("UI/ToJ Effects/Soft Shadow", 3)]
[RequireComponent(typeof(Text))]
public class SoftShadow : Shadow
{
	[SerializeField]
	private float m_BlurSpread = 1;

	[SerializeField]
	private bool m_OnlyInitialCharactersDropShadow = true;


	protected SoftShadow () { }

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		blurSpread = m_BlurSpread;
		onlyInitialCharactersDropShadow = m_OnlyInitialCharactersDropShadow;
		base.OnValidate();
	}
	#endif

	public float blurSpread
	{
		get { return m_BlurSpread; }
		set
		{
			m_BlurSpread = value;
			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	public bool onlyInitialCharactersDropShadow
	{
		get { return m_OnlyInitialCharactersDropShadow; }
		set
		{
			m_OnlyInitialCharactersDropShadow = value;
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

		int initialVertexCount = verts.Count;

		Text textComponent = GetComponent<Text>();

		List<UIVertex> neededVerts = new List<UIVertex>();

        neededVerts = verts;

        Color effectColorEdited = effectColor;
		effectColorEdited.a /= 4;

		var start = 0;
		var end = neededVerts.Count;
		ApplyShadowZeroAlloc(neededVerts, effectColorEdited, start, neededVerts.Count, effectDistance.x, effectDistance.y);

		for (int i = -1 ; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if (!((i == 0) && (j == 0)))
				{
					start = end;
					end = neededVerts.Count;
					ApplyShadowZeroAlloc(neededVerts, effectColorEdited, start, neededVerts.Count, effectDistance.x + i * blurSpread, effectDistance.y + j * blurSpread);
				}
			}
		}

		if (onlyInitialCharactersDropShadow)
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