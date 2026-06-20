using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using StudyManager.Models;
using StudyManager.Views.Dialogs;

namespace StudyManager.ViewModels
{
    public class ThemeDetailsViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        public Study Study { get; }
        public StudyTopic Topic { get; }
        public StudyTheme Theme { get; }

        public ICommand BackCommand { get; }
        public ICommand AddLinkCommand { get; }
        public ICommand EditLinkCommand { get; }
        public ICommand DeleteLinkCommand { get; }
        public ICommand OpenLinkCommand { get; }

        public ThemeDetailsViewModel(MainViewModel main, Study study, StudyTopic topic, StudyTheme theme)
        {
            _main = main;
            Study = study;
            Topic = topic;
            Theme = theme;
            
            BackCommand = new RelayCommand(GoBack);
            AddLinkCommand = new RelayCommand(AddLink);
            EditLinkCommand = new RelayCommand(EditLink);
            DeleteLinkCommand = new RelayCommand(DeleteLink);
            OpenLinkCommand = new RelayCommand(OpenLink);
        }

        private void GoBack()
        {
            _main.NavigateToStudyDetails(Study);
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
    }
}
