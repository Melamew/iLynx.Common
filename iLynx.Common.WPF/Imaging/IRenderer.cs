namespace iLynx.Common.WPF.Imaging
{
    public interface IRenderer
    {
        /// <summary>
        /// Gets or sets a value indicating whether [clear each pass].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [clear each pass]; otherwise, <c>false</c>.
        /// </value>
        bool ClearEachPass { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        bool IsRunning { get; }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();
    }
}