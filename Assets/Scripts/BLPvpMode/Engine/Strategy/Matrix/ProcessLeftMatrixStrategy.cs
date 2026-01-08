using System.Linq;

using UnityEngine;

namespace BLPvpMode.Engine.Strategy.Matrix {
    public class ProcessLeftMatrixStrategy : IMatrixStrategy {
        private readonly bool _ccw;

        public ProcessLeftMatrixStrategy(bool ccw) {
            _ccw = ccw;
        }

        public IMatrixState Process(IMatrixState state) {
            var range = _ccw
                ? Enumerable.Range(state.Top, state.Bottom - state.Top + 1)
                : Enumerable.Range(state.Top, state.Bottom - state.Top + 1).Reverse();
            return new MatrixState(
                left: state.Left,
                right: state.Right,
                top: state.Top,
                bottom: state.Bottom,
                positions: state.Positions
                    .Concat(range.Select(it => new Vector2Int(state.Left, it)))
                    .ToArray()
            );
        }
    }
}