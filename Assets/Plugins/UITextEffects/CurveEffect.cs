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

[AddComponentMenu("UI/ToJ Effects/Curve Effect", 6)]
[RequireComponent(typeof(Text))]
public class CurveEffect : BaseMeshEffect
{
	public enum CurveMode
	{
		TextArea = 0,
		FullRect = 1
	}

	[SerializeField]
	private CurveMode m_CurveMode = CurveMode.TextArea;

	[SerializeField]
	private AnimationCurve m_Curve = new AnimationCurve(new Keyframe(0, 0, 0, 2), new Keyframe(1, 0, -2, 0));

	[SerializeField]
	private float m_Strength = 1;

	protected CurveEffect () { }

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		curve = m_Curve;
		strength = m_Strength;
		base.OnValidate();
	}
	#endif

	public AnimationCurve curve
	{
		get { return m_Curve; }
		set
		{
			if (m_Curve == value)
				return;
			m_Curve = value;
			if (graphic != null)
				graphic.SetVerticesDirty();
		}
	}

	public float strength
	{
		get { return m_Strength; }
		set
		{
			if (m_Strength == value)
				return;
			m_Strength = value;
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
		if (m_CurveMode == CurveMode.FullRect)
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

		float overallWidth = bottomRightPos.x - topLeftPos.x;

		for (int index = 0; index < verts.Count; index++)
		{
			UIVertex vert = verts[index];
			vert.position.y += curve.Evaluate((vert.position.x - topLeftPos.x) / overallWidth) * strength;
			verts[index] = vert;
		}

		vh.Clear();
		vh.AddUIVertexTriangleStream(verts);
	}
}
