using System;
using StudyManager.ViewModels;

namespace StudyManager.Models
{
    public class StudyLink : ViewModelBase
    {
        private string _name = string.Empty;
        private string _url = string.Empty;
        private LinkType _type = LinkType.Article;

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        public LinkType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }
    }
}
