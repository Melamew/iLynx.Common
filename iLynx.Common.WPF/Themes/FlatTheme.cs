using System;

namespace iLynx.Common.WPF.Themes
{
    /// <summary>
    /// FlatTheme
    /// </summary>
    public class FlatTheme : Theme
    {
        /// <summary>
        /// The id
        /// </summary>
        private static readonly Guid Id = "FlatTheme".CreateGuidV5(RuntimeHelper.LynxSpace);

        /// <summary>
        /// Gets the resource location.
        /// </summary>
        /// <returns></returns>
        public override string GetResourceLocation()
        {
            return "Themes/FlatTheme.xaml";
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override Guid UniqueId
        {
            get { return Id; }
        }
    }
}
