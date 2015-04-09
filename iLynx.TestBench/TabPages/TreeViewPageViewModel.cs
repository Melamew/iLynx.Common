using System;
using System.Collections.ObjectModel;

namespace iLynx.TestBench.TabPages
{
    public class TreeViewPageViewModel
    {
        private readonly ObservableCollection<TreeNode> items = new ObservableCollection<TreeNode>();

        public TreeViewPageViewModel()
        {
            items.Add(new TreeNode(AddBeforeCallback, AddAfterCallback) { Header = "Item 1" });
        }

        private void AddAfterCallback(TreeNode treeNode)
        {
            var index = items.IndexOf(treeNode);
            if (-1 == index && 0 != items.Count)
                index = 0;
            items.Insert(index + 1, new TreeNode(AddBeforeCallback, AddAfterCallback));
        }

        private void AddBeforeCallback(TreeNode treeNode)
        {
            var index = items.IndexOf(treeNode);
            if (-1 == index)
                index = 0;
            items.Insert(index, new TreeNode(AddBeforeCallback, AddAfterCallback));
        }

        public ObservableCollection<TreeNode> Items
        {
            get { return items; }
        } 
    }
}
