using System;
using System.Windows;
using System.Windows.Input;
using StudyManager.Models;

namespace StudyManager.Views.Dialogs
{
    public partial class LinkDialog : Window
    {
        public string ResultLinkName { get; private set; } = string.Empty;
        public string ResultLinkUrl { get; private set; } = string.Empty;
        public LinkType ResultLinkType { get; private set; } = LinkType.Article;

        public LinkDialog(string? existingName = null, string? existingUrl = null, LinkType existingType = LinkType.Article)
        {
            InitializeComponent();

            if (existingName != null)
            {
                TitleTextBlock.Text = "Editar Link";
                NameTextBox.Text = existingName;
                UrlTextBox.Text = existingUrl;

                if (existingType == LinkType.Video)
                {
                    VideoRadioButton.IsChecked = true;
                }
                else
                {
                    ArticleRadioButton.IsChecked = true;
                }
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
            string linkName = NameTextBox.Text.Trim();
            string linkUrl = UrlTextBox.Text.Trim();
            LinkType linkType = VideoRadioButton.IsChecked == true ? LinkType.Video : LinkType.Article;

            if (string.IsNullOrWhiteSpace(linkName))
            {
                MessageBox.Show("O nome do link é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(linkUrl))
            {
                MessageBox.Show("A URL do link é obrigatória.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Simple URL validation: must start with http:// or https://
            if (!linkUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !linkUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("A URL deve começar com http:// ou https://.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ResultLinkName = linkName;
            ResultLinkUrl = linkUrl;
            ResultLinkType = linkType;

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
