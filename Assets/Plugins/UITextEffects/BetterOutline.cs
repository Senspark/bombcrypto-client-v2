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

[AddComponentMenu("UI/ToJ Effects/Better Outline", 0)]
[RequireComponent(typeof(Text))]
public class BetterOutline : Shadow
{

	protected BetterOutline() { }

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
	}
	#endif

    public override void ModifyMesh(VertexHelper vh)
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

		for (int i = -1 ; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if ((i != 0) && (j != 0))
				{
					start = end;
					end = verts.Count;
					ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, i * effectDistance.x * 0.707f, j * effectDistance.y * 0.707f);
				}
			}
		}

		start = end;
		end = verts.Count;
		ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -effectDistance.x, 0);

		start = end;
		end = verts.Count;
		ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, effectDistance.x, 0);


		start = end;
		end = verts.Count;
		ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, 0, -effectDistance.y);

		start = end;
		end = verts.Count;
		ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, 0, effectDistance.y);

			
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