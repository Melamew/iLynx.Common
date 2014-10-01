using System.Windows;
using System.Windows.Input;

namespace iLynx.Common.WPF.Behaviours
{
    public class ElementSubmitBehaviour
    {
        public static readonly DependencyProperty AttachProperty = DependencyProperty.RegisterAttached(
            "Attach", typeof (bool), typeof (ElementSubmitBehaviour), new PropertyMetadata(default(bool), OnAttachChanged));
        public static readonly DependencyProperty SubmitCommandProperty = DependencyProperty.RegisterAttached(
            "SubmitCommand", typeof(ICommand), typeof(ElementSubmitBehaviour), new PropertyMetadata(default(ICommand)));

        private static void OnAttachChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var element = dependencyObject as FrameworkElement;
            if (null == element) return;
            var value = (bool)dependencyPropertyChangedEventArgs.NewValue;
            if (!value)
                element.PreviewKeyDown -= ElementOnPreviewKeyDown;
            else
                element.PreviewKeyDown += ElementOnPreviewKeyDown;
        }

        private static void ElementOnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (Key.Enter != keyEventArgs.Key) return;
            var dependencyObject = sender as DependencyObject;
            if (null == dependencyObject) return;
            var command = GetSubmitCommand(dependencyObject);
            if (null == command) return;
            command.Execute(null);
        }

        public static void SetAttach(DependencyObject element, bool value)
        {
            element.SetValue(AttachProperty, value);
        }

        public static bool GetAttach(DependencyObject element)
        {
            return (bool) element.GetValue(AttachProperty);
        }

        public static void SetSubmitCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(SubmitCommandProperty, value);
        }

        public static ICommand GetSubmitCommand(DependencyObject element)
        {
            return (ICommand) element.GetValue(SubmitCommandProperty);
        }
    }
}
