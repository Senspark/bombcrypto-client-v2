using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[AddComponentMenu("UI/ToJ Effects/ToJ Outline", 15)]
public class ToJOutline : Shadow
{
	protected ToJOutline ()
    {}

    public override void ModifyMesh (VertexHelper vh)
    {
        if (!IsActive())
			return;

		List<UIVertex> verts = new List<UIVertex>();
		vh.GetUIVertexStream(verts);

		int initialVertexCount = verts.Count;

        var neededCpacity = verts.Count * 5;
        if (verts.Capacity < neededCpacity)
            verts.Capacity = neededCpacity;

        var start = 0;
        var end = verts.Count;
        ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, effectDistance.x, effectDistance.y);

        start = end;
        end = verts.Count;
        ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, effectDistance.x, -effectDistance.y);

        start = end;
        end = verts.Count;
        ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -effectDistance.x, effectDistance.y);

        start = end;
        end = verts.Count;
		ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -effectDistance.x, -effectDistance.y);


		Text textComponent = GetComponent<Text>();
		if ((textComponent != null) && (textComponent.material.shader == Shader.Find("Text Effects/Fancy Text")))
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
