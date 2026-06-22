using System;
using System.Windows.Media;
using StudyManager.Models;

namespace StudyManager.ViewModels
{
    public class SearchResultViewModel
    {
        public string Type { get; set; } = string.Empty; // "Study", "Topic", "Theme", "Note", "AiExplanation"
        public string DisplayTitle { get; set; } = string.Empty;
        public string DisplayPath { get; set; } = string.Empty;
        public string DisplaySnippet { get; set; } = string.Empty;
        public bool HasSnippet => !string.IsNullOrEmpty(DisplaySnippet);

        public Study Study { get; set; } = null!;
        public StudyTopic? Topic { get; set; }
        public StudyTheme? Theme { get; set; }

        public string TypeLabel => Type switch
        {
            "Study" => "Estudo",
            "Topic" => "Tópico",
            "Theme" => "Tema",
            "Note" => "Anotação",
            "AiExplanation" => "Explicação IA",
            _ => Type
        };

        public Brush TypeBrush
        {
            get
            {
                var brush = Type switch
                {
                    "Study" => new SolidColorBrush(Color.FromRgb(59, 130, 246)), // Blue (#3B82F6)
                    "Topic" => new SolidColorBrush(Color.FromRgb(139, 92, 246)), // Violet (#8B5CF6)
                    "Theme" => new SolidColorBrush(Color.FromRgb(99, 102, 241)), // Indigo (#6366F1)
                    "Note" => new SolidColorBrush(Color.FromRgb(16, 185, 129)), // Emerald/Green (#10B981)
                    "AiExplanation" => new SolidColorBrush(Color.FromRgb(245, 158, 11)), // Amber/Orange (#F59E0B)
                    _ => Brushes.Gray
                };
                if (brush.CanFreeze)
                {
                    brush.Freeze();
                }
                return brush;
            }
        }
    }
}
