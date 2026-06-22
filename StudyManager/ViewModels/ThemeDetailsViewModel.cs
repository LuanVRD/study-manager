using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using StudyManager.Models;
using StudyManager.Views.Dialogs;
using StudyManager.Services;

namespace StudyManager.ViewModels
{
    public class ThemeDetailsViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        public Study Study { get; }
        public StudyTopic Topic { get; }
        public StudyTheme Theme { get; }

        private DispatcherTimer? _saveTimer;
        private string _notes = string.Empty;
        private string _saveStatus = "Salvo";
        private bool _isNotesExpanded;
        private bool _isAiExplanationExpanded;
        private bool _isLinksExpanded;
        private bool _isQuizExpanded;
        private int _selectedTabIndex;
        private bool _isConsultingGemini;

        public bool IsConsultingGemini
        {
            get => _isConsultingGemini;
            set => SetProperty(ref _isConsultingGemini, value);
        }

        public string Notes
        {
            get => _notes;
            set
            {
                if (SetProperty(ref _notes, value))
                {
                    OnNotesChanged();
                }
            }
        }

        public string AiExplanation
        {
            get => Theme.AiExplanation;
            set
            {
                if (Theme.AiExplanation != value)
                {
                    Theme.AiExplanation = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<ThemeQuestion> Questions => Theme.Questions;

        public string SaveStatus
        {
            get => _saveStatus;
            set => SetProperty(ref _saveStatus, value);
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        public bool IsNotesExpanded
        {
            get => _isNotesExpanded;
            set
            {
                if (SetProperty(ref _isNotesExpanded, value) && value)
                {
                    IsAiExplanationExpanded = false;
                }
            }
        }

        public bool IsAiExplanationExpanded
        {
            get => _isAiExplanationExpanded;
            set
            {
                if (SetProperty(ref _isAiExplanationExpanded, value) && value)
                {
                    IsNotesExpanded = false;
                }
            }
        }

        public bool IsLinksExpanded
        {
            get => _isLinksExpanded;
            set
            {
                if (SetProperty(ref _isLinksExpanded, value) && value)
                {
                    IsQuizExpanded = false;
                }
            }
        }

        public bool IsQuizExpanded
        {
            get => _isQuizExpanded;
            set
            {
                if (SetProperty(ref _isQuizExpanded, value) && value)
                {
                    IsLinksExpanded = false;
                }
            }
        }

        public ICommand BackCommand { get; }
        public ICommand AddLinkCommand { get; }
        public ICommand EditLinkCommand { get; }
        public ICommand DeleteLinkCommand { get; }
        public ICommand OpenLinkCommand { get; }
        public ICommand SaveDataCommand { get; }
        public ICommand ToggleNotesExpandedCommand { get; }
        public ICommand ToggleAiExplanationExpandedCommand { get; }
        public ICommand ToggleLinksExpandedCommand { get; }
        public ICommand ToggleQuizExpandedCommand { get; }
        public ICommand ToggleCompleteCommand { get; }
        public ICommand GenerateStudyGuideCommand { get; }
        public ICommand AnswerQuestionCommand { get; }
        public ICommand ClearAnswersCommand { get; }
        public ICommand CopyAiExplanationCommand { get; }

        public ThemeDetailsViewModel(MainViewModel main, Study study, StudyTopic topic, StudyTheme theme)
        {
            _main = main;
            Study = study;
            Topic = topic;
            Theme = theme;
            _notes = theme.Notes; // Set backing field directly to avoid triggering save timer on load
            
            // Wire up parent references on loaded questions
            if (Theme.Questions != null)
            {
                foreach (var q in Theme.Questions)
                {
                    if (q.Options != null)
                    {
                        foreach (var opt in q.Options)
                        {
                            opt.Question = q;
                        }
                    }
                }
            }

            BackCommand = new RelayCommand(GoBack);
            AddLinkCommand = new RelayCommand(AddLink);
            EditLinkCommand = new RelayCommand(EditLink);
            DeleteLinkCommand = new RelayCommand(DeleteLink);
            OpenLinkCommand = new RelayCommand(OpenLink);
            SaveDataCommand = new RelayCommand(() => _main.SaveData());
            
            ToggleNotesExpandedCommand = new RelayCommand(() => IsNotesExpanded = !IsNotesExpanded);
            ToggleAiExplanationExpandedCommand = new RelayCommand(() => IsAiExplanationExpanded = !IsAiExplanationExpanded);
            ToggleLinksExpandedCommand = new RelayCommand(() => IsLinksExpanded = !IsLinksExpanded);
            ToggleQuizExpandedCommand = new RelayCommand(() => IsQuizExpanded = !IsQuizExpanded);
            
            ToggleCompleteCommand = new RelayCommand(ToggleComplete);
            GenerateStudyGuideCommand = new RelayCommand(GenerateStudyGuide, () => !IsConsultingGemini);
            AnswerQuestionCommand = new RelayCommand(AnswerQuestion);
            ClearAnswersCommand = new RelayCommand(ClearAnswers);
            CopyAiExplanationCommand = new RelayCommand(CopyAiExplanation);
        }

        private void GoBack()
        {
            _main.NavigateToStudyDetails(Study);
        }

        private void ToggleComplete()
        {
            Theme.IsCompleted = !Theme.IsCompleted;
            _main.SaveData();
            Study.NotifyProgressChanged();
        }

        private void OnNotesChanged()
        {
            SaveStatus = "Salvando...";

            if (_saveTimer == null)
            {
                _saveTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(600)
                };
                _saveTimer.Tick += SaveTimer_Tick;
            }

            _saveTimer.Stop();
            _saveTimer.Start();
        }

        private void SaveTimer_Tick(object? sender, EventArgs e)
        {
            _saveTimer?.Stop();
            Theme.Notes = Notes;
            _main.SaveData();
            SaveStatus = "Salvo";
        }

        public void SavePendingChanges()
        {
            if (_saveTimer != null && _saveTimer.IsEnabled)
            {
                _saveTimer.Stop();
                Theme.Notes = Notes;
                _main.SaveData();
                SaveStatus = "Salvo";
            }
        }

        private void AddLink()
        {
            var dialog = new LinkDialog();
            if (Application.Current.MainWindow != null)
            {
                dialog.Owner = Application.Current.MainWindow;
            }

            if (dialog.ShowDialog() == true)
            {
                var newLink = new StudyLink
                {
                    Name = dialog.ResultLinkName,
                    Url = dialog.ResultLinkUrl,
                    Type = dialog.ResultLinkType
                };

                Theme.Links.Add(newLink);
                _main.SaveData();
            }
        }

        private void EditLink(object? parameter)
        {
            if (parameter is StudyLink link)
            {
                var dialog = new LinkDialog(link.Name, link.Url, link.Type);
                if (Application.Current.MainWindow != null)
                {
                    dialog.Owner = Application.Current.MainWindow;
                }

                if (dialog.ShowDialog() == true)
                {
                    link.Name = dialog.ResultLinkName;
                    link.Url = dialog.ResultLinkUrl;
                    link.Type = dialog.ResultLinkType;
                    
                    _main.SaveData();
                }
            }
        }

        private void DeleteLink(object? parameter)
        {
            if (parameter is StudyLink link)
            {
                var result = MessageBox.Show(
                    $"Deseja realmente excluir o link \"{link.Name}\"?",
                    "Confirmar Exclusão",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Theme.Links.Remove(link);
                    _main.SaveData();
                }
            }
        }

        private void OpenLink(object? parameter)
        {
            if (parameter is StudyLink link)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = link.Url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Não foi possível abrir o link: {ex.Message}",
                        "Erro ao Abrir Link",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private async void GenerateStudyGuide()
        {
            string apiKey = _main.AppData.GeminiApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                MessageBox.Show(
                    "Por favor, configure a chave de API do Gemini nas configurações da tela inicial antes de gerar o guia de estudo.",
                    "Configuração Requerida",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            IsConsultingGemini = true;
            SaveStatus = "Gerando guia de estudos...";

            try
            {
                var service = new GeminiService();
                var result = await service.GenerateStudyGuideAsync(Study.Name, Topic.Name, Theme.Name, apiKey);

                // 1. Process explanation (convert markdown to FlowDocument XAML)
                string xaml = GeminiService.AppendMarkdownToXaml(string.Empty, result.Explanation);
                AiExplanation = xaml;

                // 2. Add links to Theme.Links
                if (result.VideoLink != null && !string.IsNullOrWhiteSpace(result.VideoLink.Url))
                {
                    Theme.Links.Add(new StudyLink
                    {
                        Name = result.VideoLink.Name,
                        Url = result.VideoLink.Url,
                        Type = LinkType.Video
                    });
                }
                if (result.ArticleLink != null && !string.IsNullOrWhiteSpace(result.ArticleLink.Url))
                {
                    Theme.Links.Add(new StudyLink
                    {
                        Name = result.ArticleLink.Name,
                        Url = result.ArticleLink.Url,
                        Type = LinkType.Article
                    });
                }

                // 3. Set questions in Theme.Questions
                Theme.Questions.Clear();
                if (result.Questions != null)
                {
                    foreach (var qResult in result.Questions)
                    {
                        var newQuestion = new ThemeQuestion
                        {
                            QuestionText = qResult.QuestionText,
                            CorrectIndex = qResult.CorrectIndex,
                            SelectedIndex = null
                        };
                        for (int i = 0; i < qResult.Options.Count; i++)
                        {
                            newQuestion.Options.Add(new QuestionOption
                            {
                                Question = newQuestion,
                                Text = qResult.Options[i],
                                Index = i
                            });
                        }
                        Theme.Questions.Add(newQuestion);
                    }
                }

                _main.SaveData();
                SaveStatus = "Salvo";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao gerar o guia de estudos:\n{ex.Message}",
                    "Erro de IA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                SaveStatus = "Erro na geração";
            }
            finally
            {
                IsConsultingGemini = false;
            }
        }

        private void AnswerQuestion(object? parameter)
        {
            if (parameter is QuestionOption option)
            {
                if (option.Question.SelectedIndex == null)
                {
                    option.Question.SelectedIndex = option.Index;
                    _main.SaveData();
                }
            }
        }

        private void ClearAnswers()
        {
            if (Theme.Questions != null && Theme.Questions.Count > 0)
            {
                var result = MessageBox.Show(
                    "Deseja realmente reiniciar o questionário?",
                    "Reiniciar Questionário",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    foreach (var q in Theme.Questions)
                    {
                        q.SelectedIndex = null;
                    }
                    _main.SaveData();
                }
            }
        }

        private void CopyAiExplanation()
        {
            if (string.IsNullOrWhiteSpace(AiExplanation)) return;

            try
            {
                var doc = GeminiService.XamlToFlowDocument(AiExplanation);
                var textRange = new TextRange(doc.ContentStart, doc.ContentEnd);
                string plainText = textRange.Text.Trim();
                
                Clipboard.SetText(plainText);
                MessageBox.Show("Texto da explicação copiado para a área de transferência!", "Copiado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao copiar explicação: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
