using System;

namespace StudyManager.Models
{
    public class StudyLink
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public LinkType Type { get; set; } = LinkType.Article;
    }
}
