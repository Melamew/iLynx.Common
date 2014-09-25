using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;

namespace iLynx.Chatter.ClientServicesModule.ViewModels
{
    public class MenuItemViewModel : NotificationBase, IMenuItem
    {
        private readonly ObservableCollection<IMenuItem> children;
        public MenuItemViewModel(string title, Action callback, params IMenuItem[] children)
        {
            Title = Guard.IsNull(() => title);
            Callback = callback;
            this.children = new ObservableCollection<IMenuItem>(children);
        }

        public string Title { get; private set; }
        public Action Callback { get; private set; }
        public IEnumerable<IMenuItem> Children { get { return children; } }
        public void AddChild(IMenuItem item)
        {
            children.Add(item);
        }

        public void RemoveChild(IMenuItem item)
        {
            children.Remove(item);
        }
    }
}
