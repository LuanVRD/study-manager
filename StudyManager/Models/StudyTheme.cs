using System;
using System.Collections.ObjectModel;
using StudyManager.ViewModels;

namespace StudyManager.Models
{
    public class StudyTheme : ViewModelBase
    {
        private string _name = string.Empty;
        private bool _isCompleted;
        private string _notes = string.Empty;
        private ObservableCollection<StudyLink> _links = new ObservableCollection<StudyLink>();

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetProperty(ref _isCompleted, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public ObservableCollection<StudyLink> Links
        {
            get => _links;
            set => SetProperty(ref _links, value);
        }
    }
}
