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

        private bool IsInCodeBlock()
        {
            if (RichNotesTextBox == null) return false;
            var caret = RichNotesTextBox.CaretPosition;
            var paragraph = caret.Paragraph;
            return paragraph != null && paragraph.FontFamily != null && paragraph.FontFamily.Source == "Consolas";
        }

        private void RichNotesTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (IsInCodeBlock())
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    var caret = RichNotesTextBox.CaretPosition;
                    var currentParagraph = caret.Paragraph;
                    if (currentParagraph != null)
                    {
                        e.Handled = true;

                        var start = currentParagraph.ContentStart;
                        var range = new TextRange(start, caret);
                        string textBeforeCaret = range.Text;
                        
                        string[] lines = textBeforeCaret.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                        string currentLineText = lines.Length > 0 ? lines[lines.Length - 1] : "";

                        // Auto-indentation
                        int leadingWhitespaceCount = 0;
                        for (int i = 0; i < currentLineText.Length; i++)
                        {
                            if (currentLineText[i] == ' ' || currentLineText[i] == '\t')
                            {
                                leadingWhitespaceCount++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        string leadingWhitespace = currentLineText.Substring(0, leadingWhitespaceCount);

                        // Execute EnterLineBreak command to insert a line break and move caret to the new line
                        System.Windows.Documents.EditingCommands.EnterLineBreak.Execute(null, RichNotesTextBox);

                        // If there is leading whitespace, insert it at the new caret position
                        if (!string.IsNullOrEmpty(leadingWhitespace))
                        {
                            RichNotesTextBox.Selection.Text = leadingWhitespace;
                            RichNotesTextBox.CaretPosition = RichNotesTextBox.Selection.End;
                        }
                    }
                }
                else if (e.Key == System.Windows.Input.Key.Tab)
                {
                    e.Handled = true;
                    if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) == System.Windows.Input.ModifierKeys.Shift)
                    {
                        // Shift+Tab: Reverse Indentation (remove up to 4 spaces from the start of the current line)
                        var caret = RichNotesTextBox.CaretPosition;
                        var currentParagraph = caret.Paragraph;
                        if (currentParagraph != null)
                        {
                            var start = currentParagraph.ContentStart;
                            var range = new TextRange(start, caret);
                            string textBeforeCaret = range.Text;
                            
                            string[] lines = textBeforeCaret.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                            string currentLineText = lines.Length > 0 ? lines[lines.Length - 1] : "";

                            int spacesToRemove = 0;
                            while (spacesToRemove < 4 && spacesToRemove < currentLineText.Length && currentLineText[spacesToRemove] == ' ')
                            {
                                spacesToRemove++;
                            }
                            if (spacesToRemove == 0 && currentLineText.Length > 0 && currentLineText[0] == '\t')
                            {
                                spacesToRemove = 1;
                            }

                            if (spacesToRemove > 0)
                            {
                                TextPointer lineStart = caret.GetPositionAtOffset(-currentLineText.Length, LogicalDirection.Forward);
                                if (lineStart != null)
                                {
                                    TextPointer removeEnd = lineStart.GetPositionAtOffset(spacesToRemove, LogicalDirection.Forward);
                                    if (removeEnd != null)
                                    {
                                        var rangeToRemove = new TextRange(lineStart, removeEnd);
                                        rangeToRemove.Text = "";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Tab: Standard Indentation (insert 4 spaces)
                        var selection = RichNotesTextBox.Selection;
                        if (selection != null)
                        {
                            selection.Text = "    "; // 4 spaces
                            RichNotesTextBox.CaretPosition = selection.End;
                        }
                    }
                }
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
                        paragraph.ClearValue(TextElement.ForegroundProperty);
                        paragraph.ClearValue(TextElement.FontSizeProperty);
                    }
                    else
                    {
                        // Apply code block styling
                        paragraph.FontFamily = new FontFamily("Consolas");
                        paragraph.Background = new SolidColorBrush(Color.FromRgb(24, 25, 35)); // Dark slate background matching theme
                        paragraph.Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)); // Light primary text
                        paragraph.FontSize = 13;
                        paragraph.Padding = new Thickness(12, 10, 12, 10);
                        paragraph.BorderBrush = (Brush)FindResource("BrushBorder");
                        paragraph.BorderThickness = new Thickness(1);

                        // Ensure there is always a normal line (paragraph) below the code block
                        if (paragraph.NextBlock == null)
                        {
                            var nextParagraph = new Paragraph();
                            nextParagraph.Margin = new Thickness(0, 0, 0, 6);
                            nextParagraph.FontFamily = new FontFamily("Segoe UI, Roboto, Helvetica");
                            nextParagraph.Background = Brushes.Transparent;
                            nextParagraph.Padding = new Thickness(0);
                            nextParagraph.BorderThickness = new Thickness(0);
                            nextParagraph.Inlines.Add(new Run(""));

                            paragraph.SiblingBlocks.InsertAfter(paragraph, nextParagraph);
                        }
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
