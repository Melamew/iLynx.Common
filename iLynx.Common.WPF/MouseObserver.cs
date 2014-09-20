using System.Windows;
using System.Windows.Input;

namespace iLynx.Common.WPF
{
    public class MouseObserver
    {
        public static readonly DependencyProperty ObserveProperty =
            DependencyProperty.RegisterAttached("Observe", typeof (bool?), typeof (MouseObserver), new PropertyMetadata(default(bool?), OnObserveChanged));

        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.RegisterAttached("MouseDownCommand", typeof (ICommand), typeof (MouseObserver), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty MouseUpCommandProperty =
            DependencyProperty.RegisterAttached("MouseUpCommand", typeof (ICommand), typeof (MouseObserver), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.RegisterAttached("MouseMoveCommand", typeof (ICommand), typeof (MouseObserver), new PropertyMetadata(default(ICommand)));

        private static readonly DependencyProperty PreviousCaptureProperty =
            DependencyProperty.RegisterAttached("PreviousCapture", typeof(FrameworkElement), typeof(MouseObserver), new PropertyMetadata(default(FrameworkElement)));

        public static void SetPreviousCapture(FrameworkElement element, object value)
        {
            element.SetValue(PreviousCaptureProperty, value);
        }

        public static FrameworkElement GetPreviousCapture(FrameworkElement element)
        {
            return (FrameworkElement)element.GetValue(PreviousCaptureProperty);
        }

        public static void SetMouseMoveCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseMoveCommandProperty, value);
        }

        public static ICommand GetMouseMoveCommand(UIElement element)
        {
            return (ICommand) element.GetValue(MouseMoveCommandProperty);
        }

        public static void SetMouseUpCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseUpCommandProperty, value);
        }

        public static ICommand GetMouseUpCommand(UIElement element)
        {
            return (ICommand) element.GetValue(MouseUpCommandProperty);
        }

        public static void SetMouseDownCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseDownCommandProperty, value);
        }

        public static ICommand GetMouseDownCommand(UIElement element)
        {
            return (ICommand) element.GetValue(MouseDownCommandProperty);
        }

        private static void OnObserveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var uiElement = dependencyObject as UIElement;
            if (null == uiElement) return;
            var newValue = dependencyPropertyChangedEventArgs.NewValue as bool?;
            if (null == newValue || false == newValue)
                Unsubscribe(uiElement);
            else if (true == newValue)
                Subscribe(uiElement);
        }

        private static void Subscribe(UIElement element)
        {
            element.PreviewMouseDown += ElementOnMouseDown;
            element.PreviewMouseUp += ElementOnMouseUp;
            element.PreviewMouseMove += ElementOnPreviewMouseMove;
        }

        private static void ElementOnPreviewMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            var element = sender as UIElement;
            if (null == element) return;
            var command = GetMouseMoveCommand(element);
            if (null == command) return;
            command.Execute(mouseEventArgs);
        }

        private static void ElementOnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var element = sender as FrameworkElement;
            if (null == element) return;
            var command = GetMouseUpCommand(element);
            if (null == command) return;
            if (Equals(element, Mouse.Captured))
                Mouse.Capture(GetPreviousCapture(element));
            command.Execute(mouseButtonEventArgs);
        }

        private static void ElementOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var element = sender as FrameworkElement;
            if (null == element) return;
            var command = GetMouseDownCommand(element);
            if (null == command) return;
            SetPreviousCapture(element, Mouse.Captured);
            Mouse.Capture(element);
            command.Execute(mouseButtonEventArgs);
        }

        private static void Unsubscribe(UIElement element)
        {
            element.PreviewMouseDown -= ElementOnMouseDown;
            element.PreviewMouseUp -= ElementOnMouseUp;
            element.PreviewMouseMove -= ElementOnPreviewMouseMove;
        }

        public static void SetObserve(UIElement element, bool? value)
        {
            element.SetValue(ObserveProperty, value);
        }

        public static bool? GetObserve(UIElement element)
        {
            return (bool?) element.GetValue(ObserveProperty);
        }
    }
}
