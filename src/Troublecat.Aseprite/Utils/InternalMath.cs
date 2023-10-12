namespace Troublecat.Aseprite.Utils {
    internal static class InternalMath {
        /// <summary>
        /// Determines the minimum of the parameters.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        internal static float Min(float a, float b) => a < b ? a : b;

        /// <summary>
        /// Determines the maximum of the parameters.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        internal static float Max(float a, float b) => a > b ? a : b;
    }
}
