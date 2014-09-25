using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace iLynx.Chatter.Infrastructure.Services
{
    public interface IMenuItem
    {
        string Title { get; }
        Action Callback { get; }
        IEnumerable<IMenuItem> Children { get; }
        void AddChild(IMenuItem item);
        void RemoveChild(IMenuItem item);
    }

    public interface IMainMenuService
    {
        void RegisterMenuItem(IMenuItem item);
        void RemoveMenuItem(IMenuItem item);
        ObservableCollection<IMenuItem> MenuItems { get; } 
    }
}
