using System;
using System.IO;

namespace StudyManager.Services
{
    public class ImageService
    {
        private readonly string _imagesDirectory;

        public ImageService(string dataDirectory)
        {
            _imagesDirectory = Path.Combine(dataDirectory, "Images");
            if (!Directory.Exists(_imagesDirectory))
            {
                Directory.CreateDirectory(_imagesDirectory);
            }
        }

        /// <summary>
        /// Copies the source image to the local app directory using a unique name based on the study ID.
        /// Returns the relative path to be saved in JSON (e.g. "Data/Images/guid.ext"), or null if no file copied.
        /// </summary>
        public string? CopyImageToLocal(string sourceFilePath, Guid studyId)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
            {
                return null;
            }

            try
            {
                string extension = Path.GetExtension(sourceFilePath);
                string uniqueFileName = $"{studyId}{extension}";
                string destinationPath = Path.Combine(_imagesDirectory, uniqueFileName);

                File.Copy(sourceFilePath, destinationPath, true);

                // Return a path relative to the app base directory
                return Path.Combine("Data", "Images", uniqueFileName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the absolute path from a relative path saved in the JSON.
        /// </summary>
        public string GetAbsolutePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return string.Empty;
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        }
    }
}
