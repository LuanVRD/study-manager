using System;
using System.Text.Json.Serialization;

namespace StudyManager.Models
{
    public class QuestionOption
    {
        [JsonIgnore]
        public ThemeQuestion Question { get; set; } = null!;
        
        public string Text { get; set; } = string.Empty;
        public int Index { get; set; }
    }
}
