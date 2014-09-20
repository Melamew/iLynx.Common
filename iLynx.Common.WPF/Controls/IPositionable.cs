using System;
using System.Windows;

namespace iLynx.Common.WPF.Controls
{
    public interface IPositionable
    {
        Point Position { get; set; }
        event EventHandler PositionChanged;
    }
}