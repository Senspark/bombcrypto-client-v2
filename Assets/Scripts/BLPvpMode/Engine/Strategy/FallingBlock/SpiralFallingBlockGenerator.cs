using System;
using System.Linq;

using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.Strategy.Matrix;

using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Strategy.FallingBlock {
    public class SpiralFallingBlockGenerator : IFallingBlockGenerator {
        [NotNull]
        private readonly IMatrixStrategy _strategy;

        private readonly int _offset;
        private readonly float _delayFraction;
        private readonly int _interval;

        public SpiralFallingBlockGenerator(
            [NotNull] IMatrixStrategy strategy,
            int offset,
            float delayFraction,
            int interval
        ) {
            _strategy = strategy;
            _offset = offset;
            _delayFraction = delayFraction;
            _interval = interval;
        }

        public IFallingBlockInfo[] Generate(int width, int height, int playTime) {
            IMatrixState state = new MatrixState(
                left: _offset,
                right: width - _offset - 1,
                top: _offset,
                bottom: height - _offset - 1,
                positions: Array.Empty<Vector2Int>()
            );
            state = _strategy.Process(state);
            var delay = (int) (playTime * _delayFraction);
            return state.Positions.Select((position, index) =>
                    (IFallingBlockInfo) new FallingBlockInfo(
                        timestamp: delay + index * _interval,
                        x: position.x,
                        y: height - position.y - 1
                    ))
                .ToArray();
        }
    }
}