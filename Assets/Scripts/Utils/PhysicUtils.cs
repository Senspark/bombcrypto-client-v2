namespace Utils {
    public static class PhysicUtils {
        public struct Box {
            public (float X, float Y) Point;
            public (float X, float Y) Size;
        }

        public static bool IsOverlap(Box box, (float, float) point) {
            var (x, y) = point;
            var halfHeight = box.Size.Y / 2;
            var halfWidth = box.Size.X / 2;
            var bottom = box.Point.X - halfHeight;
            var left = box.Point.X - halfWidth;
            var right = box.Point.X + halfWidth;
            var top = box.Point.X + halfHeight;
            return box.Point.X < right && box.Point.X > left && box.Point.Y < top && box.Point.Y > bottom;
        }
    }
}