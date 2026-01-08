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

[AddComponentMenu("UI/ToJ Effects/Skew Effect", 5)]
[RequireComponent(typeof(Text))]
public class SkewEffect : BaseMeshEffect
{
	public enum SkewMode
	{
		TextArea = 0,
		FullRect = 1
	}

	[SerializeField]
	private SkewMode m_SkewMode = SkewMode.TextArea;

	[SerializeField]
	private Vector2 m_UpperLeftOffset = Vector2.zero;

	[SerializeField]
	private Vector2 m_UpperRightOffset = Vector2.zero;

	[SerializeField]
	private Vector2 m_LowerLeftOffset = Vector2.zero;

	[SerializeField]
	private Vector2 m_LowerRightOffset = Vector2.zero;


	protected SkewEffect() { }

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		upperLeftOffset = m_UpperLeftOffset;
		upperRightOffset = m_UpperRightOffset;
		lowerLeftOffset = m_LowerLeftOffset;
		lowerRightOffset = m_LowerRightOffset;
		base.OnValidate();
	}
	#endif

	public Vector2 upperLeftOffset
	{
		get { return m_UpperLeftOffset; }
		set
		{
			m_UpperLeftOffset = value;
			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	public Vector2 upperRightOffset
	{
		get { return m_UpperRightOffset; }
		set
		{
			m_UpperRightOffset = value;
			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	public Vector2 lowerLeftOffset
	{
		get { return m_LowerLeftOffset; }
		set
		{
			m_LowerLeftOffset = value;
			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	public Vector2 lowerRightOffset
	{
		get { return m_LowerRightOffset; }
		set
		{
			m_LowerRightOffset = value;
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

		if (verts.Count == 0)
		{
			return;
		}

		Vector2 topLeftPos = Vector2.zero;
		Vector2 bottomRightPos = Vector2.zero;
		if (m_SkewMode == SkewMode.FullRect)
		{
			Rect rect = GetComponent<RectTransform>().rect;
			topLeftPos = new Vector2(rect.xMin, rect.yMax);
			bottomRightPos = new Vector2(rect.xMax, rect.yMin);
		}
		else
		{
			topLeftPos = verts[0].position;
			bottomRightPos = verts[verts.Count - 1].position;

			for (int i = 0; i < verts.Count; i++)
			{
				if (verts[i].position.x < topLeftPos.x)
				{
					topLeftPos.x = verts[i].position.x;
				}
				if (verts[i].position.y > topLeftPos.y)
				{
					topLeftPos.y = verts[i].position.y;
				}

				if (verts[i].position.x > bottomRightPos.x)
				{
					bottomRightPos.x = verts[i].position.x;
				}
				if (verts[i].position.y < bottomRightPos.y)
				{
					bottomRightPos.y = verts[i].position.y;
				}
			}
		}

		float overallHeight = topLeftPos.y - bottomRightPos.y;
		float overallWidth = bottomRightPos.x - topLeftPos.x;

		for (int i = 0; i < verts.Count; i++)
		{
			UIVertex uiVertex = verts[i];

			float topWeightY = (uiVertex.position.y - bottomRightPos.y) / overallHeight;
			float bottomWeightY = 1 - topWeightY;

			float leftWeightX = (bottomRightPos.x - uiVertex.position.x) / overallWidth;
			float rightWeightX = 1 - leftWeightX;

			Vector3 additionalPos = Vector3.zero;
			additionalPos.y = (upperLeftOffset.y * topWeightY + lowerLeftOffset.y * bottomWeightY) * leftWeightX +
				(upperRightOffset.y * topWeightY + lowerRightOffset.y * bottomWeightY) * rightWeightX;

			additionalPos.x = (upperLeftOffset.x * leftWeightX + upperRightOffset.x * rightWeightX) * topWeightY +
				(lowerLeftOffset.x * leftWeightX + lowerRightOffset.x * rightWeightX) * bottomWeightY;

			uiVertex.position += additionalPos;

			verts[i] = uiVertex;
		}

        vh.Clear();
		vh.AddUIVertexTriangleStream(verts);
    }
}