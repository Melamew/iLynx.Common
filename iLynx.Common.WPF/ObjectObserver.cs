using System.Windows;
using System.Windows.Input;

namespace iLynx.Common.WPF
{
    public class ObjectObserver
    {
        public static readonly DependencyProperty ObjectLoadedCommandProperty =
            DependencyProperty.RegisterAttached("ObjectLoadedCommand", typeof (ICommand), typeof (ObjectObserver), new PropertyMetadata(default(ICommand), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var element = dependencyObject as FrameworkElement;
            if (null == element) return;
            if (null == dependencyPropertyChangedEventArgs.NewValue)
                element.Loaded -= ElementOnLoaded;
            else
                element.Loaded += ElementOnLoaded;
        }

        private static void ElementOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var element = sender as FrameworkElement;
            if (null == element) return;
            var command = GetObjectLoadedCommand(element);
            if (null == command) return;
            command.Execute(element);
        }

        public static void SetObjectLoadedCommand(FrameworkElement element, ICommand value)
        {
            element.SetValue(ObjectLoadedCommandProperty, value);
        }

        public static ICommand GetObjectLoadedCommand(FrameworkElement element)
        {
            return (ICommand) element.GetValue(ObjectLoadedCommandProperty);
        }
    }
}
