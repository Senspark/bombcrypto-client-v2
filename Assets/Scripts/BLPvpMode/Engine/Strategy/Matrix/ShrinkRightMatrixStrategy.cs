namespace BLPvpMode.Engine.Strategy.Matrix {
    public class ShrinkRightMatrixStrategy : IMatrixStrategy {
        public IMatrixState Process(IMatrixState state) {
            return new MatrixState(
                left: state.Left,
                right: state.Right - 1,
                top: state.Top,
                bottom: state.Bottom,
                positions: state.Positions
            );
        }
    }
}