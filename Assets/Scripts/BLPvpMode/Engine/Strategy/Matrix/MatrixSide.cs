using System.Linq;

namespace BLPvpMode.Engine.Strategy.Matrix {
    public enum MatrixSide {
        Top,
        Right,
        Bottom,
        Left,
    }

    public static class MatrixSideExtensions {
        public static MatrixSide[] NextSides(this MatrixSide side, bool ccw, int skip, int count) {
            var values = new[] {
                MatrixSide.Top, //
                MatrixSide.Right, // 
                MatrixSide.Bottom, // 
                MatrixSide.Left,
            };
            return Enumerable.Range(0, count).Select(it => {
                var index = skip + it;
                return values[((int) side + (ccw ? -index : +index) % values.Length + values.Length) % values.Length];
            }).ToArray();
        }
    }
}