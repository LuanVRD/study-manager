using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StudyManager.Services
{
    public class GroqService
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public async Task<StudyGuideResult> GenerateStudyGuideAsync(string studyName, string topicName, string themeName, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("Chave da API do Groq não configurada.");
            }

            // Fase 1: Gerar a explicação teórica rica
            string explanationPrompt = $@"
Você é um especialista em tecnologia e desenvolvimento de software. Crie uma explicação completa, detalhada e aprofundada sobre o tema '{themeName}', que faz parte do tópico '{topicName}' no contexto do estudo de '{studyName}'.
Foque em definições claras, boas práticas contemporâneas de desenvolvimento de software, casos de uso práticos e trechos de código limpos e bem estruturados.
Como o leitor já é um desenvolvedor experiente, não explique conceitos básicos de programação.
Sua resposta deve ser estruturada em Markdown, utilizando títulos com '#' (H1), '##' (H2), '###' (H3), listas com marcadores '*' ou '-' para pontos importantes, negrito com '**' para termos-chave e blocos de código com '```' (especificando a linguagem, como csharp, javascript, typescript, html, css, etc.) para os exemplos práticos.
";

            var explanationRequestBody = new JsonObject
            {
                ["messages"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["role"] = "user",
                        ["content"] = explanationPrompt
                    }
                },
                ["temperature"] = 0.3
            };

            string explanationText = await SendApiRequestWithFallbackAsync(apiKey, explanationRequestBody, isJsonMode: false);

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
                ["messages"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["role"] = "user",
                        ["content"] = guidePrompt
                    }
                },
                ["temperature"] = 0.2,
                ["response_format"] = new JsonObject
                {
                    ["type"] = "json_object"
                }
            };

            string guideTextResult = await SendApiRequestWithFallbackAsync(apiKey, guideRequestBody, isJsonMode: true);

            try
            {
                var result = JsonSerializer.Deserialize<StudyGuideResult>(guideTextResult, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    throw new Exception("Falha ao desserializar o resultado do guia de estudos gerado pelo Groq.");
                }

                // Injetamos a explicação gerada na Fase 1
                result.Explanation = explanationText;

                return result;
            }
            catch (JsonException ex)
            {
                throw new Exception($"A resposta do Groq não está em um formato JSON válido: {ex.Message}\nResposta crua:\n{guideTextResult}");
            }
        }

        private async Task<string> SendApiRequestWithFallbackAsync(string apiKey, JsonObject requestBody, bool isJsonMode)
        {
            string[] models = { "llama-3.3-70b-versatile", "llama-3.1-8b-instant", "llama3-8b-8192" };
            Exception? lastException = null;

            foreach (var model in models)
            {
                // Injeta o modelo no request
                requestBody["model"] = model;

                string url = "https://api.groq.com/openai/v1/chat/completions";
                try
                {
                    return await SendApiRequestAsync(url, apiKey, requestBody);
                }
                catch (Exception ex) when (
                    ex.Message.Contains("503") || ex.Message.Contains("429") || 
                    ex.Message.Contains("TooManyRequests") || ex.Message.Contains("Rate limit") || 
                    ex.Message.Contains("400") || ex.Message.Contains("BadRequest")
                )
                {
                    lastException = ex;
                    System.Diagnostics.Debug.WriteLine($"Modelo {model} do Groq falhou com erro: {ex.Message}. Tentando modelo fallback...");

                    if (ex.Message.Contains("429") || ex.Message.Contains("TooManyRequests") || ex.Message.Contains("Rate limit"))
                    {
                        await Task.Delay(2000);
                    }
                }
            }

            if (lastException != null)
            {
                string cleanMessage = lastException.Message;
                if (lastException.Message.Contains("429") || lastException.Message.Contains("TooManyRequests") || lastException.Message.Contains("Rate limit"))
                {
                    cleanMessage = "Limite de cota de requisições excedido na API gratuita do Groq. Por favor, aguarde cerca de 30 segundos.";
                }
                else if (lastException.Message.Contains("400") || lastException.Message.Contains("BadRequest"))
                {
                    cleanMessage = "Requisição inválida enviada ao Groq. Verifique a formatação.";
                }
                throw new Exception(cleanMessage, lastException);
            }

            throw new Exception("Falha ao se comunicar com a API do Groq em todos os modelos tentados.");
        }

        private async Task<string> SendApiRequestAsync(string url, string apiKey, JsonObject requestBody)
        {
            HttpResponseMessage response = null!;
            int maxRetries = 3;
            int delayMs = 1500;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    request.Headers.Add("Authorization", $"Bearer {apiKey}");
                    var content = new StringContent(requestBody.ToJsonString(), Encoding.UTF8, "application/json");
                    request.Content = content;

                    response = await HttpClient.SendAsync(request);

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                        response.StatusCode == (System.Net.HttpStatusCode)429)
                    {
                        if (attempt == maxRetries) break;
                        await Task.Delay(delayMs * attempt);
                        continue;
                    }

                    break;
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                string errorMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro na API do Groq: {response.StatusCode} - {errorMsg}");
            }

            string responseString = await response.Content.ReadAsStringAsync();
            JsonNode? jsonResponse = JsonNode.Parse(responseString);
            string textResult = jsonResponse?["choices"]?[0]?["message"]?["content"]?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(textResult))
            {
                throw new Exception("Não foi possível obter resposta da IA do Groq.");
            }

            return textResult;
        }
    }
}
