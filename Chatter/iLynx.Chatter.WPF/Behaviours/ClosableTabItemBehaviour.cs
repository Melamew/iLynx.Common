//using System;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Input;
//using iLynx.Common.WPF;

//namespace iLynx.Chatter.WPF.Behaviours
//{
//    public class ClosableTabItemBehaviour : DependencyObject
//    {
//        public static readonly DependencyPropertyKey CloseTabCommandProperty = DependencyProperty.RegisterReadOnly(
//            "CloseTabCommand", typeof (ICommand), typeof (ClosableTabItemBehaviour), new PropertyMetadata(default(ICommand)));

//        public ClosableTabItemBehaviour()
//        {
//            CloseTabCommand = new DelegateCommand<TabItem>(OnCloseTab);
//        }

//        private void OnCloseTab(TabItem item)
//        {
//            if (null == item) return;
//            var parent = item.FindVisualParent<TabControl>();
//            if (null == parent) return;
//            parent.Items.Remove();
//        }

//        public ICommand CloseTabCommand
//        {
//            get { return (ICommand) GetValue(CloseTabCommandProperty.DependencyProperty); }
//            private set { SetValue(CloseTabCommandProperty, value); }
//        }
//    }
//}
