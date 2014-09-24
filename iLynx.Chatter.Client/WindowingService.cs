using System;
using iLynx.Chatter.Infrastructure.Services;

namespace iLynx.Chatter.Client
{
    public class WindowingService : IWindowingService
    {
        public int Show(object content, string title)
        {
            throw new NotImplementedException();
        }

        public void Minimize(int window)
        {
            throw new NotImplementedException();
        }

        public void Close(int window)
        {
            throw new NotImplementedException();
        }

        public void Maximize(int window)
        {
            throw new NotImplementedException();
        }

        public int FindIdByContent(object content)
        {
            throw new NotImplementedException();
        }

        public bool ShowDialog(IDialog content)
        {
            throw new NotImplementedException();
        }
    }
}
