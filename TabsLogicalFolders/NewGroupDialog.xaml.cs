using System.Windows;

namespace TabsLogicalFolders
{
    /// <summary>
    /// Lógica de interacción para NewGroupDialog.xaml
    /// </summary>
    public partial class NewGroupDialog : Window
    {
        public string GroupName { get; private set; }
        public NewGroupDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            GroupName = NameTextBox.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object obj, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NameTextBox.Focus();
        }
    }
}
