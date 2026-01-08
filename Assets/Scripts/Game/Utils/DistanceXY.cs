using UnityEngine;

public static class DistanceXY
{
    public static float DistancePower2(Vector3 v1, Vector3 v2) {
        var dX = v1.x - v2.x;
        var dY = v1.y - v2.y;
        return (dX * dX) + (dY * dY);
    }

    public static int DistancePower2(Vector2Int v1, Vector2Int v2) {
        var dX = v1.x - v2.x;
        var dY = v1.y - v2.y;
        return (dX * dX) + (dY * dY);
    }

    public static float DistancePower2(float x1, float y1, float x2, float y2) {
        var dX = x1 - x2;
        var dY = y1 - y2;
        return (dX * dX) + (dY * dY);
    }
    
    public static bool Equal(Vector3 v1, Vector3 v2) {
        return DistancePower2(v1, v2) < 0.0025f; // = 0.05f * 0.05f;
    }
    
    public static bool EqualInMoving(Vector3 v1, Vector3 v2) {
        return DistancePower2(v1, v2) < 0.25; // = 0.5f * 0.5f;
    }

}