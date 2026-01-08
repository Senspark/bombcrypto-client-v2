namespace BLPvpMode.Engine.Strategy.Matrix {
    public class ShrinkLeftMatrixStrategy : IMatrixStrategy {
        public IMatrixState Process(IMatrixState state) {
            return new MatrixState(
                left: state.Left + 1,
                right: state.Right,
                top: state.Top,
                bottom: state.Bottom,
                positions: state.Positions
            );
        }
    }
}