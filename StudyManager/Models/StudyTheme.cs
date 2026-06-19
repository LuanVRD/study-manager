using System;
using System.Collections.Generic;

namespace StudyManager.Models
{
    public class StudyTheme
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<StudyLink> Links { get; set; } = new List<StudyLink>();
    }
}
