using System.Windows;
using System.Windows.Input;

namespace StudyManager.Views.Dialogs
{
    public partial class SettingsDialog : Window
    {
        public string ResultApiKey { get; private set; } = string.Empty;

        public SettingsDialog(string currentApiKey)
        {
            InitializeComponent();
            ApiKeyTextBox.Text = currentApiKey;
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ResultApiKey = ApiKeyTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
