using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLynx.Common.WPF
{
    /// <summary>
    /// IMergeDictionaryService
    /// </summary>
    public interface IMergeDictionaryService
    {
        /// <summary>
        /// Adds the resource.
        /// </summary>
        /// <param name="uri">The URI.</param>
        void AddResource(Uri uri);
    }
}
