using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using StudyManager.Models;

namespace StudyManager.Views.Dialogs
{
    public partial class StudyDialog : Window
    {
        private readonly Study? _existingStudy;
        public Study? ResultStudy { get; private set; }
        public string? SelectedImagePath { get; private set; }

        public StudyDialog(Study? existingStudy = null)
        {
            InitializeComponent();
            _existingStudy = existingStudy;

            if (_existingStudy != null)
            {
                TitleTextBlock.Text = "Editar Estudo";
                NameTextBox.Text = _existingStudy.Name;
                if (!string.IsNullOrEmpty(_existingStudy.ImagePath))
                {
                    ImagePathTextBox.Text = Path.GetFileName(_existingStudy.ImagePath);
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

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Arquivos de Imagem (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif|Todos os arquivos (*.*)|*.*",
                Title = "Selecionar Imagem de Capa"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedImagePath = openFileDialog.FileName;
                ImagePathTextBox.Text = openFileDialog.SafeFileName;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string studyName = NameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(studyName))
            {
                MessageBox.Show("O nome do estudo é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_existingStudy != null)
            {
                // Editing mode: result will carry the updated details
                ResultStudy = _existingStudy;
                ResultStudy.Name = studyName;
                ResultStudy.UpdatedAt = DateTime.Now;
            }
            else
            {
                // Creation mode
                ResultStudy = new Study
                {
                    Name = studyName,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
            }

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
