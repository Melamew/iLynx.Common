using System;

namespace iLynx.Common.WPF.Themes
{
    /// <summary>
    /// Theme
    /// </summary>
    public abstract class Theme
    {
        /// <summary>
        /// Gets the resource location.
        /// </summary>
        /// <returns></returns>
        public abstract string GetResourceLocation();

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public abstract Guid UniqueId { get; }
    }
}