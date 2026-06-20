using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace StudyManager.Views
{
    public static class RichTextBoxHelper
    {
        public static readonly DependencyProperty DocumentXamlProperty =
            DependencyProperty.RegisterAttached(
                "DocumentXaml",
                typeof(string),
                typeof(RichTextBoxHelper),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnDocumentXamlChanged));

        public static string GetDocumentXaml(DependencyObject obj)
        {
            return (string)obj.GetValue(DocumentXamlProperty);
        }

        public static void SetDocumentXaml(DependencyObject obj, string value)
        {
            obj.SetValue(DocumentXamlProperty, value);
        }

        private static bool _isUpdating = false;

        private static void OnDocumentXamlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (_isUpdating) return;
            if (d is RichTextBox rtb)
            {
                var xaml = e.NewValue as string;
                rtb.TextChanged -= RichTextBox_TextChanged;

                if (string.IsNullOrWhiteSpace(xaml))
                {
                    rtb.Document = new FlowDocument();
                }
                else
                {
                    try
                    {
                        string trimmed = xaml.TrimStart();
                        if (trimmed.StartsWith("<FlowDocument"))
                        {
                            var context = new ParserContext();
                            context.BaseUri = new Uri(AppDomain.CurrentDomain.BaseDirectory, UriKind.Absolute);

                            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xaml)))
                            {
                                var doc = (FlowDocument)XamlReader.Load(stream, context);
                                rtb.Document = doc;
                            }
                        }
                        else
                        {
                            // Backward compatibility: Load plain text notes
                            LoadPlainText(rtb, xaml);
                        }
                    }
                    catch (Exception)
                    {
                        // Fallback: Load plain text notes if XAML parsing fails
                        LoadPlainText(rtb, xaml);
                    }
                }

                rtb.TextChanged += RichTextBox_TextChanged;
            }
        }

        private static void LoadPlainText(RichTextBox rtb, string text)
        {
            var doc = new FlowDocument();
            var paragraph = new Paragraph();
            paragraph.Margin = new Thickness(0, 0, 0, 6);
            paragraph.Inlines.Add(new Run(text));
            doc.Blocks.Add(paragraph);
            rtb.Document = doc;
        }

        private static void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is RichTextBox rtb)
            {
                _isUpdating = true;
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        XamlWriter.Save(rtb.Document, stream);
                        string xaml = Encoding.UTF8.GetString(stream.ToArray());

                        // Make paths portable: replace absolute path prefixes of the local directory with relative paths.
                        // For example, file:///C:/Projects/.../Data/Images/xxx.png gets replaced with Data/Images/xxx.png.
                        string baseDir = AppDomain.CurrentDomain.BaseDirectory.Replace('\\', '/');
                        if (!baseDir.EndsWith("/")) baseDir += "/";

                        string absolutePrefix = $"file:///{baseDir}";
                        xaml = xaml.Replace(absolutePrefix, "");
                        xaml = xaml.Replace(AppDomain.CurrentDomain.BaseDirectory, "");

                        SetDocumentXaml(rtb, xaml);
                    }
                }
                catch (Exception)
                {
                    // Ignore or handle silently
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }
    }
}
