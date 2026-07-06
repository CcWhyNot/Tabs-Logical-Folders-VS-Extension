using System.Collections.Generic;
using System.Windows.Controls;

namespace TabsLogicalFolders
{
    /// <summary>
    /// Interaction logic for LogicalFoldersToolWindowControl.
    /// </summary>
    public partial class LogicalFoldersToolWindowControl : UserControl
    {
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
    }
}