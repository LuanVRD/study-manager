using System.Collections.Generic;

namespace StudyManager.Models
{
    public class AppData
    {
        public List<Study> Studies { get; set; } = new List<Study>();
        public string GeminiApiKey { get; set; } = string.Empty;
        public string GroqApiKey { get; set; } = string.Empty;
    }
}
