namespace iLynx.Common.WPF.Imaging
{
    /// <summary>
    /// RenderProxy
    /// </summary>
    public class RenderProxy : IRenderProxy
    {
        public int Height { get; private set; }
        public int Width { get; private set; }
        public int BackBufferStride { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderProxy" /> class.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <param name="backBufferStride">The back buffer stride.</param>
        public RenderProxy(int height, int width, int backBufferStride)
        {
            Height = height;
            Width = width;
            BackBufferStride = backBufferStride;
        }
    }
}