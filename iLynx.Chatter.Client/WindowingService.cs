using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Windows;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common.WPF;

namespace iLynx.Chatter.Client
{
    public class WindowingService : IWindowingService
    {
        private readonly Dictionary<int, BorderlessWindow> openWindows = new Dictionary<int, BorderlessWindow>();
        private int nextId = int.MinValue;

        public int Show(object content, string title)
        {
            var window = new BorderlessWindow
            {
                Content = content,
                Title = title,
            };
            var id = NextId;
            openWindows.Add(id, window);
            window.Show();
            return id;
        }

        private int NextId
        {
            get { return nextId++; }
        }

        public void Minimize(int id)
        {
            BorderlessWindow window;
            if (!openWindows.TryGetValue(id, out window)) return;
            window.WindowState = WindowState.Minimized;
        }

        public void Close(int id)
        {
            BorderlessWindow window;
            if (!openWindows.TryGetValue(id, out window)) return;
            window.Close();
        }

        public void Maximize(int id)
        {
            BorderlessWindow window;
            if (!openWindows.TryGetValue(id, out window)) return;
            window.WindowState = WindowState.Maximized;
        }

        public int FindIdByContent(object content)
        {
            var kvp = openWindows.FirstOrDefault(x => ReferenceEquals(content, x.Value.Content));
            if (null == kvp.Value) throw new InstanceNotFoundException();
            return kvp.Key;
        }

        public bool ShowDialog(IDialog content)
        {
            var window = new BorderlessWindow
            {
                Content = content,
                Title = content.Title
            };
            window.ShowDialog();
            return content.Result;
        }
    }
}
