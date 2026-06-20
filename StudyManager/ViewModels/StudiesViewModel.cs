using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using StudyManager.Models;
using StudyManager.Views.Dialogs;

namespace StudyManager.ViewModels
{
    public class StudiesViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;

        public ObservableCollection<Study> Studies { get; }

        public ICommand AddStudyCommand { get; }
        public ICommand OpenStudyCommand { get; }
        public ICommand EditStudyCommand { get; }
        public ICommand DeleteStudyCommand { get; }
        public ICommand SyncStudiesCommand { get; }

        public StudiesViewModel(MainViewModel main)
        {
            _main = main;
            // Load studies into observable collection for UI data binding
            Studies = new ObservableCollection<Study>(_main.AppData.Studies);

            AddStudyCommand = new RelayCommand(AddStudy);
            OpenStudyCommand = new RelayCommand(OpenStudy);
            EditStudyCommand = new RelayCommand(EditStudy);
            DeleteStudyCommand = new RelayCommand(DeleteStudy);
            SyncStudiesCommand = new RelayCommand(SyncStudies);
        }

        private void AddStudy()
        {
            // Create and open the dialog
            var dialog = new StudyDialog();
            // Center dialog relative to app window
            if (Application.Current.MainWindow != null)
            {
                dialog.Owner = Application.Current.MainWindow;
            }

            if (dialog.ShowDialog() == true)
            {
                var newStudy = dialog.ResultStudy;
                if (newStudy != null)
                {
                    // Copy image if selected
                    if (!string.IsNullOrEmpty(dialog.SelectedImagePath))
                    {
                        string? localPath = _main.ImageService.CopyImageToLocal(dialog.SelectedImagePath, newStudy.Id);
                        newStudy.ImagePath = localPath;
                    }

                    _main.AppData.Studies.Add(newStudy);
                    Studies.Add(newStudy);
                    _main.SaveData();
                }
            }
        }

        private void OpenStudy(object? parameter)
        {
            if (parameter is Study study)
            {
                _main.NavigateToStudyDetails(study);
            }
        }

        private void EditStudy(object? parameter)
        {
            if (parameter is Study study)
            {
                var dialog = new StudyDialog(study);
                if (Application.Current.MainWindow != null)
                {
                    dialog.Owner = Application.Current.MainWindow;
                }

                if (dialog.ShowDialog() == true)
                {
                    // Copy new image if selected
                    if (!string.IsNullOrEmpty(dialog.SelectedImagePath))
                    {
                        // Delete old image file if it exists
                        if (!string.IsNullOrEmpty(study.ImagePath))
                        {
                            try
                            {
                                string oldAbsPath = _main.ImageService.GetAbsolutePath(study.ImagePath);
                                if (System.IO.File.Exists(oldAbsPath))
                                {
                                    System.IO.File.Delete(oldAbsPath);
                                }
                            }
                            catch
                            {
                                // Ignore cleanup errors
                            }
                        }

                        string? localPath = _main.ImageService.CopyImageToLocal(dialog.SelectedImagePath, study.Id);
                        study.ImagePath = localPath;
                    }

                    // Properties are automatically updated and notified by Study Dialog save event
                    _main.SaveData();
                }
            }
        }

        private void DeleteStudy(object? parameter)
        {
            if (parameter is Study study)
            {
                var result = MessageBox.Show(
                    $"Deseja realmente excluir o estudo \"{study.Name}\"?\n\nTodos os tópicos, temas, links e anotações relacionados também serão excluídos.",
                    "Confirmar Exclusão",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Delete image file off disk if exists
                    if (!string.IsNullOrEmpty(study.ImagePath))
                    {
                        try
                        {
                            string absPath = _main.ImageService.GetAbsolutePath(study.ImagePath);
                            if (System.IO.File.Exists(absPath))
                            {
                                System.IO.File.Delete(absPath);
                            }
                        }
                        catch
                        {
                            // Ignore cleanup errors
                        }
                    }

                    _main.AppData.Studies.Remove(study);
                    Studies.Remove(study);
                    _main.SaveData();
                }
            }
        }

        private void SyncStudies()
        {
            _main.AppData.Studies.Clear();
            foreach (var study in Studies)
            {
                _main.AppData.Studies.Add(study);
            }
            _main.SaveData();
        }
    }
}
