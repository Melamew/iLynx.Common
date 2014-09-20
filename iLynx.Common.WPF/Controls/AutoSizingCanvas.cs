using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace iLynx.Common.WPF.Controls
{
    public class AutoSizingCanvas : Canvas
    {
        //public AutoSizingCanvas()
        //{
        //    var descriptor = DependencyPropertyDescriptor.FromProperty(LeftProperty, typeof(Canvas));
        //    descriptor.AddValueChanged(this, OnCanvasLeftChanged);
        //    descriptor = DependencyPropertyDescriptor.FromProperty(TopProperty, typeof(Canvas));
        //    descriptor.AddValueChanged(this, OnCanvasLeftChanged);
        //}

        //private void OnCanvasLeftChanged(object sender, EventArgs e)
        //{
        //    InvalidateMeasure();
        //}

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            var node = visualAdded as Node;
            if (null != node)
            {
                node.PositionChanged += NodeOnPositionChanged;
                node.SizeChanged += NodeOnSizeChanged;
            }
            node = visualRemoved as Node;
            if (null != node)
            {
                node.PositionChanged -= NodeOnPositionChanged;
                node.SizeChanged -= NodeOnSizeChanged;
            }
            InvalidateMeasure();
        }

        private void NodeOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            InvalidateMeasure();
        }

        private void NodeOnPositionChanged(object sender, EventArgs eventArgs)
        {
            InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            double maxX = double.MinValue, maxY = double.MinValue;
            foreach (var element in Children.OfType<FrameworkElement>())
            {
                element.Measure(constraint);
                var x = GetLeft(element) + element.DesiredSize.Width;
                var y = GetTop(element) + element.DesiredSize.Height;
                if (x > maxX)
                    maxX = x;
                if (y > maxY)
                    maxY = y;
            }
            if (maxX < 0d)
                maxX = 0d;
            if (maxY < 0d)
                maxY = 0d;
            return new Size(maxX, maxY);
            //return base.MeasureOverride(constraint);
        }
    }
}
