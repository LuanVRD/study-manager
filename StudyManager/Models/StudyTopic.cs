using System.Collections.ObjectModel;
using StudyManager.ViewModels;

namespace StudyManager.Models
{
    public class StudyTopic : ViewModelBase
    {
        private string _name = string.Empty;

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ObservableCollection<StudyTheme> Themes { get; set; } = new ObservableCollection<StudyTheme>();
    }
}
