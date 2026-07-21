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

        public void PopulateTree(Dictionary<string, List<TabInfo>> grouped, List<TabInfo> otherTabs)
        {
            LogicalFolderTree.Items.Clear();

            foreach (var folder in grouped)
            {
                var folderNode = new TreeViewItem { Header = folder.Key, Tag = (Kind: NodeKind.Folder, Moniker: (string)null) };

                foreach (var item in folder.Value)
                {
                    folderNode.Items.Add(new TreeViewItem { Header = item.Caption, Tag = (Kind: NodeKind.Document, Moniker: item.Moniker) });
                }
                LogicalFolderTree.Items.Add(folderNode);
            }

            var otherNode = new TreeViewItem { Header = LogicalFoldersToolWindow.OTHERNAME, Tag = (NodeKind.Other, Moniker: (string)null) };
            foreach (var item in otherTabs)
            {
                otherNode.Items.Add(new TreeViewItem { Header = item.Caption, Tag = (Kind: NodeKind.Other, Moniker: item.Moniker) });
            }
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