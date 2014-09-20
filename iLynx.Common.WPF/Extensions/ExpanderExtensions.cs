using System.Windows;
using System.Windows.Controls;

namespace iLynx.Common.WPF.Extensions
{
    public class ExpanderExtensions
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.RegisterAttached(
            "Orientation", typeof (Orientation), typeof (ExpanderExtensions), new PropertyMetadata(Orientation.Horizontal));

        public static readonly DependencyProperty ToggleButtonVisibilityProperty = DependencyProperty.RegisterAttached(
            "ToggleButtonVisibility", typeof (Visibility), typeof (ExpanderExtensions), new PropertyMetadata(Visibility.Visible));

        public static void SetToggleButtonVisibility(DependencyObject element, Visibility value)
        {
            element.SetValue(ToggleButtonVisibilityProperty, value);
        }

        public static Visibility GetToggleButtonVisibility(DependencyObject element)
        {
            return (Visibility) element.GetValue(ToggleButtonVisibilityProperty);
        }

        public static void SetOrientation(DependencyObject element, Orientation value)
        {
            element.SetValue(OrientationProperty, value);
        }

        public static Orientation GetOrientation(DependencyObject element)
        {
            return (Orientation) element.GetValue(OrientationProperty);
        }
    }
}
