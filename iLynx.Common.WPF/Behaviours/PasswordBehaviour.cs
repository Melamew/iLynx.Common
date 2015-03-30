using System;
using System.Windows;
using System.Windows.Controls;

namespace iLynx.Common.WPF.Behaviours
{
    public class PasswordBehaviour
    {
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.RegisterAttached(
            "Password", typeof (string), typeof (PasswordBehaviour), new PropertyMetadata(default(string), OnPasswordChanged));

        private static void OnPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as PasswordBox;
            if (null == obj) return;
            obj.Password = dependencyPropertyChangedEventArgs.NewValue as string;
        }

        public static readonly DependencyProperty AttachProperty = DependencyProperty.RegisterAttached(
            "Attach", typeof (bool), typeof (PasswordBehaviour), new PropertyMetadata(default(bool), OnAttachChanged));

        private static void OnAttachChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var passwordBox = dependencyObject as PasswordBox;
            if (null == passwordBox) return;
            var attach = (bool) dependencyPropertyChangedEventArgs.NewValue;
            if (!attach)
                passwordBox.PasswordChanged -= PasswordBoxOnPasswordChanged;
            else
            {
                passwordBox.PasswordChanged += PasswordBoxOnPasswordChanged;
                passwordBox.Password = GetPassword(passwordBox);
                
            }
        }

        private static void PasswordBoxOnPasswordChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            var passwordBox = sender as PasswordBox;
            if (null == passwordBox) return;
            SetPassword(passwordBox, passwordBox.Password);
        }

        public static void SetAttach(DependencyObject element, bool value)
        {
            element.SetValue(AttachProperty, value);
        }

        public static bool GetAttach(DependencyObject element)
        {
            return (bool) element.GetValue(AttachProperty);
        }
        
        public static void SetPassword(DependencyObject element, string value)
        {
            element.SetValue(PasswordProperty, value);
        }

        public static string GetPassword(DependencyObject element)
        {
            return (string) element.GetValue(PasswordProperty);
        }
    }
}
