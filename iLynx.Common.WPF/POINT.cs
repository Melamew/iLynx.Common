using System.Runtime.InteropServices;

namespace iLynx.Common.WPF
{
    /// <summary>
    ///     POINT aka POINTAPI
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
// ReSharper disable once InconsistentNaming
    public struct POINT
    {
        /// <summary>
        ///     x coordinate of point.
        /// </summary>
        public int x;

        /// <summary>
        ///     y coordinate of point.
        /// </summary>
        public int y;

        /// <summary>
        ///     Construct a point of coordinates (x,y).
        /// </summary>
        public POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}