using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using iLynx.Common;
using iLynx.Common.WPF;

namespace iLynx.TestBench.TabPages
{
    public class TreeNode : NotificationBase
    {
        private readonly Action<TreeNode> addBeforeCallback;
        private readonly Action<TreeNode> addAfterCallback;
        private string header;
        private readonly ObservableCollection<TreeNode> children = new ObservableCollection<TreeNode>();
        private ICommand addBeforeCommand;
        private ICommand addAfterCommand;
        private ICommand addChildCommand;

        public TreeNode(Action<TreeNode> addBeforeCallback, Action<TreeNode> addAfterCallback)
        {
            this.addBeforeCallback = addBeforeCallback;
            this.addAfterCallback = addAfterCallback;
        }

        public string Header
        {
            get { return header; }
            set
            {
                if (value == header) return;
                header = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TreeNode> Children
        {
            get { return children; }
        }

        public ICommand AddBeforeCommand
        {
            get { return addBeforeCommand ?? (addBeforeCommand = new DelegateCommand(OnAddBefore)); }
        }

        public ICommand AddAfterCommand
        {
            get { return addAfterCommand ?? (addAfterCommand = new DelegateCommand(OnAddAfter)); }
        }

        public ICommand AddChildCommand
        {
            get { return addChildCommand ?? (addChildCommand = new DelegateCommand(OnAddChild)); }
        }

        private void OnAddChild()
        {
            children.Add(new TreeNode(AddBefore, AddAfter));
        }

        private void AddAfter(TreeNode treeNode)
        {
            var index = children.IndexOf(treeNode);
            if (-1 == index && 0 != children.Count)
                index = 0;
            children.Insert(index + 1, new TreeNode(AddBefore, AddAfter));
        }

        private void AddBefore(TreeNode treeNode)
        {
            var index = children.IndexOf(treeNode);
            if (-1 == index)
                index = 0;
            children.Insert(index, new TreeNode(AddBefore, AddAfter));
        }

        private void OnAddAfter()
        {
            addAfterCallback(this);
        }

        private void OnAddBefore()
        {
            addBeforeCallback(this);
        }
    }
}
