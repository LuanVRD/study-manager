using System;
using System.Collections.Generic;

namespace StudyManager.Models
{
    public class StudyTopic
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public List<StudyTheme> Themes { get; set; } = new List<StudyTheme>();
    }
}
