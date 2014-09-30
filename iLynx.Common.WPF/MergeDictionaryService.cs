using System;
using System.Windows;

namespace iLynx.Common.WPF
{
    /// <summary>
    /// MergeDictionaryService
    /// </summary>
    public class MergeDictionaryService : IMergeDictionaryService
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeDictionaryService" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public MergeDictionaryService(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Adds the resource.
        /// </summary>
        /// <param name="uri">The URI.</param>
        public void AddResource(Uri uri)
        {
            logger.Log(LogLevel.Information, this, string.Format("Attempting to add resource: {0}", uri));
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
        }

        /// <summary>
        /// Adds the resource.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void AddResource(string filename)
        {
            AddResource(new Uri(filename, UriKind.RelativeOrAbsolute));
        }
    }
}
