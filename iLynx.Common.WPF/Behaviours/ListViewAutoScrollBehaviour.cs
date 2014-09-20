using System.Windows;
using System.Windows.Controls;

namespace iLynx.Common.WPF.Behaviours
{
    public class ListViewAutoScrollBehaviour
    {
        public static readonly DependencyProperty ScrollSelectionInToViewProperty = DependencyProperty.RegisterAttached(
            "ScrollSelectionInToView", typeof (bool), typeof (ListViewAutoScrollBehaviour), new PropertyMetadata(default(bool), OnScrollSelectionInToViewPropertyChanged));

        private static void OnScrollSelectionInToViewPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var listView = dependencyObject as ListView;
            if (null == listView) return;
            var newValue = (bool) dependencyPropertyChangedEventArgs.NewValue;
            if (newValue)
                listView.SelectionChanged += ListViewOnSelectionChanged;
            else
                listView.SelectionChanged -= ListViewOnSelectionChanged;
        }

        private static void ListViewOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var listView = sender as ListView;
            if (null == listView) return;
            listView.ScrollIntoView(listView.SelectedItem);
        }

        public static void SetScrollSelectionInToView(DependencyObject element, bool value)
        {
            element.SetValue(ScrollSelectionInToViewProperty, value);
        }

        public static bool GetScrollSelectionInToView(DependencyObject element)
        {
            return (bool) element.GetValue(ScrollSelectionInToViewProperty);
        }
    }
}
