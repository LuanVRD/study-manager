using System;
using System.Collections.ObjectModel;
using StudyManager.ViewModels;

namespace StudyManager.Models
{
    public class ThemeQuestion : ViewModelBase
    {
        private string _questionText = string.Empty;
        private ObservableCollection<QuestionOption> _options = new ObservableCollection<QuestionOption>();
        private int _correctIndex;
        private int? _selectedIndex;

        public string QuestionText
        {
            get => _questionText;
            set => SetProperty(ref _questionText, value);
        }

        public ObservableCollection<QuestionOption> Options
        {
            get => _options;
            set => SetProperty(ref _options, value);
        }

        public int CorrectIndex
        {
            get => _correctIndex;
            set => SetProperty(ref _correctIndex, value);
        }

        public int? SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (SetProperty(ref _selectedIndex, value))
                {
                    if (Options != null)
                    {
                        foreach (var opt in Options)
                        {
                            opt.NotifyStateChanged();
                        }
                    }
                }
            }
        }
    }
}
