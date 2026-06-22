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
            set
            {
                if (_currentViewModel is ThemeDetailsViewModel oldThemeVm)
                {
                    oldThemeVm.SavePendingChanges();
                }
                SetProperty(ref _currentViewModel, value);
            }
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

        public void NavigateToThemeDetails(Study study, StudyTopic topic, StudyTheme theme, string? sectionToExpand = null)
        {
            var vm = new ThemeDetailsViewModel(this, study, topic, theme);
            if (sectionToExpand == "Notes")
            {
                vm.IsNotesExpanded = true;
            }
            else if (sectionToExpand == "AiExplanation")
            {
                vm.IsAiExplanationExpanded = true;
            }
            CurrentViewModel = vm;
        }

        public void SaveData()
        {
            StorageService.Save(AppData);
        }
    }
}
