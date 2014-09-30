using System.Windows;
using System.Windows.Controls;

namespace iLynx.Common.WPF.Behaviours
{
    public class TextBoxSelectionBehaviour
    {
        public static readonly DependencyProperty AutoSelectOnFocusedProperty = DependencyProperty.RegisterAttached(
            "AutoSelectOnFocused", typeof (bool), typeof (TextBoxSelectionBehaviour), new PropertyMetadata(default(bool), OnAutoSelectOnFocusedChanged));

        private static void OnAutoSelectOnFocusedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var textField = dependencyObject as TextBox;
            if (null == textField) return;
            var newValue = (bool)dependencyPropertyChangedEventArgs.NewValue;
            if (newValue)
                textField.GotFocus += TextBoxOnGotFocus;
            else
                textField.PreviewGotKeyboardFocus -= TextBoxOnGotFocus;
        }

        private static void TextFieldOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            var textBox = sender as TextBox;
            if (null == textBox) return;
            RuntimeCommon.DefaultLogger.Log(LogLevel.Information, typeof(TextBoxSelectionBehaviour), string.Format("Text Changed: {0}", textBox.Text));
        }

        private static void TextBoxOnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            RuntimeCommon.DefaultLogger.Log(LogLevel.Information, typeof(TextBoxSelectionBehaviour), string.Format("GotFocus: {0}", routedEventArgs));
            var textField = sender as TextBox;
            if (null == textField) return;
            textField.SelectAll();
        }

        public static void SetAutoSelectOnFocused(DependencyObject element, bool value)
        {
            element.SetValue(AutoSelectOnFocusedProperty, value);
        }

        public static bool GetAutoSelectOnFocused(DependencyObject element)
        {
            return (bool) element.GetValue(AutoSelectOnFocusedProperty);
        }
    }
}
