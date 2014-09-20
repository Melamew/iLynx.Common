namespace iLynx.Common.WPF.Imaging
{
    /// <summary>
    /// IRenderProxy
    /// </summary>
    public interface IRenderProxy
    {
        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        int Height { get; }
        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        int Width { get; }
        /// <summary>
        /// Gets the back buffer stride.
        /// </summary>
        /// <value>
        /// The back buffer stride.
        /// </value>
        int BackBufferStride { get; }
    }
}