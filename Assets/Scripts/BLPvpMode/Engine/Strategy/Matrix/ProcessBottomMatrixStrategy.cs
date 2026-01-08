using System.Linq;

using UnityEngine;

namespace BLPvpMode.Engine.Strategy.Matrix {
    public class ProcessBottomMatrixStrategy : IMatrixStrategy {
        private readonly bool _ccw;

        public ProcessBottomMatrixStrategy(bool ccw) {
            _ccw = ccw;
        }

        public IMatrixState Process(IMatrixState state) {
            var range = _ccw
                ? Enumerable.Range(state.Left, state.Right - state.Left + 1)
                : Enumerable.Range(state.Left, state.Right - state.Left + 1).Reverse();
            return new MatrixState(
                left: state.Left,
                right: state.Right,
                top: state.Top,
                bottom: state.Bottom,
                positions: state.Positions
                    .Concat(range.Select(it => new Vector2Int(it, state.Bottom)))
                    .ToArray()
            );
        }
    }
}