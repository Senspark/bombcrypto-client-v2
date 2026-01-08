namespace BLPvpMode.Engine.Strategy.Matrix {
    public class ShrinkTopMatrixStrategy : IMatrixStrategy {
        public IMatrixState Process(IMatrixState state) {
            return new MatrixState(
                left: state.Left,
                right: state.Right,
                top: state.Top + 1,
                bottom: state.Bottom,
                positions: state.Positions
            );
        }
    }
}