using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using iLynx.Common.Threading;

namespace iLynx.Common.WPF.Imaging
{
    /// <summary>
    /// WriteableBitmapRenderer
    /// </summary>
    public class UnmanagedBitmapRenderer : RendererBase, IBitmapRenderer
    {
        private readonly IDispatcher dispatcher;

        private readonly SortedList<int, RenderCallback> renderCallbacks = new SortedList<int, RenderCallback>();
        private int renderWidth;
        private int renderHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnmanagedBitmapRenderer" /> class.
        /// </summary>
        /// <param name="threadManager">The thread manager.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        public UnmanagedBitmapRenderer(IThreadManager threadManager, IDispatcher dispatcher)
            : base(threadManager)
        {
            threadManager.Guard("threadManager");
            dispatcher.Guard("dispatcher");
            this.dispatcher = dispatcher;
        }

        private readonly object syncRoot = new object();
        private RenderSetup setup;

        private class RenderSetup
        {
            public WriteableBitmap Bitmap;
            public Int32Rect Dirty;
            public RenderContext Context;
        }

        #region Overrides of RendererBase

        /// <summary>
        /// Renders the loop.
        /// </summary>
        protected override void RenderLoop()
        {
            while (Render)
            {
                Thread.CurrentThread.Join(RenderInterval);
                if (!Render) return;
                var currentSetup = setup;
                if (null == currentSetup) continue;
                Lock(currentSetup);
                if (ClearEachPass)
                    NativeMethods.MemSet(currentSetup.Context.BackBuffer, 0x00, currentSetup.Context.Height * currentSetup.Context.Stride);
                var renderContext = currentSetup.Context;
                lock (renderCallbacks)
                {
                    var cnt = renderCallbacks.Count;
                    while (cnt-- > 0)
                        renderCallbacks.Values[cnt](renderContext);
                }
                Unlock(currentSetup);
            }
        }

        private void Lock(RenderSetup currentSetup)
        {
            dispatcher.Invoke(DispatcherPriority.Normal, () => currentSetup.Bitmap.Lock());
        }

        private void Unlock(RenderSetup currentSetup)
        {
            dispatcher.Invoke(DispatcherPriority.Normal, () =>
                                                         {
                                                             currentSetup.Bitmap.AddDirtyRect(currentSetup.Dirty);
                                                             currentSetup.Bitmap.Unlock();
                                                         });
        }

        /// <summary>
        /// Gets the width of the render.
        /// </summary>
        /// <value>
        /// The width of the render.
        /// </value>
        public override int RenderWidth
        {
            get { return renderWidth; }
        }

        /// <summary>
        /// Gets the height of the render.
        /// </summary>
        /// <value>
        /// The height of the render.
        /// </value>
        public override int RenderHeight
        {
            get { return renderHeight; }
        }

        /// <summary>
        /// Changes the size of the render.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public override void ChangeRenderSize(int width, int height)
        {
            if (!dispatcher.CheckAccess())
                dispatcher.Invoke(DispatcherPriority.Background, () => ChangeRenderSize(width, height));
            else
            {
                if (0 >= width || 0 >= height) return;
                renderWidth = width;
                renderHeight = height;
                var bitmap = new WriteableBitmap(renderWidth, renderHeight, 96d, 96d, PixelFormats.Pbgra32, null);
                var newSetup = new RenderSetup
                               {
                                   Bitmap = bitmap,
                                   Context = new RenderContext(bitmap.BackBuffer, renderWidth, renderHeight, bitmap.BackBufferStride),
                                   Dirty = new Int32Rect(0, 0, width, height)
                               };
                OnSourceCreated(bitmap);
                setup = newSetup;
            }
        }

        protected virtual void OnSourceCreated(BitmapSource source)
        {
            var handler = SourceCreated;
            if (null == handler) return;
            handler(source);
        }

        #endregion

        #region Implementation of IBitmapRenderer

        public event Action<BitmapSource> SourceCreated;

        /// <summary>
        /// Registers the render callback.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="tieBreaker">if set to <c>true</c> [tie breaker].</param>
        public void RegisterRenderCallback(RenderCallback callback, int priority, bool tieBreaker = false)
        {
            Monitor.Enter(syncRoot);
            var prio = priority * 100;
            AdjustPriority(ref prio, tieBreaker);
            renderCallbacks.Add(prio, callback);
            Monitor.Exit(syncRoot);
        }

        /// <summary>
        /// Adjusts the priority.
        /// </summary>
        /// <param name="priority">The priority.</param>
        /// <param name="breaker">if set to <c>true</c> [breaker].</param>
        protected virtual void AdjustPriority(ref int priority, bool breaker)
        {
            while (renderCallbacks.ContainsKey(priority))
                priority += (breaker ? 1 : -1);
            // Invert priority to allow the renderthread to go through the callbacks in reverse order
            priority *= -1;
        }

        /// <summary>
        /// Removes the render callback.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public void RemoveRenderCallback(RenderCallback callback)
        {
            Monitor.Enter(syncRoot);
            try
            {
                var exists = renderCallbacks.Values.Contains(callback);
                if (!exists) return;
                var kvp = renderCallbacks.FirstOrDefault(k => k.Value == callback);
                renderCallbacks.Remove(kvp.Key);
            }
            finally { Monitor.Exit(syncRoot); }
        }

        #endregion
    }
}
