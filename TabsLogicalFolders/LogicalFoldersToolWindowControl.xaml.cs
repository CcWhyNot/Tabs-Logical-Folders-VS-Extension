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
        public enum NodeKind { Folder, Document, Other };

        public struct TabInfo
        {
            public string Caption;
            public string Moniker;
            public NodeKind Kind;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalFoldersToolWindowControl"/> class.
        /// </summary>
        public LogicalFoldersToolWindowControl()
        {
            this.InitializeComponent();
        }

        public void PopulateTree(List<TabInfo> items)
        {
            LogicalFolderTree.Items.Clear();

            var ungroupedNode = new TreeViewItem { Header = "Ungrouped", Tag = (kind: NodeKind.Folder, Moniker: (string)null) };
            var otherNode = new TreeViewItem { Header = "Other", Tag = (NodeKind.Other, Moniker: (string)null) };
            foreach (var item in items)
            {
                if (item.Kind == NodeKind.Document)
                    ungroupedNode.Items.Add(new TreeViewItem { Header = item.Caption, Tag = (Kind: NodeKind.Document, Moniker: item.Moniker) });
                else if (item.Kind == NodeKind.Other)
                    otherNode.Items.Add(new TreeViewItem { Header = item.Caption, Tag = (Kind: NodeKind.Other, Moniker: item.Moniker) });
            }
            LogicalFolderTree.Items.Add(ungroupedNode);
            LogicalFolderTree.Items.Add(otherNode);
        }

        private void LogicalFolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = (TreeViewItem)e.NewValue;
            if (item == null) return;

            var (kind, moniker) = ((NodeKind, string))item.Tag;
            if (kind != NodeKind.Folder && !string.IsNullOrEmpty(moniker))
            {
                DocumentActivated?.Invoke(moniker);
            }
        }
    }
}