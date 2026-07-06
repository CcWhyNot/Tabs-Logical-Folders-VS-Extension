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

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalFoldersToolWindowControl"/> class.
        /// </summary>
        public LogicalFoldersToolWindowControl()
        {
            this.InitializeComponent();
        }

        public void PopulateTree(List<string> items)
        {
            var ungroupedNode = new TreeViewItem { Header = "Ungrouped" };
            foreach (var item in items)
                ungroupedNode.Items.Add(new TreeViewItem { Header = item });
            LogicalFolderTree.Items.Add(ungroupedNode);
        }

        private void LogicalFolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = (TreeViewItem)e.NewValue;
            // Si el item no es "una carpeta logica", es decir, no tiene hijos
            if (item.Items.Count == 0)
            {
                DocumentActivated.Invoke(item.Header.ToString());
            }
        }
    }
}