using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace StudyManager.Services
{
    public class GeminiService
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public async Task<string> ConsultThemeAsync(string studyName, string topicName, string themeName, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("Chave da API do Gemini não configurada.");
            }

            string prompt = $@"
Você é um especialista em tecnologia e desenvolvimento de software. Descreva detalhadamente o tema de estudo '{themeName}', que faz parte do tópico '{topicName}' no contexto do estudo de '{studyName}'.
Forneça explicações aprofundadas, objetivos/funcionalidades do assunto e exemplos práticos de código se aplicável.
Como o leitor já é um desenvolvedor experiente, não explique conceitos extremamente básicos de programação. Foque em definições claras, boas práticas, casos de uso reais e trechos de código limpos e bem estruturados.
Sua resposta deve ser estruturada em Markdown, utilizando títulos com '#' (H1), '##' (H2), '###' (H3), listas com marcadores '*' ou '-' para pontos importantes, negrito com '**' para termos-chave e blocos de código com '```' (especificando a linguagem, como csharp, javascript, typescript, html, css, etc.) para os exemplos práticos.
";

            var requestBody = new JsonObject
            {
                ["contents"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["parts"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["text"] = prompt
                            }
                        }
                    }
                }
            };

            return await SendApiRequestWithFallbackAsync(apiKey, requestBody);
        }

        public async Task<StudyGuideResult> GenerateStudyGuideAsync(string studyName, string topicName, string themeName, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("Chave da API do Gemini não configurada.");
            }

            // Fase 1: Gerar a explicação teórica rica fundamentada na pesquisa do Google em tempo real (2026)
            string explanationPrompt = $@"
Você é um especialista em tecnologia e desenvolvimento de software. Crie uma explicação completa, detalhada e aprofundada sobre o tema '{themeName}', que faz parte do tópico '{topicName}' no contexto do estudo de '{studyName}'.
Você DEVE utilizar a ferramenta de pesquisa do Google (Google Search Grounding) para embasar sua resposta nas práticas, versões e padrões tecnológicos mais recentes e vigentes de 2026.
Foque em definições claras, boas práticas contemporâneas de desenvolvimento de software, casos de uso práticos e trechos de código limpos e bem estruturados.
Como o leitor já é um desenvolvedor experiente, não explique conceitos básicos de programação.
Sua resposta deve ser estruturada em Markdown, utilizando títulos com '#' (H1), '##' (H2), '###' (H3), listas com marcadores '*' ou '-' para pontos importantes, negrito com '**' para termos-chave e blocos de código com '```' (especificando a linguagem, como csharp, javascript, typescript, html, css, etc.) para os exemplos práticos.
";

            var explanationRequestBody = new JsonObject
            {
                ["contents"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["parts"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["text"] = explanationPrompt
                            }
                        }
                    }
                },
                ["tools"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["google_search"] = new JsonObject()
                    }
                }
            };

            string explanationText = await SendApiRequestWithFallbackAsync(apiKey, explanationRequestBody);

            // Fase 2: Gerar os links de busca e as 5 perguntas estruturadas em JSON baseando-se na explicação gerada na Fase 1
            string guidePrompt = $@"
Você é um especialista em tecnologia. Com base na explicação fornecida sobre o tema '{themeName}', gere os links de apoio de busca (vídeo e artigo) e 5 perguntas de fixação para validar o entendimento do assunto.

Você DEVE retornar a resposta estritamente como um objeto JSON válido, sem qualquer tipo de formatação externa (como blocos de markdown ```json ... ```) e sem caracteres extras. O JSON gerado deve seguir exatamente a seguinte estrutura:

{{
  ""videoLink"": {{
    ""name"": ""Título do vídeo relevante no YouTube (ex: 'Aprenda X na prática' ou tutorial oficial)"",
    ""url"": ""Uma URL de busca dinâmica no YouTube baseada no tema (ex: 'https://www.youtube.com/results?search_query=csharp+linq+tutorial' usando '+' para separar as palavras na busca)""
  }},
  ""articleLink"": {{
    ""name"": ""Título da documentação oficial ou artigo de extrema importância"",
    ""url"": ""Uma URL de busca dinâmica no Google ou Microsoft Learn baseada no tema (ex: 'https://www.google.com/search?q=documentacao+oficial+csharp+linq' ou busca oficial do framework)""
  }},
  ""questions"": [
    {{
      ""questionText"": ""Pergunta direta de extrema importância baseada no tema para validar o entendimento."",
      ""options"": [
        ""Opção A"",
        ""Opção B"",
        ""Opção C"",
        ""Opção D"",
        ""Opção E""
      ],
      ""correctIndex"": 0
    }}
  ]
}}

Tema: {themeName}
Explicação de Referência:
{explanationText}

Importante:
1. Retorne EXATAMENTE 5 perguntas objetivas no array de questions. Cada pergunta deve ter EXATAMENTE 5 opções (Opção A, B, C, D, E).
2. O correctIndex deve ser o índice da opção correta (0 a 4).
3. Todas as strings internas devem ter as aspas duplas devidamente escapadas com barra invertida (\u0022 ou \"") para manter o JSON válido.
4. Para os links (videoLink e articleLink), crie URLs de busca dinâmica focadas no tema '{themeName}' para garantir que o usuário acesse materiais sempre atualizados e relevantes.
";

            var guideRequestBody = new JsonObject
            {
                ["contents"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["parts"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["text"] = guidePrompt
                            }
                        }
                    }
                },
                ["generationConfig"] = new JsonObject
                {
                    ["responseMimeType"] = "application/json",
                    ["temperature"] = 0.3,
                    ["maxOutputTokens"] = 4096
                }
            };

            string guideTextResult = await SendApiRequestWithFallbackAsync(apiKey, guideRequestBody);

            try
            {
                var result = JsonSerializer.Deserialize<StudyGuideResult>(guideTextResult, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    throw new Exception("Falha ao desserializar o resultado do guia de estudos.");
                }

                // Injetamos a explicação grounded gerada na Fase 1
                result.Explanation = explanationText;

                return result;
            }
            catch (JsonException ex)
            {
                throw new Exception($"A resposta de links/perguntas da IA não está em um formato JSON válido: {ex.Message}\nResposta crua:\n{guideTextResult}");
            }
        }

        private async Task<string> SendApiRequestWithFallbackAsync(string apiKey, JsonObject requestBody)
        {
            string[] models = { "gemini-2.0-flash", "gemini-2.5-flash", "gemini-3.5-flash", "gemini-2.5-pro", "gemini-2.0-flash-lite" };
            Exception? lastException = null;

            foreach (var model in models)
            {
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
                try
                {
                    return await SendApiRequestAsync(url, requestBody);
                }
                catch (Exception ex) when (
                    ex.Message.Contains("503") || ex.Message.Contains("ServiceUnavailable") || 
                    ex.Message.Contains("429") || ex.Message.Contains("TooManyRequests") || 
                    ex.Message.Contains("UNAVAILABLE") || ex.Message.Contains("404") || 
                    ex.Message.Contains("NotFound") || ex.Message.Contains("NOT_FOUND") ||
                    ex.Message.Contains("400") || ex.Message.Contains("BadRequest")
                )
                {
                    lastException = ex;
                    System.Diagnostics.Debug.WriteLine($"Modelo {model} falhou com erro: {ex.Message}. Tentando modelo fallback...");

                    // Se falhou por limite de requisições (429), aguarda 2 segundos antes de tentar o próximo modelo
                    // para evitar estourar o limite de requisições por segundo (RPS) do gateway da API
                    if (ex.Message.Contains("429") || ex.Message.Contains("TooManyRequests") || ex.Message.Contains("RESOURCE_EXHAUSTED"))
                    {
                        await Task.Delay(2000);
                    }
                }
            }

            if (lastException != null)
            {
                string cleanMessage = lastException.Message;
                if (lastException.Message.Contains("429") || lastException.Message.Contains("TooManyRequests") || lastException.Message.Contains("RESOURCE_EXHAUSTED"))
                {
                    cleanMessage = "Limite de cota de requisições excedido na API gratuita do Gemini. Por favor, aguarde cerca de 30 segundos antes de tentar novamente.";
                }
                else if (lastException.Message.Contains("404") || lastException.Message.Contains("NotFound"))
                {
                    cleanMessage = "Os modelos do Gemini configurados não foram encontrados ou estão indisponíveis temporariamente.";
                }
                else if (lastException.Message.Contains("400") || lastException.Message.Contains("BadRequest"))
                {
                    cleanMessage = "Requisição inválida enviada à API do Gemini. Por favor, certifique-se de que o tema ou os parâmetros de busca não contêm caracteres inválidos.";
                }
                
                throw new Exception(cleanMessage, lastException);
            }

            throw new Exception("Falha ao se comunicar com a API do Gemini em todos os modelos tentados.");
        }

        private async Task<string> SendApiRequestAsync(string url, JsonObject requestBody)
        {
            var content = new StringContent(requestBody.ToJsonString(), Encoding.UTF8, "application/json");
            HttpResponseMessage response = null!;
            int maxRetries = 3;
            int delayMs = 1500;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                response = await HttpClient.PostAsync(url, content);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                    response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    if (attempt == maxRetries) break;
                    await Task.Delay(delayMs * attempt);
                    continue;
                }

                break;
            }

            if (!response.IsSuccessStatusCode)
            {
                string errorMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro na API do Gemini: {response.StatusCode} - {errorMsg}");
            }

            string responseString = await response.Content.ReadAsStringAsync();
            JsonNode? jsonResponse = JsonNode.Parse(responseString);
            string textResult = jsonResponse?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(textResult))
            {
                throw new Exception("Não foi possível obter resposta da IA.");
            }

            return textResult;
        }

        public static string AppendMarkdownToXaml(string currentXaml, string markdownText)
        {
            // 1. Convert current XAML to FlowDocument
            FlowDocument doc = XamlToFlowDocument(currentXaml);

            // 2. Parse markdown line by line and append to doc
            string[] lines = markdownText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            bool inCodeBlock = false;
            StringBuilder codeBuilder = new StringBuilder();

            // Colors matching the UI theme (from Theme.xaml)
            var brushPrimary = new SolidColorBrush(Color.FromRgb(99, 102, 241));     // #6366f1
            var brushTextPrimary = new SolidColorBrush(Color.FromRgb(248, 250, 252)); // #f8fafc
            var brushTextSecondary = new SolidColorBrush(Color.FromRgb(148, 163, 184)); // #94a3b8
            var brushBorder = new SolidColorBrush(Color.FromRgb(42, 44, 63));         // #2a2c3f
            var brushCodeBg = new SolidColorBrush(Color.FromRgb(24, 25, 35));         // #181923
            var brushAccent = new SolidColorBrush(Color.FromRgb(139, 92, 246));       // #8b5cf6

            // Add separator if document already contains text
            if (doc.Blocks.Count > 0 && !string.IsNullOrWhiteSpace(new TextRange(doc.ContentStart, doc.ContentEnd).Text.Trim()))
            {
                var separator = new Paragraph();
                separator.Margin = new Thickness(0, 20, 0, 20);
                separator.Inlines.Add(new Run("━" + new string('━', 40)) { Foreground = brushBorder });
                doc.Blocks.Add(separator);
            }

            // Add AI reference header
            var aiHeader = new Paragraph();
            aiHeader.Margin = new Thickness(0, 0, 0, 12);
            aiHeader.Inlines.Add(new Run("✨ Referência de IA: " + DateTime.Now.ToString("g")) { 
                FontSize = 14, 
                FontWeight = FontWeights.Bold, 
                Foreground = brushAccent
            });
            doc.Blocks.Add(aiHeader);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.Trim().StartsWith("```"))
                {
                    if (inCodeBlock)
                    {
                        // End of code block
                        var codePara = new Paragraph();
                        codePara.FontFamily = new FontFamily("Consolas");
                        codePara.Background = brushCodeBg;
                        codePara.Foreground = brushTextPrimary;
                        codePara.FontSize = 13;
                        codePara.Padding = new Thickness(12, 10, 12, 10);
                        codePara.BorderBrush = brushBorder;
                        codePara.BorderThickness = new Thickness(1);
                        codePara.Margin = new Thickness(0, 0, 0, 8);

                        string codeText = codeBuilder.ToString().TrimEnd('\r', '\n');
                        string[] codeLines = codeText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                        for (int j = 0; j < codeLines.Length; j++)
                        {
                            if (j > 0)
                            {
                                codePara.Inlines.Add(new LineBreak());
                            }
                            codePara.Inlines.Add(new Run(codeLines[j]));
                        }

                        doc.Blocks.Add(codePara);
                        codeBuilder.Clear();
                        inCodeBlock = false;

                        // Ensure a normal empty paragraph below the code block
                        var normalPara = new Paragraph();
                        normalPara.Margin = new Thickness(0, 0, 0, 6);
                        normalPara.FontFamily = new FontFamily("Segoe UI, Roboto, Helvetica");
                        normalPara.Inlines.Add(new Run(""));
                        doc.Blocks.Add(normalPara);
                    }
                    else
                    {
                        // Start of code block
                        inCodeBlock = true;
                    }
                    continue;
                }

                if (inCodeBlock)
                {
                    codeBuilder.AppendLine(line);
                    continue;
                }

                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    continue;
                }

                // Check for headers
                if (trimmed.StartsWith("#"))
                {
                    int hashCount = 0;
                    while (hashCount < trimmed.Length && trimmed[hashCount] == '#')
                    {
                        hashCount++;
                    }

                    if (hashCount > 0 && hashCount < trimmed.Length && trimmed[hashCount] == ' ')
                    {
                        string headerText = trimmed.Substring(hashCount + 1).Trim();
                        var headerPara = new Paragraph();
                        headerPara.Margin = new Thickness(0, 16, 0, 8);

                        double fontSize = 14;
                        FontWeight fontWeight = FontWeights.Bold;
                        Brush foreground = brushTextPrimary;

                        if (hashCount == 1)
                        {
                            fontSize = 20;
                            foreground = brushPrimary;
                        }
                        else if (hashCount == 2)
                        {
                            fontSize = 16;
                            foreground = brushTextPrimary;
                        }
                        else if (hashCount == 3)
                        {
                            fontSize = 14;
                            foreground = brushTextPrimary;
                        }

                        headerPara.FontSize = fontSize;
                        headerPara.FontWeight = fontWeight;
                        headerPara.Foreground = foreground;

                        ParseInlineFormatting(headerPara, headerText, brushTextPrimary, brushCodeBg, brushBorder);
                        doc.Blocks.Add(headerPara);
                        continue;
                    }
                }

                // Check for list items
                if (trimmed.StartsWith("* ") || trimmed.StartsWith("- "))
                {
                    string listText = trimmed.Substring(2).Trim();
                    var listPara = new Paragraph();
                    listPara.Margin = new Thickness(16, 0, 0, 6);

                    listPara.Inlines.Add(new Run("• ") { Foreground = brushPrimary, FontWeight = FontWeights.Bold });
                    ParseInlineFormatting(listPara, listText, brushTextSecondary, brushCodeBg, brushBorder);
                    doc.Blocks.Add(listPara);
                    continue;
                }

                // Normal paragraph
                var para = new Paragraph();
                para.Margin = new Thickness(0, 0, 0, 6);
                para.Foreground = brushTextSecondary;
                para.FontSize = 14;
                ParseInlineFormatting(para, line, brushTextSecondary, brushCodeBg, brushBorder);
                doc.Blocks.Add(para);
            }

            // 3. Convert back to XAML
            return FlowDocumentToXaml(doc);
        }

        private static void ParseInlineFormatting(Paragraph paragraph, string text, Brush normalBrush, Brush codeBg, Brush borderBrush)
        {
            int i = 0;
            while (i < text.Length)
            {
                // Check for bold: **
                if (i + 1 < text.Length && text[i] == '*' && text[i + 1] == '*')
                {
                    int endIdx = text.IndexOf("**", i + 2);
                    if (endIdx != -1)
                    {
                        string boldText = text.Substring(i + 2, endIdx - (i + 2));
                        paragraph.Inlines.Add(new Run(boldText) { FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)) });
                        i = endIdx + 2;
                        continue;
                    }
                }

                // Check for inline code: `
                if (text[i] == '`')
                {
                    int endIdx = text.IndexOf('`', i + 1);
                    if (endIdx != -1)
                    {
                        string codeText = text.Substring(i + 1, endIdx - (i + 1));
                        paragraph.Inlines.Add(new Run(codeText) {
                            FontFamily = new FontFamily("Consolas"),
                            Background = codeBg,
                            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                            FontSize = 13
                        });
                        i = endIdx + 1;
                        continue;
                    }
                }

                // Read plain text until next special character
                int nextBold = text.IndexOf("**", i);
                int nextCode = text.IndexOf('`', i);

                int nextSpecial = -1;
                if (nextBold != -1 && nextCode != -1)
                    nextSpecial = Math.Min(nextBold, nextCode);
                else if (nextBold != -1)
                    nextSpecial = nextBold;
                else
                    nextSpecial = nextCode;

                if (nextSpecial == -1)
                {
                    paragraph.Inlines.Add(new Run(text.Substring(i)) { Foreground = normalBrush });
                    break;
                }
                else
                {
                    paragraph.Inlines.Add(new Run(text.Substring(i, nextSpecial - i)) { Foreground = normalBrush });
                    i = nextSpecial;
                }
            }
        }

        private static string FlowDocumentToXaml(FlowDocument doc)
        {
            using (var stream = new MemoryStream())
            {
                XamlWriter.Save(doc, stream);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public static FlowDocument XamlToFlowDocument(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
            {
                return new FlowDocument();
            }

            try
            {
                string trimmed = xaml.TrimStart();
                if (trimmed.StartsWith("<FlowDocument"))
                {
                    var context = new ParserContext();
                    context.BaseUri = new Uri(AppDomain.CurrentDomain.BaseDirectory, UriKind.Absolute);

                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xaml)))
                    {
                        return (FlowDocument)XamlReader.Load(stream, context);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing XAML in GeminiService: {ex.Message}");
            }

            var doc = new FlowDocument();
            var p = new Paragraph();
            p.Margin = new Thickness(0, 0, 0, 6);
            p.Inlines.Add(new Run(xaml));
            doc.Blocks.Add(p);
            return doc;
        }
    }

    public class StudyGuideResult
    {
        public string Explanation { get; set; } = string.Empty;
        public LinkResult? VideoLink { get; set; }
        public LinkResult? ArticleLink { get; set; }
        public List<QuestionResult> Questions { get; set; } = new List<QuestionResult>();
    }

    public class LinkResult
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class QuestionResult
    {
        public string QuestionText { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectIndex { get; set; }
    }
}
