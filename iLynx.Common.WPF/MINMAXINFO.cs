using System.Runtime.InteropServices;

namespace iLynx.Common.WPF
{
    [StructLayout(LayoutKind.Sequential)]
// ReSharper disable once InconsistentNaming
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    };
}