using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using StudyManager.Models;
using StudyManager.Services;
using StudyManager.Views.Dialogs;

namespace StudyManager.ViewModels
{
    public class StudiesViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        private string _searchQuery = string.Empty;

        public ObservableCollection<Study> Studies { get; }
        public ObservableCollection<SearchResultViewModel> SearchResults { get; } = new ObservableCollection<SearchResultViewModel>();

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    ExecuteSearch();
                }
            }
        }

        public ICommand AddStudyCommand { get; }
        public ICommand OpenStudyCommand { get; }
        public ICommand EditStudyCommand { get; }
        public ICommand DeleteStudyCommand { get; }
        public ICommand SyncStudiesCommand { get; }
        public ICommand ImportCsvCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand NavigateToSearchResultCommand { get; }

        public StudiesViewModel(MainViewModel main)
        {
            _main = main;
            // Load studies into observable collection for UI data binding
            Studies = new ObservableCollection<Study>(_main.AppData.Studies);

            AddStudyCommand = new RelayCommand(AddStudy);
            OpenStudyCommand = new RelayCommand(OpenStudy);
            EditStudyCommand = new RelayCommand(EditStudy);
            DeleteStudyCommand = new RelayCommand(DeleteStudy);
            SyncStudiesCommand = new RelayCommand(SyncStudies);
            ImportCsvCommand = new RelayCommand(ImportCsv);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            ClearSearchCommand = new RelayCommand(ClearSearch);
            NavigateToSearchResultCommand = new RelayCommand(NavigateToSearchResult);
        }

        private void AddStudy()
        {
            // Create and open the dialog
            var dialog = new StudyDialog();
            // Center dialog relative to app window
            if (Application.Current.MainWindow != null)
            {
                dialog.Owner = Application.Current.MainWindow;
            }

            if (dialog.ShowDialog() == true)
            {
                var newStudy = dialog.ResultStudy;
                if (newStudy != null)
                {
                    // Copy image if selected
                    if (!string.IsNullOrEmpty(dialog.SelectedImagePath))
                    {
                        string? localPath = _main.ImageService.CopyImageToLocal(dialog.SelectedImagePath, newStudy.Id);
                        newStudy.ImagePath = localPath;
                    }

                    _main.AppData.Studies.Add(newStudy);
                    Studies.Add(newStudy);
                    _main.SaveData();
                }
            }
        }

        private void OpenStudy(object? parameter)
        {
            if (parameter is Study study)
            {
                _main.NavigateToStudyDetails(study);
            }
        }

        private void EditStudy(object? parameter)
        {
            if (parameter is Study study)
            {
                var dialog = new StudyDialog(study);
                if (Application.Current.MainWindow != null)
                {
                    dialog.Owner = Application.Current.MainWindow;
                }

                if (dialog.ShowDialog() == true)
                {
                    // Copy new image if selected
                    if (!string.IsNullOrEmpty(dialog.SelectedImagePath))
                    {
                        // Delete old image file if it exists
                        if (!string.IsNullOrEmpty(study.ImagePath))
                        {
                            try
                            {
                                string oldAbsPath = _main.ImageService.GetAbsolutePath(study.ImagePath);
                                if (System.IO.File.Exists(oldAbsPath))
                                {
                                    System.IO.File.Delete(oldAbsPath);
                                }
                            }
                            catch
                            {
                                // Ignore cleanup errors
                            }
                        }

                        string? localPath = _main.ImageService.CopyImageToLocal(dialog.SelectedImagePath, study.Id);
                        study.ImagePath = localPath;
                    }

                    // Properties are automatically updated and notified by Study Dialog save event
                    _main.SaveData();
                }
            }
        }

        private void DeleteStudy(object? parameter)
        {
            if (parameter is Study study)
            {
                var result = MessageBox.Show(
                    $"Deseja realmente excluir o estudo \"{study.Name}\"?\n\nTodos os tópicos, temas, links e anotações relacionados também serão excluídos.",
                    "Confirmar Exclusão",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Delete image file off disk if exists
                    if (!string.IsNullOrEmpty(study.ImagePath))
                    {
                        try
                        {
                            string absPath = _main.ImageService.GetAbsolutePath(study.ImagePath);
                            if (System.IO.File.Exists(absPath))
                            {
                                System.IO.File.Delete(absPath);
                            }
                        }
                        catch
                        {
                            // Ignore cleanup errors
                        }
                    }

                    _main.AppData.Studies.Remove(study);
                    Studies.Remove(study);
                    _main.SaveData();
                }
            }
        }

        private void SyncStudies()
        {
            _main.AppData.Studies.Clear();
            foreach (var study in Studies)
            {
                _main.AppData.Studies.Add(study);
            }
            _main.SaveData();
        }

        private void ImportCsv()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Arquivos CSV (*.csv)|*.csv|Todos os arquivos (*.*)|*.*",
                Title = "Selecionar arquivo CSV para importar estudos"
            };

            bool? dialogResult = Application.Current.MainWindow != null
                ? openFileDialog.ShowDialog(Application.Current.MainWindow)
                : openFileDialog.ShowDialog();

            if (dialogResult == true)
            {
                var importer = new CsvImportService();
                var result = importer.ImportAll(openFileDialog.FileName, _main.AppData.Studies);

                if (result.Success)
                {
                    // Refresh the observable collection to reflect added studies/topics/themes
                    Studies.Clear();
                    foreach (var study in _main.AppData.Studies)
                    {
                        Studies.Add(study);
                    }

                    _main.SaveData();

                    // Display summary
                    MessageBox.Show(
                        $"Importação concluída.\n\n" +
                        $"• {result.StudiesCreated} estudos criados.\n" +
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

        private void OpenSettings()
        {
            var dialog = new SettingsDialog(_main.AppData.GeminiApiKey);
            if (Application.Current.MainWindow != null)
            {
                dialog.Owner = Application.Current.MainWindow;
            }

            if (dialog.ShowDialog() == true)
            {
                _main.AppData.GeminiApiKey = dialog.ResultApiKey;
                _main.SaveData();
            }
        }

        private void ClearSearch()
        {
            SearchQuery = string.Empty;
        }

        private void NavigateToSearchResult(object? parameter)
        {
            if (parameter is SearchResultViewModel result)
            {
                // Clear search before navigating so when returning to the home screen it is clean
                SearchQuery = string.Empty;

                if (result.Type == "Study")
                {
                    _main.NavigateToStudyDetails(result.Study);
                }
                else if (result.Type == "Topic")
                {
                    _main.NavigateToStudyDetails(result.Study);
                }
                else if (result.Type == "Theme")
                {
                    if (result.Topic != null && result.Theme != null)
                    {
                        _main.NavigateToThemeDetails(result.Study, result.Topic, result.Theme);
                    }
                }
                else if (result.Type == "Note")
                {
                    if (result.Topic != null && result.Theme != null)
                    {
                        _main.NavigateToThemeDetails(result.Study, result.Topic, result.Theme, "Notes");
                    }
                }
                else if (result.Type == "AiExplanation")
                {
                    if (result.Topic != null && result.Theme != null)
                    {
                        _main.NavigateToThemeDetails(result.Study, result.Topic, result.Theme, "AiExplanation");
                    }
                }
            }
        }

        private void ExecuteSearch()
        {
            SearchResults.Clear();
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                return;
            }

            string query = SearchQuery.Trim();

            foreach (var study in _main.AppData.Studies)
            {
                // 1. Search Study Name
                if (study.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase))
                {
                    SearchResults.Add(new SearchResultViewModel
                    {
                        Type = "Study",
                        DisplayTitle = study.Name,
                        DisplayPath = "Estudo",
                        Study = study
                    });
                }

                foreach (var topic in study.Topics)
                {
                    // 2. Search Topic Name
                    if (topic.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase))
                    {
                        SearchResults.Add(new SearchResultViewModel
                        {
                            Type = "Topic",
                            DisplayTitle = topic.Name,
                            DisplayPath = $"Estudo: {study.Name}",
                            Study = study,
                            Topic = topic
                        });
                    }

                    foreach (var theme in topic.Themes)
                    {
                        // 3. Search Theme Name
                        if (theme.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase))
                        {
                            SearchResults.Add(new SearchResultViewModel
                            {
                                Type = "Theme",
                                DisplayTitle = theme.Name,
                                DisplayPath = $"Estudo: {study.Name} › Tópico: {topic.Name}",
                                Study = study,
                                Topic = topic,
                                Theme = theme
                            });
                        }

                        // 4. Search Notes
                        if (!string.IsNullOrEmpty(theme.Notes))
                        {
                            string plainNotes = StripXaml(theme.Notes);
                            if (plainNotes.Contains(query, StringComparison.CurrentCultureIgnoreCase))
                            {
                                SearchResults.Add(new SearchResultViewModel
                                {
                                    Type = "Note",
                                    DisplayTitle = $"Anotações em: {theme.Name}",
                                    DisplayPath = $"Estudo: {study.Name} › Tópico: {topic.Name} › Tema: {theme.Name}",
                                    DisplaySnippet = ExtractSnippet(plainNotes, query),
                                    Study = study,
                                    Topic = topic,
                                    Theme = theme
                                });
                            }
                        }

                        // 5. Search AI Explanation
                        if (!string.IsNullOrEmpty(theme.AiExplanation))
                        {
                            string plainAi = StripXaml(theme.AiExplanation);
                            if (plainAi.Contains(query, StringComparison.CurrentCultureIgnoreCase))
                            {
                                SearchResults.Add(new SearchResultViewModel
                                {
                                    Type = "AiExplanation",
                                    DisplayTitle = $"Explicação da IA em: {theme.Name}",
                                    DisplayPath = $"Estudo: {study.Name} › Tópico: {topic.Name} › Tema: {theme.Name}",
                                    DisplaySnippet = ExtractSnippet(plainAi, query),
                                    Study = study,
                                    Topic = topic,
                                    Theme = theme
                                });
                            }
                        }
                    }
                }
            }
        }

        private static string StripXaml(string xaml)
        {
            if (string.IsNullOrEmpty(xaml)) return string.Empty;
            string plain = System.Text.RegularExpressions.Regex.Replace(xaml, "<[^>]*>", " ");
            plain = System.Net.WebUtility.HtmlDecode(plain);
            plain = System.Text.RegularExpressions.Regex.Replace(plain, @"\s+", " ").Trim();
            return plain;
        }

        private static string ExtractSnippet(string text, string query)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            int index = text.IndexOf(query, StringComparison.CurrentCultureIgnoreCase);
            if (index < 0) return text.Length > 150 ? text.Substring(0, 150) + "..." : text;

            int start = Math.Max(0, index - 60);
            int length = Math.Min(text.Length - start, 150);
            string snippet = text.Substring(start, length);

            if (start > 0) snippet = "..." + snippet;
            if (start + length < text.Length) snippet = snippet + "...";

            return snippet;
        }
    }
}
