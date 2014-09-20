using System;
using System.Windows.Media.Imaging;

namespace iLynx.Common.WPF.Imaging
{
    public interface IBitmapRenderer : IRenderer
    {
        event Action<BitmapSource> SourceCreated;

        /// <summary>
        /// Registers the render callback.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="tieBreaker">if set to <c>true</c> [tie breaker].</param>
        void RegisterRenderCallback(RenderCallback callback, int priority, bool tieBreaker = false);

        /// <summary>
        /// Removes the render callback.
        /// </summary>
        /// <param name="callback">The callback.</param>
        void RemoveRenderCallback(RenderCallback callback);

        /// <summary>
        /// Gets the width of the render.
        /// </summary>
        /// <value>
        /// The width of the render.
        /// </value>
        int RenderWidth { get; }

        /// <summary>
        /// Gets the height of the render.
        /// </summary>
        /// <value>
        /// The height of the render.
        /// </value>
        int RenderHeight { get; }

        /// <summary>
        /// Changes the size of the render.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        void ChangeRenderSize(int width, int height);
    }
}