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

[AddComponentMenu("UI/ToJ Effects/Limit Visible Characters", 8)]
[RequireComponent(typeof(Text))]
public class LimitVisibleCharacters : BaseMeshEffect
{
	private const string REGEX_TAGS = @"<b>|</b>|<i>|</i>|<size=.*?>|</size>|<color=.*?>|</color>|<material=.*?>|</material>";

	[SerializeField]
	private int m_VisibleCharacterCount = 0;


	protected LimitVisibleCharacters () { }

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		visibleCharacterCount = m_VisibleCharacterCount;
		base.OnValidate();
	}
	#endif

	public int visibleCharacterCount
	{
		get { return m_VisibleCharacterCount; }
		set
		{
			if (m_VisibleCharacterCount == value)
				return;
			m_VisibleCharacterCount = value;
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

		Text textComponent = GetComponent<Text>();

		List<UIVertex> neededVerts = new List<UIVertex>();

		IEnumerator matchedTagCollection = null;
		Match currentMatchedTag = null;

		string text = textComponent.text.Substring(0, textComponent.cachedTextGenerator.characterCountVisible);
		int textLength = text.Length;

		if (textComponent.supportRichText)
		{
			matchedTagCollection = GetRegexMatchedTags(text, out textLength).GetEnumerator();
			currentMatchedTag = null;
			if (matchedTagCollection.MoveNext())
			{
				currentMatchedTag = (Match)matchedTagCollection.Current;
			}
		}

		if (visibleCharacterCount >= textLength)
		{
			return;
		}

		int charIndex = 0;
		while (neededVerts.Count < visibleCharacterCount * 6)
		{
			bool hasToSkip = false;
			if (textComponent.supportRichText)
			{
				if (currentMatchedTag != null && currentMatchedTag.Index == charIndex)
				{
					charIndex += currentMatchedTag.Length - 1;

					currentMatchedTag = null;
					if (matchedTagCollection.MoveNext())
					{
						currentMatchedTag = (Match)matchedTagCollection.Current;
					}

					hasToSkip = true;
				}
			}

			if (hasToSkip == false)
			{
				for (int i = 0; i < 6; i++)
				{
					UIVertex vert = verts[charIndex * 6 + i];
					neededVerts.Add(vert);
				}
			}
			charIndex++;
		}

		vh.Clear();
		vh.AddUIVertexTriangleStream(neededVerts);
	}

	private MatchCollection GetRegexMatchedTags(string text, out int lengthWithoutTags)
	{
		MatchCollection matchedTags = Regex.Matches(text, REGEX_TAGS);
		lengthWithoutTags = 0;
		int overallTagLength = 0;

		foreach (Match matchedTag in matchedTags)
		{
			overallTagLength += matchedTag.Length;
		}

		lengthWithoutTags = text.Length - overallTagLength;
		return matchedTags;
	}
}