using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StudyManager.Models
{
    public class Study
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
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
    }
}
