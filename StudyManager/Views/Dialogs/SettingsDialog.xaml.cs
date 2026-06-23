using System.Windows;
using System.Windows.Input;

namespace StudyManager.Views.Dialogs
{
    public partial class SettingsDialog : Window
    {
        public string ResultGeminiApiKey { get; private set; } = string.Empty;
        public string ResultGroqApiKey { get; private set; } = string.Empty;

        public SettingsDialog(string currentGeminiApiKey, string currentGroqApiKey)
        {
            InitializeComponent();
            ApiKeyTextBox.Text = currentGeminiApiKey;
            GroqApiKeyTextBox.Text = currentGroqApiKey;
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
            ResultGeminiApiKey = ApiKeyTextBox.Text.Trim();
            ResultGroqApiKey = GroqApiKeyTextBox.Text.Trim();
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
