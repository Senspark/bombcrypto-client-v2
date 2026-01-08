using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeneratePoints
{
    public struct BezierPoint
    {
        public Vector3 Position { get; }
        public float Angle { get; }
        public bool Flip { get; }

        public BezierPoint(Vector3 _position, float _angle, bool _flip)
        {
            Position = _position;
            Angle = _angle;
            Flip = _flip;
        }
    }

    public static BezierPoint StepBezierPoint(int step, Vector3 origin, Vector3 control, Vector3 destination, int seqments)
    {
        var t = (1.0f / seqments) * step;
        var postion = Mathf.Pow(1 - t, 2) * origin + 2.0f * (1 - t) * t * control + t * t * destination;
        var tangent = -(1 - t) * origin + (1 - 2 * t) * control + t * destination;
        var angle = Vector2.SignedAngle(tangent, Vector2.left);
        return new BezierPoint(postion, -angle, origin.x < destination.x);
    }

    public static BezierPoint StepCubicBezierPoint(int step, Vector3 origin, Vector3 control1, Vector3 control2, Vector3 destination, int segments)
    {
        var t = (1.0f / segments) * step;
        var position = Mathf.Pow(1 - t, 3) * origin + 3.0f * Mathf.Pow(1 - t, 2) * t * control1 + 3.0f * (1 - t) * t * t * control2 + t * t * t * destination;
        var tangent = -3 * Mathf.Pow(1 - t, 2) * origin + 3 * (1 - 4 * t + 3 * t * t) * control1 + 3 * (2 * t - 3 * t * t) * control2 + 3 * (t * t) * destination;
        var angle = Vector2.SignedAngle(tangent, Vector2.left);
        return new BezierPoint(position, -angle, origin.x < destination.x);
    }

    public static void GenerateCubicBezierPoints(Vector3 origin, Vector3 control1, Vector3 control2, Vector3 destination, int segments)
    {
        var vertices = new Vector3[segments + 1];

        float t = 0;
        for (var i = 0; i < segments; i++)
        {
            vertices[i].x = Mathf.Pow(1 - t, 3) * origin.x + 3.0f * Mathf.Pow(1 - t, 2) * t * control1.x + 3.0f * (1 - t) * t * t * control2.x + t * t * t * destination.x;
            vertices[i].y = Mathf.Pow(1 - t, 3) * origin.y + 3.0f * Mathf.Pow(1 - t, 2) * t * control1.y + 3.0f * (1 - t) * t * t * control2.y + t * t * t * destination.y;
            t += 1.0f / segments;
        }

        vertices[segments].x = destination.x;
        vertices[segments].y = destination.y;

    }


    public static Vector3[] GenerateBezierPoints(Vector3 origin, Vector3 control, Vector3 destination, int segments)
    {
        var vertices = new Vector3[segments + 1];

        var t = 0.0f;
        for (var i = 0; i < segments; i++)
        {
            vertices[i].x = Mathf.Pow(1 - t, 2) * origin.x + 2.0f * (1 - t) * t * control.x + t * t * destination.x;
            vertices[i].y = Mathf.Pow(1 - t, 2) * origin.y + 2.0f * (1 - t) * t * control.y + t * t * destination.y;
            vertices[i].z = 0;
            t += 1.0f / segments;
        }
        vertices[segments].x = destination.x;
        vertices[segments].y = destination.y;

        return vertices;
    }

    public static void SetLeft(this RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public static void SetRight(this RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public static void SetTop(this RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public static void SetBottom(this RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }

}
