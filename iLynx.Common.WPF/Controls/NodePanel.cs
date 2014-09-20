using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using iLynx.Common.Interfaces;

namespace iLynx.Common.WPF.Controls
{
    [ContentProperty("ItemsSource")]
    [DefaultProperty("ItemsSource")]
    [DefaultEvent("OnItemsChanged")]
    public class NodePanel : Control, IAddChild
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<object>), typeof(NodePanel), new PropertyMetadata(new ObservableCollection<object>(), OnItemsPropertyChanged));

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof (ObservableCollection<object>), typeof (NodePanel), new PropertyMetadata(default(ObservableCollection<object>)));

        public static readonly DependencyProperty NodeHeaderTemplateProperty =
            DependencyProperty.Register("NodeHeaderTemplate", typeof (DataTemplate), typeof (NodePanel), new PropertyMetadata(default(DataTemplate)));

        public static readonly DependencyProperty NodeTemplateProperty =
            DependencyProperty.Register("NodeTemplate", typeof (DataTemplate), typeof (NodePanel), new PropertyMetadata(default(DataTemplate)));

        public static readonly DependencyPropertyKey ResultPropertyKey =
            DependencyProperty.RegisterReadOnly("Results", typeof (object[]), typeof (NodePanel), new PropertyMetadata(default(object[])));


        public ObservableCollection<object> Items
        {
            get { return (ObservableCollection<object>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public object[] Results
        {
            get { return (object[]) GetValue(ResultPropertyKey.DependencyProperty); }
            private set { SetValue(ResultPropertyKey, value); }
        }

        public DataTemplate NodeTemplate
        {
            get { return (DataTemplate) GetValue(NodeTemplateProperty); }
            set { SetValue(NodeTemplateProperty, value); }
        }

        public DataTemplate NodeHeaderTemplate
        {
            get { return (DataTemplate) GetValue(NodeHeaderTemplateProperty); }
            set { SetValue(NodeHeaderTemplateProperty, value); }
        }

        private static void OnItemsPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var source = dependencyObject as NodePanel;
            if (null == source) return;
            var oldItems = dependencyPropertyChangedEventArgs.OldValue as INotifyCollectionChanged;
            if (null != oldItems)
                oldItems.CollectionChanged -= source.OnItemsCollectionChanged;
            var newItems = dependencyPropertyChangedEventArgs.NewValue as INotifyCollectionChanged;
            if (null == newItems) return;
            newItems.CollectionChanged += source.OnItemsCollectionChanged;
        }

        public void RemoveNode(Node node)
        {
            Items.Remove(node);
        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnOnItemsChanged();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    RemoveItems(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Replace:
                    RemoveItems(e.OldItems);
                    AddItems(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Add:
                    AddItems(e.NewItems);
                    break;
            }
        }

        private void RemoveItems(IEnumerable items)
        {
            foreach (var item in items)
                RemoveNode(item);
        }

        private void AddItems(IEnumerable items)
        {
            foreach (var item in items)
                AddNode(item);
        }

        private void AddNode(object forItem)
        {
            var node = new Node { DataContext = forItem, Content = forItem, ContentTemplate = NodeTemplate, HeaderTemplate = NodeHeaderTemplate };
            Items.Add(node);
            var calculator = forItem as IRequestRecalculate;
            if (null == calculator) return;
            calculator.RequestRecalculate += CalculatorOnRequestRecalculate;
        }

        private IEnumerable<Node> FindOutputEndPoints()
        {
            return Items.OfType<Node>().Where(x => !x.HasConnectedOutputs());
        }

        private void CalculatorOnRequestRecalculate(object sender, EventArgs eventArgs)
        {
            Results = FindOutputEndPoints().Select(x => x.RunCalculation()).ToArray();
        }

        private void RemoveNode(object forItem)
        {
            var node = GetNodeForChild(forItem);
            if (null == node) return;
            Items.Remove(node);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            var position = e.GetPosition(this);
            if (null != InputHitTest(position)) return;
            if (e.ChangedButton != MouseButton.Right) return;
            if (null == ContextMenu) return;
            ContextMenu.DataContext = DataContext;
            ContextMenu.IsOpen = true;
        }

        protected virtual void OnOnItemsChanged()
        {
            var handler = OnItemsChanged;
            if (null == handler) return;
            handler(this, EventArgs.Empty);
        }

        public event EventHandler OnItemsChanged;

        public ObservableCollection<object> ItemsSource
        {
            get { return (ObservableCollection<object>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public void AddConnector(Connector connector)
        {
            Items.Insert(0, connector);
        }

        public void RemoveConnector(Connector connector)
        {
            Items.Remove(connector);
        }

        public NodePanel()
        {
            ItemsSource = new ObservableCollection<object>();
            Items = new ObservableCollection<object>();
            DefaultStyleKey = typeof(NodePanel);
        }

        public Node GetNearestNode(Point point)
        {
            return Items.OfType<Node>().GetNearestItem(point);
        }

        private Node GetNodeForChild(object value)
        {
            return Items.OfType<Node>().FirstOrDefault(x => value == x.DataContext);
        }
        
        #region Implementation of IAddChild

        /// <summary>
        /// Adds a child object. 
        /// </summary>
        /// <param name="value">The child object to add.</param>
        public void AddChild(object value)
        {
            if (null == ItemsSource) return;
            ItemsSource.Add(value);
        }

        /// <summary>
        /// Adds the text content of a node to the object. 
        /// </summary>
        /// <param name="text">The text to add to the object.</param>
        public void AddText(string text)
        {
            AddChild(text);
        }

        #endregion
    }
}
