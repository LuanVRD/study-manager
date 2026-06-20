using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using StudyManager.Models;

namespace StudyManager.Services
{
    public class ImportResult
    {
        public int StudiesCreated { get; set; }
        public int TopicsCreated { get; set; }
        public int ThemesAdded { get; set; }
        public int ThemesIgnored { get; set; }
        public int InvalidLines { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class CsvImportService
    {
        public ImportResult Import(string filePath, Study study)
        {
            var result = new ImportResult();

            if (!File.Exists(filePath))
            {
                result.Success = false;
                result.ErrorMessage = "O arquivo selecionado não existe.";
                return result;
            }

            try
            {
                var lines = File.ReadLines(filePath, Encoding.UTF8).ToList();
                if (lines.Count <= 1)
                {
                    result.Success = false;
                    result.ErrorMessage = "O arquivo está vazio ou contém apenas o cabeçalho.";
                    return result;
                }

                // 1. Detect separator based on header (first line)
                string header = lines[0];
                char separator = ',';
                if (header.Contains(';'))
                {
                    separator = ';';
                }

                // 2. Parse data lines (skip header)
                for (int i = 1; i < lines.Count; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue; // Ignore empty lines
                    }

                    string[] parts = ParseCsvLine(line, separator);
                    if (parts.Length != 2 || 
                        string.IsNullOrWhiteSpace(parts[0]) || 
                        string.IsNullOrWhiteSpace(parts[1]))
                    {
                        result.InvalidLines++;
                        continue; // Skip invalid line
                    }

                    string topicName = parts[0].Trim();
                    string themeName = parts[1].Trim();

                    // Find topic by name (case-insensitive)
                    var topic = study.Topics.FirstOrDefault(t => 
                        t.Name.Equals(topicName, StringComparison.OrdinalIgnoreCase));

                    if (topic == null)
                    {
                        topic = new StudyTopic { Name = topicName };
                        study.Topics.Add(topic);
                        result.TopicsCreated++;
                    }

                    // Check duplicate themes in this topic (case-insensitive)
                    bool themeExists = topic.Themes.Any(t => 
                        t.Name.Equals(themeName, StringComparison.OrdinalIgnoreCase));

                    if (themeExists)
                    {
                        result.ThemesIgnored++;
                    }
                    else
                    {
                        var theme = new StudyTheme
                        {
                            Name = themeName,
                            IsCompleted = false
                        };
                        topic.Themes.Add(theme);
                        result.ThemesAdded++;
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Erro ao ler o arquivo CSV: {ex.Message}";
            }

            return result;
        }

        public ImportResult ImportAll(string filePath, List<Study> studies)
        {
            var result = new ImportResult();

            if (!File.Exists(filePath))
            {
                result.Success = false;
                result.ErrorMessage = "O arquivo selecionado não existe.";
                return result;
            }

            try
            {
                var lines = File.ReadLines(filePath, Encoding.UTF8).ToList();
                if (lines.Count <= 1)
                {
                    result.Success = false;
                    result.ErrorMessage = "O arquivo está vazio ou contém apenas o cabeçalho.";
                    return result;
                }

                // 1. Detect separator based on header (first line)
                string header = lines[0];
                char separator = ',';
                if (header.Contains(';'))
                {
                    separator = ';';
                }

                // 2. Parse data lines (skip header)
                for (int i = 1; i < lines.Count; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue; // Ignore empty lines
                    }

                    string[] parts = ParseCsvLine(line, separator);
                    if (parts.Length < 3 || 
                        string.IsNullOrWhiteSpace(parts[0]) || 
                        string.IsNullOrWhiteSpace(parts[1]) || 
                        string.IsNullOrWhiteSpace(parts[2]))
                    {
                        result.InvalidLines++;
                        continue; // Skip invalid line
                    }

                    string studyName = parts[0].Trim();
                    string topicName = parts[1].Trim();
                    string themeName = parts[2].Trim();

                    // Find or create Study (case-insensitive)
                    var study = studies.FirstOrDefault(s => 
                        s.Name.Equals(studyName, StringComparison.OrdinalIgnoreCase));

                    if (study == null)
                    {
                        study = new Study { Name = studyName };
                        studies.Add(study);
                        result.StudiesCreated++;
                    }

                    // Find or create Topic in this Study (case-insensitive)
                    var topic = study.Topics.FirstOrDefault(t => 
                        t.Name.Equals(topicName, StringComparison.OrdinalIgnoreCase));

                    if (topic == null)
                    {
                        topic = new StudyTopic { Name = topicName };
                        study.Topics.Add(topic);
                        result.TopicsCreated++;
                    }

                    // Check duplicate themes in this topic (case-insensitive)
                    bool themeExists = topic.Themes.Any(t => 
                        t.Name.Equals(themeName, StringComparison.OrdinalIgnoreCase));

                    if (themeExists)
                    {
                        result.ThemesIgnored++;
                    }
                    else
                    {
                        var theme = new StudyTheme
                        {
                            Name = themeName,
                            IsCompleted = false
                        };
                        topic.Themes.Add(theme);
                        result.ThemesAdded++;
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Erro ao ler o arquivo CSV: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Splits a CSV line by the separator while respecting fields encapsulated in double quotes.
        /// </summary>
        private string[] ParseCsvLine(string line, char separator)
        {
            var result = new List<string>();
            var currentToken = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == separator && !inQuotes)
                {
                    result.Add(currentToken.ToString().Trim(' ', '"'));
                    currentToken.Clear();
                }
                else
                {
                    currentToken.Append(c);
                }
            }
            result.Add(currentToken.ToString().Trim(' ', '"'));

            return result.ToArray();
        }
    }
}
