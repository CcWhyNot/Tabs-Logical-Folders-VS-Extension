using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TabsLogicalFolders
{
    /// <summary>
    /// Interaction logic for LogicalFoldersToolWindowControl.
    /// </summary>
    public partial class LogicalFoldersToolWindowControl : UserControl
    {
        public event Action<string> DocumentActivated;
        public event Action<string> NewGroupRequested;
        public event Action<string, string> DocumentDroppedOnFolder;

        //CONTEXT MENU
        public event Action<string> FolderDeleteRequested;
        public event Action<string, string> FolderRenameRequested;

        public event Action<string> DocumentCloseRequested;

        public enum NodeKind { Folder, Document, Other };

        public struct TabInfo
        {
            public string Caption;
            public string Moniker;
            public NodeKind Kind;
        }


        private HashSet<string> expandedFolders = new HashSet<string>();
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

                if (folder.Key == LogicalFoldersToolWindow.UNGROUPEDNAME)
                {
                    // TODO: PONER AQUI QUE NO SE PUEDA SELECCIONAR
                }
                else
                {
                    var contextMenu = new ContextMenu();

                    var deleteItem = new MenuItem { Header = "Delete" };
                    deleteItem.Click += (s, e) => FolderDeleteRequested?.Invoke(folder.Key);


                    var renameItem = new MenuItem { Header = "Rename" };
                    renameItem.Click += (s, e) =>
                    {
                        bool resolved = false;
                        var textBox = new TextBox { Text = folder.Key };
                        folderNode.Header = textBox;

                        textBox.Loaded += (s2, e2) =>
                        {
                            textBox.Focus();
                            textBox.SelectAll();
                        };

                        textBox.PreviewKeyDown += (s2, e2) =>
                        {
                            if (e2.Key == Key.Enter)
                            {
                                resolved = true;
                                FolderRenameRequested?.Invoke(folder.Key, textBox.Text);
                                e2.Handled = true;
                            }
                            // TODO : FIX escape. VS take control at escape key
                            //else if (e2.Key == Key.Escape)
                            //{
                            //    resolved = true;
                            //    folderNode.Header = folder.Key;
                            //    e2.Handled = true;
                            //}
                        };

                        textBox.LostFocus += (s2, e2) =>
                        {
                            if (!resolved)
                            {
                                resolved = true;
                                folderNode.Header = folder.Key;
                            }
                        };
                    };

                    contextMenu.Items.Add(renameItem);
                    contextMenu.Items.Add(deleteItem);


                    folderNode.ContextMenu = contextMenu;

                }


                folderNode.IsExpanded = expandedFolders.Contains(folder.Key);
                folderNode.Expanded += (s, e) => expandedFolders.Add(folder.Key);
                folderNode.Collapsed += (s, e) => expandedFolders.Remove(folder.Key);

                folderNode.AllowDrop = true;
                folderNode.DragOver += (s, e) =>
                {
                    e.Effects = DragDropEffects.Move;
                    e.Handled = true;
                };

                folderNode.Drop += (s, e) =>
                {
                    if (e.Data.GetDataPresent(DataFormats.StringFormat))
                    {
                        string moniker = (string)e.Data.GetData(DataFormats.StringFormat);
                        DocumentDroppedOnFolder?.Invoke(moniker, folder.Key);
                    }
                    e.Handled = true;
                };

                foreach (var item in folder.Value)
                {
                    var contextMenu = new ContextMenu();
                    var moveToItem = new MenuItem { Header = "Move to" };
                    foreach (var folderItem in grouped)
                    {
                        if (folderItem.Key == folder.Key) continue;
                        var folderMenuItem = new MenuItem { Header = folderItem.Key };
                        folderMenuItem.Click += (s2, e2) => DocumentDroppedOnFolder?.Invoke(item.Moniker, folderItem.Key);
                        moveToItem.Items.Add(folderMenuItem);
                    }
                    contextMenu.Items.Add(moveToItem);

                    var closeButton = new Button { Content = "X", Padding = new Thickness(4, 0, 4, 0) };
                    closeButton.Click += (s, e) => DocumentCloseRequested?.Invoke(item.Moniker);

                    var headerGrid = new Grid();
                    headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var textBlock = new TextBlock { Text = item.Caption, VerticalAlignment = VerticalAlignment.Center };

                    Grid.SetColumn(textBlock, 0);
                    Grid.SetColumn(closeButton, 1);

                    headerGrid.Children.Add(textBlock);
                    headerGrid.Children.Add(closeButton);

                    var leaf = new TreeViewItem { Header = headerGrid, Tag = (Kind: NodeKind.Document, Moniker: item.Moniker), HorizontalContentAlignment = HorizontalAlignment.Stretch };

                    Point? dragStartPoint = null;
                    leaf.PreviewMouseLeftButtonDown += (s, e) =>
                    {
                        dragStartPoint = e.GetPosition(null);
                    };

                    leaf.PreviewMouseMove += (s, e) =>
                    {
                        if (dragStartPoint.HasValue && e.LeftButton == MouseButtonState.Pressed)
                        {
                            var diff = dragStartPoint.Value - e.GetPosition(null);
                            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                            {
                                DragDrop.DoDragDrop(leaf, item.Moniker, DragDropEffects.Move);
                                Mouse.Capture(null);
                                dragStartPoint = null;
                            }
                        }

                    };

                    leaf.ContextMenu = contextMenu;
                    folderNode.Items.Add(leaf);
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

        private void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new NewGroupDialog();
            if (dialog.ShowDialog() == true)
                NewGroupRequested?.Invoke(dialog.GroupName);

        }
    }
}