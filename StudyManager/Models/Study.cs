using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using StudyManager.ViewModels;

namespace StudyManager.Models
{
    public class Study : ViewModelBase
    {
        private string _name = string.Empty;
        private string? _imagePath;
        private DateTime _updatedAt = DateTime.Now;

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    OnPropertyChanged(nameof(ProgressPercentage)); // Might change if topics change
                }
            }
        }

        public string? ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set => SetProperty(ref _updatedAt, value);
        }

        public List<StudyTopic> Topics { get; set; } = new List<StudyTopic>();

        [JsonIgnore]
        public double ProgressPercentage
        {
            get
            {
                int totalThemes = 0;
                int completedThemes = 0;

                if (Topics != null)
                {
                    foreach (var topic in Topics)
                    {
                        if (topic.Themes != null)
                        {
                            foreach (var theme in topic.Themes)
                            {
                                totalThemes++;
                                if (theme.IsCompleted)
                                {
                                    completedThemes++;
                                }
                            }
                        }
                    }
                }

                if (totalThemes == 0) return 0.0;
                return Math.Round((double)completedThemes / totalThemes * 100.0, 1);
            }
        }

        public void NotifyProgressChanged()
        {
            OnPropertyChanged(nameof(ProgressPercentage));
        }
    }
}
