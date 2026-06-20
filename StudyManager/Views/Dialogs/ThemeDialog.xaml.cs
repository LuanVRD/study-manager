using System.Windows;
using System.Windows.Input;

namespace StudyManager.Views.Dialogs
{
    public partial class ThemeDialog : Window
    {
        public string ResultThemeName { get; private set; } = string.Empty;

        public ThemeDialog(string? existingName = null)
        {
            InitializeComponent();

            if (existingName != null)
            {
                TitleTextBlock.Text = "Editar Tema";
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
            string themeName = NameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(themeName))
            {
                MessageBox.Show("O nome do tema é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ResultThemeName = themeName;
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
