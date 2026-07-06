using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TabsLogicalFolders
{
    /// <summary>
    /// Interaction logic for LogicalFoldersToolWindowControl.
    /// </summary>
    public partial class LogicalFoldersToolWindowControl : UserControl
    {
        public event Action<string> DocumentActivated;
        public enum NodeKind { Folder, Document };

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalFoldersToolWindowControl"/> class.
        /// </summary>
        public LogicalFoldersToolWindowControl()
        {
            this.InitializeComponent();
        }

        public void PopulateTree(List<string> items)
        {
            var ungroupedNode = new TreeViewItem { Header = "Ungrouped", Tag = NodeKind.Folder };
            foreach (var item in items)
                ungroupedNode.Items.Add(new TreeViewItem { Header = item, Tag = NodeKind.Document });
            LogicalFolderTree.Items.Add(ungroupedNode);
        }

        private void LogicalFolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = (TreeViewItem)e.NewValue;
            if ((NodeKind)item.Tag == NodeKind.Document)
            {
                DocumentActivated?.Invoke(item.Header.ToString());
            }
        }
    }
}