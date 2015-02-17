using System.Windows;
using System.Windows.Controls;

namespace iLynx.Common.WPF.Controls
{
    public class LoadScreen : ContentControl
    {
        static LoadScreen()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadScreen), new FrameworkPropertyMetadata(typeof(LoadScreen)));
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
            "IsLoading", typeof(bool), typeof(LoadScreen), new PropertyMetadata(default(bool)));
        
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof(string), typeof(LoadScreen), new PropertyMetadata(default(string)));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
    }
}
