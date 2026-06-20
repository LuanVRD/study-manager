using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StudyManager.Views
{
    public partial class ThemeDetailsView : UserControl
    {
        public ThemeDetailsView()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(RichNotesTextBox, OnPaste);
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            try
            {
                // 1. Check for file drop (e.g. copied image file from Explorer)
                if (e.DataObject.GetDataPresent(DataFormats.FileDrop))
                {
                    var files = e.DataObject.GetData(DataFormats.FileDrop) as string[];
                    if (files != null && files.Length > 0)
                    {
                        string filePath = files[0];
                        string ext = Path.GetExtension(filePath).ToLower();
                        if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".bmp")
                        {
                            e.CancelCommand();
                            CopyAndInsertImage(filePath);
                            return;
                        }
                    }
                }

                // 2. Check for bitmap data (e.g. printscreen/clipboard image)
                if (e.DataObject.GetDataPresent(DataFormats.Bitmap))
                {
                    e.CancelCommand();
                    var image = Clipboard.GetImage();
                    if (image != null)
                    {
                        string uniqueFileName = $"{Guid.NewGuid()}.png";
                        string imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Images");
                        if (!Directory.Exists(imagesDir))
                        {
                            Directory.CreateDirectory(imagesDir);
                        }
                        string destPath = Path.Combine(imagesDir, uniqueFileName);

                        using (var fileStream = new FileStream(destPath, FileMode.Create))
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(image));
                            encoder.Save(fileStream);
                        }

                        InsertImageIntoRichTextBox(destPath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao colar imagem: {ex.Message}", "Erro de Colagem", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyAndInsertImage(string sourcePath)
        {
            try
            {
                string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(sourcePath)}";
                string imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Images");
                if (!Directory.Exists(imagesDir))
                {
                    Directory.CreateDirectory(imagesDir);
                }
                string destPath = Path.Combine(imagesDir, uniqueFileName);

                File.Copy(sourcePath, destPath, true);
                InsertImageIntoRichTextBox(destPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao copiar imagem: {ex.Message}", "Erro de Cópia", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InsertImageIntoRichTextBox(string imagePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            var img = new Image
            {
                Source = bitmap,
                MaxWidth = 500,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(0, 8, 0, 8)
            };

            var container = new InlineUIContainer(img, RichNotesTextBox.CaretPosition);
            RichNotesTextBox.CaretPosition = container.ElementEnd;
            RichNotesTextBox.Focus();
        }

        private void Heading1_Click(object sender, RoutedEventArgs e)
        {
            ApplyHeading(20, FontWeights.Bold, (Brush)FindResource("BrushPrimary"));
        }

        private void Heading2_Click(object sender, RoutedEventArgs e)
        {
            ApplyHeading(16, FontWeights.Bold, (Brush)FindResource("BrushTextPrimary"));
        }

        private void NormalText_Click(object sender, RoutedEventArgs e)
        {
            ApplyHeading(14, FontWeights.Normal, (Brush)FindResource("BrushTextSecondary"));
        }

        private void ApplyHeading(double fontSize, FontWeight fontWeight, Brush foreground)
        {
            var selection = RichNotesTextBox.Selection;
            if (selection != null)
            {
                if (selection.IsEmpty)
                {
                    var paragraph = selection.Start.Paragraph;
                    if (paragraph != null)
                    {
                        paragraph.FontSize = fontSize;
                        paragraph.FontWeight = fontWeight;
                        paragraph.Foreground = foreground;
                    }
                }
                else
                {
                    selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
                    selection.ApplyPropertyValue(TextElement.FontWeightProperty, fontWeight);
                    selection.ApplyPropertyValue(TextElement.ForegroundProperty, foreground);
                }
                RichNotesTextBox.Focus();
            }
        }

        private void CodeBlock_Click(object sender, RoutedEventArgs e)
        {
            var selection = RichNotesTextBox.Selection;
            if (selection != null)
            {
                var paragraph = selection.Start.Paragraph;
                if (paragraph != null)
                {
                    // Check if it's already styled as code block
                    if (paragraph.FontFamily != null && paragraph.FontFamily.Source == "Consolas")
                    {
                        // Reset to normal text
                        paragraph.FontFamily = new FontFamily("Segoe UI, Roboto, Helvetica");
                        paragraph.Background = Brushes.Transparent;
                        paragraph.Padding = new Thickness(0);
                        paragraph.BorderThickness = new Thickness(0);
                    }
                    else
                    {
                        // Apply code block styling
                        paragraph.FontFamily = new FontFamily("Consolas");
                        paragraph.Background = new SolidColorBrush(Color.FromRgb(24, 25, 35)); // Dark slate background matching theme
                        paragraph.Padding = new Thickness(10, 8, 10, 8);
                        paragraph.BorderBrush = (Brush)FindResource("BrushBorder");
                        paragraph.BorderThickness = new Thickness(1);
                    }
                }
                RichNotesTextBox.Focus();
            }
        }

        private void InsertImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imagens (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|Todos os arquivos (*.*)|*.*",
                Title = "Selecionar Imagem"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                CopyAndInsertImage(openFileDialog.FileName);
            }
        }
    }
}
