using System.Linq;

using UnityEngine;

namespace BLPvpMode.Engine.Strategy.Matrix {
    public class ProcessRightMatrixStrategy : IMatrixStrategy {
        private readonly bool _ccw;

        public ProcessRightMatrixStrategy(bool ccw) {
            _ccw = ccw;
        }

        public IMatrixState Process(IMatrixState state) {
            var range = _ccw
                ? Enumerable.Range(state.Top, state.Bottom - state.Top + 1).Reverse()
                : Enumerable.Range(state.Top, state.Bottom - state.Top + 1);
            return new MatrixState(
                left: state.Left,
                right: state.Right,
                top: state.Top,
                bottom: state.Bottom,
                positions: state.Positions
                    .Concat(range.Select(it => new Vector2Int(state.Right, it)))
                    .ToArray()
            );
        }
    }
}