using System;

namespace App {
    public static class MathUtils {
        public static bool Approximately(double a, double b) =>
            Math.Abs(b - a) <
            Math.Max(
                1E-12f * Math.Max(Math.Abs(a), Math.Abs(b)),
                double.Epsilon * 8f);

        public static bool MultiplyNotOverflow(int a, int b, out int result) {
            result = 0;
            try {
                checked {
                    result = a * b;
                    return true;
                }
            } catch (OverflowException e) {
                result = 0;
                return false;
            }
        }
    }
}