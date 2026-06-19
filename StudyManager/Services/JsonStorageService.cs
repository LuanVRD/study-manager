using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using StudyManager.Models;

namespace StudyManager.Services
{
    public class JsonStorageService
    {
        private readonly string _dataDirectory;
        private readonly string _filePath;
        private readonly string _backupFilePath;
        private readonly string _tempFilePath;
        
        private readonly JsonSerializerOptions _options;

        public JsonStorageService()
        {
            // Base directory is the application execution directory
            _dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            _filePath = Path.Combine(_dataDirectory, "app-data.json");
            _backupFilePath = Path.Combine(_dataDirectory, "app-data.backup.json");
            _tempFilePath = Path.Combine(_dataDirectory, "app-data.tmp.json");

            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public string GetDataDirectory() => _dataDirectory;
        public string GetFilePath() => _filePath;
        public string GetBackupFilePath() => _backupFilePath;

        public AppData Load()
        {
            EnsureDirectoryExists();

            if (!File.Exists(_filePath))
            {
                // Create automatically when it doesn't exist
                var newData = new AppData();
                Save(newData);
                return newData;
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                var data = JsonSerializer.Deserialize<AppData>(json, _options);
                if (data == null)
                {
                    throw new JsonException("Deserialized data is null.");
                }
                return data;
            }
            catch (Exception ex)
            {
                // Principal file is invalid or corrupted
                return HandleCorruptedMainFile(ex.Message);
            }
        }

        private AppData HandleCorruptedMainFile(string originalError)
        {
            if (File.Exists(_backupFilePath))
            {
                try
                {
                    string backupJson = File.ReadAllText(_backupFilePath);
                    var data = JsonSerializer.Deserialize<AppData>(backupJson, _options);
                    if (data != null)
                    {
                        MessageBox.Show(
                            "O arquivo de dados principal estava corrompido ou era inválido. Os dados foram recuperados a partir do backup automático.",
                            "Aviso de Recuperação",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        
                        // Save the backup data back to main file
                        Save(data);
                        return data;
                    }
                }
                catch (Exception backupEx)
                {
                    // Backup is also invalid
                    MessageBox.Show(
                        $"Não foi possível carregar os dados. O arquivo principal e o backup estão corrompidos ou são inválidos.\n\nDetalhes:\nPrincipal: {originalError}\nBackup: {backupEx.Message}\n\nUma estrutura de dados vazia foi inicializada.",
                        "Erro Crítico de Dados",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(
                    $"O arquivo de dados principal estava inválido ou corrompido e nenhum backup foi encontrado.\n\nDetalhes: {originalError}\n\nUma estrutura de dados vazia foi inicializada.",
                    "Erro ao Carregar Dados",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            // Fallback to empty data
            var emptyData = new AppData();
            Save(emptyData);
            return emptyData;
        }

        public void Save(AppData data)
        {
            EnsureDirectoryExists();

            try
            {
                // 1. Serialize data to temporary file
                string json = JsonSerializer.Serialize(data, _options);
                File.WriteAllText(_tempFilePath, json);

                // 2. If main file exists, copy it to backup before replacing it
                if (File.Exists(_filePath))
                {
                    File.Copy(_filePath, _backupFilePath, true);
                }

                // 3. Replace main file with the temporary one
                File.Copy(_tempFilePath, _filePath, true);
                
                // Clean up temp file
                if (File.Exists(_tempFilePath))
                {
                    File.Delete(_tempFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ocorreu um erro ao salvar os dados:\n{ex.Message}",
                    "Erro ao Salvar",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
            
            // Also create the Images directory inside Data
            string imagesDir = Path.Combine(_dataDirectory, "Images");
            if (!Directory.Exists(imagesDir))
            {
                Directory.CreateDirectory(imagesDir);
            }
        }
    }
}
