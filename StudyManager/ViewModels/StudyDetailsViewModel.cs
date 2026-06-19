using System.Windows.Input;
using StudyManager.Models;

namespace StudyManager.ViewModels
{
    public class StudyDetailsViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        
        public Study Study { get; }
        
        public ICommand BackCommand { get; }

        public StudyDetailsViewModel(MainViewModel main, Study study)
        {
            _main = main;
            Study = study;
            
            BackCommand = new RelayCommand(GoBack);
        }

        private void GoBack()
        {
            _main.NavigateToStudies();
        }
    }
}
