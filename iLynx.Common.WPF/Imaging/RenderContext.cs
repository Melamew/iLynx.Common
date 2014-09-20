using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLynx.Common.WPF.Imaging
{
    public class RenderContext
    {
        public IntPtr BackBuffer { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        public int Stride { get; private set; }

        public RenderContext(IntPtr backBuffer,
                             int width,
                             int height,
                             int stride)
        {
            BackBuffer = backBuffer;
            Width = width;
            Height = height;
            Stride = stride;
        }
    }
}
