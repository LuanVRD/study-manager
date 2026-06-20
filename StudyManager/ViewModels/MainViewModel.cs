using System;
using StudyManager.Models;
using StudyManager.Services;

namespace StudyManager.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase? _currentViewModel;
        
        public JsonStorageService StorageService { get; }
        public ImageService ImageService { get; }
        public AppData AppData { get; }

        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public MainViewModel()
        {
            StorageService = new JsonStorageService();
            ImageService = new ImageService(StorageService.GetDataDirectory());
            AppData = StorageService.Load();

            NavigateToStudies();
        }

        public void NavigateToStudies()
        {
            CurrentViewModel = new StudiesViewModel(this);
        }

        public void NavigateToStudyDetails(Study study)
        {
            CurrentViewModel = new StudyDetailsViewModel(this, study);
        }

        public void NavigateToThemeDetails(Study study, StudyTopic topic, StudyTheme theme)
        {
            CurrentViewModel = new ThemeDetailsViewModel(this, study, topic, theme);
        }

        public void SaveData()
        {
            StorageService.Save(AppData);
        }
    }
}
