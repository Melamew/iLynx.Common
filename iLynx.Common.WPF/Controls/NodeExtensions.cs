using System;
using System.Collections.Generic;
using System.Windows;

namespace iLynx.Common.WPF.Controls
{
    public static class NodeExtensions
    {
        public static T GetNearestItem<T>(this IEnumerable<T> source, Point point, Func<T, bool> verifyItem = null) where T : IPositionable
        {
            var nearestDelta = new Vector(double.MaxValue, double.MaxValue);
            var nearestNode = default(T);
            foreach (var child in source)
            {
                var delta = child.Position - point;
                if (!(delta.Length < nearestDelta.Length) || (null != verifyItem && !verifyItem(child))) continue;
                nearestNode = child;
                nearestDelta = delta;
            }
            return nearestNode;
        }
    }
}