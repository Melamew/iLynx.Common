using System.Windows;
using System.Windows.Controls;

namespace iLynx.Common.WPF.Behaviours
{
    public class TextBoxAutoScrollBehaviour
    {
        public static readonly DependencyProperty AutoScrollProperty = DependencyProperty.RegisterAttached(
            "AutoScroll", typeof (bool), typeof (TextBoxAutoScrollBehaviour), new PropertyMetadata(default(bool), OnAutoScrollChanged));

        private static void OnAutoScrollChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var tb = dependencyObject as TextBox;
            if (null == tb) return;
            var value = (bool)dependencyPropertyChangedEventArgs.NewValue;
            if (value)
                tb.TextChanged += TextChanged;
            else
                tb.TextChanged -= TextChanged;
        }

        private static void TextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            var textBox = sender as TextBox;
            if (null == textBox) return;
            textBox.ScrollToEnd();
        }

        public static void SetAutoScroll(DependencyObject element, bool value)
        {
            element.SetValue(AutoScrollProperty, value);
        }

        public static bool GetAutoScroll(DependencyObject element)
        {
            return (bool) element.GetValue(AutoScrollProperty);
        }
    }
}
