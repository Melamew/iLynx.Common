using System.Runtime.InteropServices;

namespace iLynx.Common.WPF
{
    /// <summary>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    // ReSharper disable once InconsistentNaming
    public class MONITORINFO
    {
        /// <summary>
        /// </summary>
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

        /// <summary>
        /// </summary>
        public RECT rcMonitor = new RECT();

        /// <summary>
        /// </summary>
        public RECT rcWork = new RECT();

        /// <summary>
        /// </summary>
        public int dwFlags = 0;
    }
}