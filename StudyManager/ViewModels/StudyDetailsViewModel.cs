using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using StudyManager.Models;
using StudyManager.Services;
using StudyManager.Views.Dialogs;

namespace StudyManager.ViewModels
{
    public class ThemeCommandParameter
    {
        public StudyTopic Topic { get; }
        public StudyTheme Theme { get; }

        public ThemeCommandParameter(StudyTopic topic, StudyTheme theme)
        {
            Topic = topic;
            Theme = theme;
        }
    }

    public class StudyDetailsViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        
        public Study Study { get; }
        
        public ICommand BackCommand { get; }
        public ICommand AddTopicCommand { get; }
        public ICommand EditTopicCommand { get; }
        public ICommand DeleteTopicCommand { get; }
        public ICommand AddThemeCommand { get; }
        public ICommand EditThemeCommand { get; }
        public ICommand DeleteThemeCommand { get; }
        public ICommand ToggleThemeCommand { get; }
        public ICommand OpenThemeCommand { get; }
        public ICommand ImportCsvCommand { get; }

        public StudyDetailsViewModel(MainViewModel main, Study study)
        {
            _main = main;
            Study = study;
            
            BackCommand = new RelayCommand(GoBack);
            AddTopicCommand = new RelayCommand(AddTopic);
            EditTopicCommand = new RelayCommand(EditTopic);
            DeleteTopicCommand = new RelayCommand(DeleteTopic);
            AddThemeCommand = new RelayCommand(AddTheme);
            EditThemeCommand = new RelayCommand(EditTheme);
            DeleteThemeCommand = new RelayCommand(DeleteTheme);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            OpenThemeCommand = new RelayCommand(OpenTheme);
            ImportCsvCommand = new RelayCommand(ImportCsv);
        }

        private void GoBack()
        {
            _main.NavigateToStudies();
        }

        private void AddTopic()
        {
            var dialog = new TopicDialog();
            if (Application.Current.MainWindow != null)
            {
                dialog.Owner = Application.Current.MainWindow;
            }

            if (dialog.ShowDialog() == true)
            {
                var newTopic = new StudyTopic
                {
                    Name = dialog.ResultTopicName
                };

                Study.Topics.Add(newTopic);
                _main.SaveData();
                Study.NotifyProgressChanged();
            }
        }

        private void EditTopic(object? parameter)
        {
            if (parameter is StudyTopic topic)
            {
                var dialog = new TopicDialog(topic.Name);
                if (Application.Current.MainWindow != null)
                {
                    dialog.Owner = Application.Current.MainWindow;
                }

                if (dialog.ShowDialog() == true)
                {
                    topic.Name = dialog.ResultTopicName;
                    _main.SaveData();
                }
            }
        }

        private void DeleteTopic(object? parameter)
        {
            if (parameter is StudyTopic topic)
            {
                var result = MessageBox.Show(
                    $"Deseja realmente excluir o tópico \"{topic.Name}\"?\n\nTodos os temas, links e anotações relacionados também serão excluídos.",
                    "Confirmar Exclusão",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Study.Topics.Remove(topic);
                    _main.SaveData();
                    Study.NotifyProgressChanged();
                }
            }
        }

        private void AddTheme(object? parameter)
        {
            if (parameter is StudyTopic topic)
            {
                var dialog = new ThemeDialog();
                if (Application.Current.MainWindow != null)
                {
                    dialog.Owner = Application.Current.MainWindow;
                }

                if (dialog.ShowDialog() == true)
                {
                    var newTheme = new StudyTheme
                    {
                        Name = dialog.ResultThemeName
                    };

                    topic.Themes.Add(newTheme);
                    _main.SaveData();
                    Study.NotifyProgressChanged();
                }
            }
        }

        private void EditTheme(object? parameter)
        {
            if (parameter is StudyTheme theme)
            {
                var dialog = new ThemeDialog(theme.Name);
                if (Application.Current.MainWindow != null)
                {
                    dialog.Owner = Application.Current.MainWindow;
                }

                if (dialog.ShowDialog() == true)
                {
                    theme.Name = dialog.ResultThemeName;
                    _main.SaveData();
                }
            }
        }

        private void DeleteTheme(object? parameter)
        {
            if (parameter is ThemeCommandParameter param)
            {
                var result = MessageBox.Show(
                    $"Deseja realmente excluir o tema \"{param.Theme.Name}\"?\n\nSeus links e anotações também serão excluídos.",
                    "Confirmar Exclusão",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    param.Topic.Themes.Remove(param.Theme);
                    _main.SaveData();
                    Study.NotifyProgressChanged();
                }
            }
        }

        private void ToggleTheme(object? parameter)
        {
            if (parameter is StudyTheme)
            {
                _main.SaveData();
                Study.NotifyProgressChanged();
            }
        }

        private void OpenTheme(object? parameter)
        {
            if (parameter is ThemeCommandParameter param)
            {
                _main.NavigateToThemeDetails(Study, param.Topic, param.Theme);
            }
        }

        private void ImportCsv()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Arquivos CSV (*.csv)|*.csv|Todos os arquivos (*.*)|*.*",
                Title = "Selecionar arquivo CSV para importar"
            };

            bool? dialogResult = Application.Current.MainWindow != null
                ? openFileDialog.ShowDialog(Application.Current.MainWindow)
                : openFileDialog.ShowDialog();

            if (dialogResult == true)
            {
                var importer = new CsvImportService();
                var result = importer.Import(openFileDialog.FileName, Study);

                if (result.Success)
                {
                    // Save and notify progress change
                    _main.SaveData();
                    Study.NotifyProgressChanged();

                    // Display summary
                    MessageBox.Show(
                        $"Importação concluída.\n\n" +
                        $"• {result.TopicsCreated} tópicos criados.\n" +
                        $"• {result.ThemesAdded} temas adicionados.\n" +
                        $"• {result.ThemesIgnored} temas ignorados por já existirem.\n" +
                        $"• {result.InvalidLines} linhas inválidas ignoradas.",
                        "Resumo da Importação",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        $"Falha na importação:\n{result.ErrorMessage}",
                        "Erro de Importação",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}
