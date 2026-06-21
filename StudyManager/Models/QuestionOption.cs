using System;
using System.Text.Json.Serialization;
using StudyManager.ViewModels;

namespace StudyManager.Models
{
    public class QuestionOption : ViewModelBase
    {
        [JsonIgnore]
        public ThemeQuestion Question { get; set; } = null!;
        
        public string Text { get; set; } = string.Empty;
        public int Index { get; set; }

        [JsonIgnore]
        public bool IsSelected => Question?.SelectedIndex == Index;

        [JsonIgnore]
        public bool IsCorrect => Question?.CorrectIndex == Index;

        [JsonIgnore]
        public bool IsIncorrect => IsSelected && !IsCorrect;

        [JsonIgnore]
        public bool IsAnswered => Question?.SelectedIndex != null;

        public void NotifyStateChanged()
        {
            OnPropertyChanged(nameof(IsSelected));
            OnPropertyChanged(nameof(IsCorrect));
            OnPropertyChanged(nameof(IsIncorrect));
            OnPropertyChanged(nameof(IsAnswered));
        }
    }
}
