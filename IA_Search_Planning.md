# Planejamento: Pesquisa IA Avançada, Links e Questionário Interativo

Este planejamento descreve o design e as etapas de implementação para a reformulação da tela de detalhes do tema (`ThemeDetailsView`) do **StudyManager**. A IA será configurada para gerar, em uma única consulta, a explicação formatada, os links de apoio (vídeo e documentação) e 5 perguntas de fixação. A interface do usuário será reestruturada em um sistema moderno de abas com painéis expansíveis/recolhíveis.

---

## 1. Planejamento do Sistema

### 1.1. Modificações na Estrutura de Dados (Models)
* **`StudyTheme.cs`**:
  * Adicionar propriedade `AiExplanation` (string observável) para armazenar o conteúdo explicativo gerado pela IA.
  * Adicionar propriedade `Questions` (`ObservableCollection<ThemeQuestion>`) para guardar o questionário atual do tema.
* **`ThemeQuestion.cs`** (Novo Modelo):
  * Representa uma questão com: `QuestionText` (string), `Options` (coleção de 5 strings), `CorrectIndex` (int) e `SelectedIndex` (int? observável) para persistir e avaliar a resposta selecionada pelo usuário.

### 1.2. Chamada Inteligente à API (Gemini Service)
* Configurar o prompt em `GeminiService.cs` para instruir o Gemini a gerar respostas estritamente no formato **JSON**, utilizando a propriedade `"responseMimeType": "application/json"`.
* Exemplo do formato JSON esperado do Gemini:
```json
{
  "explanation": "Conteúdo detalhado do tema em Markdown...",
  "videoLink": {
    "name": "Título do vídeo recomendado no YouTube",
    "url": "https://www.youtube.com/watch?v=..."
  },
  "articleLink": {
    "name": "Documentação Oficial / Artigo de Referência",
    "url": "https://..."
  },
  "questions": [
    {
      "questionText": "Pergunta 1...",
      "options": ["Opção 1", "Opção 2", "Opção 3", "Opção 4", "Opção 5"],
      "correctIndex": 2
    },
    ...
  ]
}
```
* **Processamento no C#**: Ao receber a resposta JSON:
  1. Converter a `explanation` do Markdown para XAML FlowDocument.
  2. Adicionar os dois links (`videoLink` e `articleLink`) diretamente à lista de links do tema (`Theme.Links`) como `StudyLink` do tipo `Video` e `Article` respectivamente.
  3. Preencher a coleção de perguntas (`Theme.Questions`).

### 1.3. Reformulação do Layout da Tela (`ThemeDetailsView.xaml`)
* O layout será reorganizado em duas abas principais por meio de um `TabControl` com visual escuro moderno e minimalista:
  1. **Caderno de Estudos**:
     * **Painel Esquerdo (Anotações)**: Mantém o editor de notas ricas livre (RichTextBox).
     * **Painel Direito (Explicação da IA)**: Um visualizador formatado do FlowDocument (com `RichTextBox` marcado como `IsReadOnly="True"`) que suporta seleção e cópia, além de um botão dedicado para copiar todo o texto.
     * **Recolhimento**: Botões no topo de cada painel permitem expandi-lo em tela cheia (colapsando o outro).
  2. **Exercícios e Links**:
     * **Painel Esquerdo (Links de Apoio)**: Exibe a lista de links, permitindo reordenação (Drag & Drop) e CRUD normal.
     * **Painel Direito (Questionário)**: Mostra as 5 perguntas. Cada pergunta apresenta suas 5 opções estilizadas. Ao clicar em uma opção, a interface avalia a resposta em tempo real (destaque verde para acerto, vermelho para erro) e desabilita outras opções da mesma pergunta. Um botão de "Refazer Exercícios" limpa as seleções.

---

## 2. Cronograma de Desenvolvimento (Tasks)

### Etapa 1 — Modelos e Persistência de Dados
- [x] Task 1.1 — Atualizar modelo `StudyTheme.cs`
  - [x] Adicionar propriedade observável `AiExplanation` (string).
  - [x] Adicionar propriedade observável `Questions` (`ObservableCollection<ThemeQuestion>`).
  - [x] Inicializar `Questions` no construtor.
- [x] Task 1.2 — Criar o modelo `ThemeQuestion.cs`
  - [x] Definir a classe com propriedades: `QuestionText` (string), `Options` (coleção de strings), `CorrectIndex` (int) e `SelectedIndex` (int? observável).
  - [x] Certificar que herda de `ViewModelBase`.

### Etapa 2 — Serviço da API do Gemini (Serviço de IA)
- [x] Task 2.1 — Implementar geração estruturada em `GeminiService.cs`
  - [x] Criar o método `GenerateStudyGuideAsync(string studyName, string topicName, string themeName, string apiKey)`.
  - [x] Escrever o prompt robusto exigindo saída JSON com chaves exatas.
  - [x] Configurar `responseMimeType` como `application/json` na chamada da API.
  - [x] Adicionar tratamento de erros e fallback para o caso de desserialização falhar.

### Etapa 3 — Integração com a ViewModel (`ThemeDetailsViewModel`)
- [x] Task 3.1 — Atualizar comandos e propriedades na ViewModel
  - [x] Substituir o comando antigo de consulta por `GenerateStudyGuideCommand` (antigo `ConsultGeminiCommand`).
  - [x] Lógica para salvar a explicação da IA, adicionar os dois links gerados à coleção `Theme.Links` e salvar o questionário em `Theme.Questions`.
  - [x] Expor comandos para controle de layout (ex. alternar abas, recolher/expandir painéis).
  - [x] Implementar comando `CopyAiExplanationCommand` para copiar texto limpo da IA.
  - [x] Implementar comandos `AnswerQuestionCommand` e `ClearAnswersCommand` para controle do quiz.

### Etapa 4 — Interface Gráfica e Layout (Views)
- [x] Task 4.1 — Estrutura de Abas Modernas em `ThemeDetailsView.xaml`
  - [x] Criar controle de abas com duas seções: "Caderno de Estudos" e "Links e Exercícios".
- [x] Task 4.2 — Implementação da aba "Caderno de Estudos"
  - [x] Adicionar editor de anotações (esquerda) e visualizador da IA somente leitura (direita) com botão de cópia.
  - [x] Adicionar botões de expandir/recolher adaptativos para cada painel.
- [x] Task 4.3 — Implementação da aba "Links e Exercícios"
  - [x] Adicionar painel de links de apoio (esquerda) com botões CRUD e drag & drop.
  - [x] Adicionar painel do questionário (direita) renderizando as 5 perguntas geradas.
  - [x] Estilizar os cards de opção com mudança de cor (Verde para acerto, Vermelho para erro) e desabilitar cliques após seleção.
  - [x] Adicionar botão para resetar questionário ("Refazer Exercícios").

### Etapa 5 — Verificação e Polimento
- [x] Task 5.1 — Validar fluxos de interação e UI
  - [x] Testar a resposta da IA e injeção automática de links e perguntas.
  - [x] Verificar persistência dos dados salvos no JSON (incluindo as respostas do usuário no quiz).
  - [x] Garantir que o design se mantenha responsivo e adaptado a temas escuros.
