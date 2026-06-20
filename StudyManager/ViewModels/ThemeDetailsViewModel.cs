using System.Windows.Input;
using StudyManager.Models;

namespace StudyManager.ViewModels
{
    public class ThemeDetailsViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        public Study Study { get; }
        public StudyTopic Topic { get; }
        public StudyTheme Theme { get; }

        public ICommand BackCommand { get; }

        public ThemeDetailsViewModel(MainViewModel main, Study study, StudyTopic topic, StudyTheme theme)
        {
            _main = main;
            Study = study;
            Topic = topic;
            Theme = theme;
            
            BackCommand = new RelayCommand(GoBack);
        }

        private void GoBack()
        {
            _main.NavigateToStudyDetails(Study);
        }
    }
}
