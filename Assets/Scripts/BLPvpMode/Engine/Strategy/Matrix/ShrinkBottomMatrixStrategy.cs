namespace BLPvpMode.Engine.Strategy.Matrix {
    public class ShrinkBottomMatrixStrategy : IMatrixStrategy {
        public IMatrixState Process(IMatrixState state) {
            return new MatrixState(
                left: state.Left,
                right: state.Right,
                top: state.Top,
                bottom: state.Bottom - 1,
                positions: state.Positions
            );
        }
    }
}