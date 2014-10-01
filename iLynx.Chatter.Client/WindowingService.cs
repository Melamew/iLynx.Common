using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Windows;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;
using iLynx.Common.WPF;

namespace iLynx.Chatter.Client
{
    public class WindowingService : IWindowingService
    {
        private readonly IDispatcher dispatcher;
        private readonly Dictionary<int, BorderlessWindow> openWindows = new Dictionary<int, BorderlessWindow>();
        private int nextId = int.MinValue;

        public WindowingService(IDispatcher dispatcher)
        {
            this.dispatcher = Guard.IsNull(() => dispatcher);
        }

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
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(Minimize, id);
                return;
            }
            BorderlessWindow window;
            if (!openWindows.TryGetValue(id, out window)) return;
            window.WindowState = WindowState.Minimized;
        }

        public void Close(int id)
        {
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(Close, id);
                return;
            }
            BorderlessWindow window;
            if (!openWindows.TryGetValue(id, out window)) return;
            window.Close();
        }

        public void Maximize(int id)
        {
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(Maximize, id);
                return;
            }
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
            if (!dispatcher.CheckAccess())
                return dispatcher.Invoke(() => ShowDialog(content));
            var window = new BorderlessWindow
            {
                Content = content,
                Title = content.Title,
                Width = content.Width,
                Height = content.Height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            var result = false;
            content.ResultReceived += (sender, args) =>
            {
                result = content.Result;
                window.Close();
            };
            window.ShowDialog();
            return result;
        }
    }
}
