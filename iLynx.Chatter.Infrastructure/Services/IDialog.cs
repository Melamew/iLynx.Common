using System;

namespace iLynx.Chatter.Infrastructure.Services
{
    public interface IDialog
    {
        /// <summary>
        /// Gets the title of this dialog
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets a value that indicates the input result of this dialog.
        /// </summary>
        bool Result { get; }

        double Width { get; }
        
        double Height { get; }

        event EventHandler ResultReceived;
    }
}