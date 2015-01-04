using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace iLynx.Chatter.Infrastructure
{
    public class RichTextBoxExtensions
    {
        public static readonly DependencyProperty DocumentProperty = DependencyProperty.RegisterAttached(
            "Document", typeof (FlowDocument), typeof (RichTextBoxExtensions), new PropertyMetadata(default(FlowDocument), OnDocumentChanged));

        private static readonly DependencyProperty IsChangingProperty = DependencyProperty.RegisterAttached(
            "IsChanging", typeof (bool), typeof (RichTextBoxExtensions), new PropertyMetadata(default(bool)));

        private static void SetIsChanging(RichTextBox element, bool value)
        {
            element.SetValue(IsChangingProperty, value);
        }

        private static bool GetIsChanging(RichTextBox element)
        {
            return (bool) element.GetValue(IsChangingProperty);
        }

        private static void OnDocumentChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var box = dependencyObject as RichTextBox;
            if (null == box) return;
            if (null == dependencyPropertyChangedEventArgs.NewValue) return;
            box.Document = dependencyPropertyChangedEventArgs.NewValue as FlowDocument;
        }

        public static void SetDocument(RichTextBox element, FlowDocument value)
        {
            element.SetValue(DocumentProperty, value);
        }

        public static FlowDocument GetDocument(RichTextBox element)
        {
            return (FlowDocument) element.GetValue(DocumentProperty);
        }
    }
}