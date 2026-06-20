using System.Windows;
using System.Windows.Input;

namespace StudyManager.Views.Dialogs
{
    public partial class TopicDialog : Window
    {
        public string ResultTopicName { get; private set; } = string.Empty;

        public TopicDialog(string? existingName = null)
        {
            InitializeComponent();

            if (existingName != null)
            {
                TitleTextBlock.Text = "Editar Tópico";
                NameTextBox.Text = existingName;
            }
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
            string topicName = NameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(topicName))
            {
                MessageBox.Show("O nome do tópico é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ResultTopicName = topicName;
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
