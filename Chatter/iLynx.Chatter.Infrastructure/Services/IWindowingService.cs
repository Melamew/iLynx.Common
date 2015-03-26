namespace iLynx.Chatter.Infrastructure.Services
{
    public interface IWindowingService
    {
        /// <summary>
        /// Opens a new window with the specified content and title and returns the Id of the newly opened window (This can be used to control the window).
        /// </summary>
        /// <param name="content"></param>
        /// <param name="title"></param>
        int Show(object content, string title);

        /// <summary>
        /// Minimizes the window with the specified id
        /// </summary>
        /// <param name="window"></param>
        void Minimize(int window);

        /// <summary>
        /// Closes the window with the specified id
        /// </summary>
        /// <param name="window"></param>
        void Close(int window);

        /// <summary>
        /// Maximizes the window with the specified id
        /// </summary>
        /// <param name="window"></param>
        void Maximize(int window);

        /// <summary>
        /// Finds the id of the window that contains the specified content.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        int FindIdByContent(object content);

        /// <summary>
        /// Shows a window as a dialog and returns the result of the dialog as a boolean.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        bool ShowDialog(IDialog content);
    }
}
