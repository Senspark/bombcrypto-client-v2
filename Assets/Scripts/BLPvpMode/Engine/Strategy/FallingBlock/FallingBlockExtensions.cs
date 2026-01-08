using System;
using System.Collections.Generic;
using System.Linq;

using BLPvpMode.Engine.Config;
using BLPvpMode.Engine.Strategy.Matrix;

namespace BLPvpMode.Engine.Strategy.FallingBlock {
    using Generator = SpiralFallingBlockGenerator; // Shorten class name.

    public static class FallingBlockExtensions {
        public static IFallingBlockGenerator ToGenerator(this FallingBlockPattern pattern) {
            IMatrixStrategy SpiralStrategy(MatrixSide side, bool ccw, bool shrinkNextSide) {
                var sides = side.NextSides(ccw, 0, 4);
                return new ListMatrixStrategy(sides.Select(it => {
                        var items = new List<IMatrixStrategy> {
                            new ProcessMatrixStrategy(it, ccw), //
                            new ShrinkMatrixStrategy(it),
                        };
                        if (shrinkNextSide) {
                            var oppositeSide = it.NextSides(ccw, 2, 1)[0];
                            items.Add(new ShrinkMatrixStrategy(oppositeSide));
                        }
                        return (IMatrixStrategy) new ListMatrixStrategy(items.ToArray());
                    })
                    .ToArray());
            }
            var generator = pattern switch {
                FallingBlockPattern.TopLeftCw => new MultiFallingBlockGenerator(new IFallingBlockGenerator[] {
                    new Generator(SpiralStrategy(MatrixSide.Top, false, false).Loop(2), 0, 0.5f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Top, false, false).Loop(2), 2, 0.75f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Top, false, false).Loop(3), 4, 1f, 140),
                }),
                FallingBlockPattern.TopLeftCcw => new MultiFallingBlockGenerator(new IFallingBlockGenerator[] {
                    new Generator(SpiralStrategy(MatrixSide.Left, true, false).Loop(2), 0, 0.5f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Left, true, false).Loop(2), 2, 0.75f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Left, true, false).Loop(3), 4, 1f, 140),
                }),
                FallingBlockPattern.BottomRightCw => new MultiFallingBlockGenerator(new IFallingBlockGenerator[] {
                    new Generator(SpiralStrategy(MatrixSide.Bottom, false, false).Loop(2), 0, 0.5f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Bottom, false, false).Loop(2), 2, 0.75f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Bottom, false, false).Loop(3), 4, 1f, 140),
                }),
                FallingBlockPattern.BottomRightCcw => new MultiFallingBlockGenerator(new IFallingBlockGenerator[] {
                    new Generator(SpiralStrategy(MatrixSide.Right, true, false).Loop(2), 0, 0.5f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Right, true, false).Loop(2), 2, 0.75f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Right, true, false).Loop(3), 4, 1f, 140),
                }),
                FallingBlockPattern.TopLeftDualCw => new MultiFallingBlockGenerator(new IFallingBlockGenerator[] {
                    new Generator(SpiralStrategy(MatrixSide.Top, false, true).Loop(1), 0, 0.5f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Top, false, true).Loop(1), 2, 0.75f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Top, false, true).Loop(2), 4, 1f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Bottom, false, true).Loop(1), 0, 0.5f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Bottom, false, true).Loop(1), 2, 0.75f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Bottom, false, true).Loop(2), 4, 1f, 140),
                }),
                FallingBlockPattern.TopLeftDualCcw => new MultiFallingBlockGenerator(new IFallingBlockGenerator[] {
                    new Generator(SpiralStrategy(MatrixSide.Left, true, true).Loop(1), 0, 0.5f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Left, true, true).Loop(1), 2, 0.75f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Left, true, true).Loop(2), 4, 1f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Right, true, true).Loop(1), 0, 0.5f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Right, true, true).Loop(1), 2, 0.75f, 140),
                    new Generator(SpiralStrategy(MatrixSide.Right, true, true).Loop(2), 4, 1f, 140),
                }),
                _ => throw new ArgumentOutOfRangeException(nameof(pattern), pattern, null),
            };
            return generator;
        }
    }
}