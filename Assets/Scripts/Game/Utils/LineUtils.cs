
using UnityEngine;

namespace Engine.Utils
{
    public static class LineUtils
    {
        public static Vector2 PointFromEndOfLine(Vector2 start, Vector2 end, float distance)
        {
            var x = end.x - start.x;
            var y = end.y - start.y;
            var z = Mathf.Sqrt(x * x + y * y); //Pathagrean Theorum for Hypotenuse
            var ratio = distance / z;
            var deltaX = x * ratio;
            var deltaY = y * ratio;

            return new Vector2(end.x + deltaX, end.y + deltaY);
        }

    }
}
