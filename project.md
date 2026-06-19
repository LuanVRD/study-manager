# Sistema Pessoal de Gerenciamento de Estudos

## 1. Visão geral

O projeto será um sistema desktop extremamente simples para gerenciamento pessoal de estudos.

O sistema deverá ser desenvolvido utilizando:

* C#
* .NET 8
* WPF
* Arquivos JSON para armazenamento
* Armazenamento totalmente local
* Sem banco de dados
* Sem API
* Sem autenticação
* Sem servidor
* Sem integração com serviços externos

O aplicativo deverá rodar diretamente no Windows e salvar todos os dados no computador do usuário.

O foco principal do projeto é simplicidade. Não deverão ser adicionadas arquiteturas complexas, bibliotecas desnecessárias ou funcionalidades que não foram solicitadas.

---

## 2. Objetivo

O sistema permitirá organizar os estudos em uma estrutura hierárquica:

```text
Estudo
└── Tópico
    └── Tema
        ├── Links
        └── Anotações
```

Exemplo:

```text
Angular
├── Fundamentos da plataforma web
│   ├── HTML semântico
│   ├── CSS
│   └── JavaScript
├── TypeScript
│   ├── Tipos
│   └── Interfaces
└── Fundamentos do Angular
    ├── Componentes
    └── Serviços
```

---

## 3. Estrutura principal do sistema

O sistema terá três telas principais:

1. Tela de estudos
2. Tela de tópicos e temas
3. Tela de detalhes do tema

A navegação deverá ser simples e ocorrer dentro da mesma janela principal.

---

# 4. Tela 1 — Estudos

## 4.1 Objetivo

Essa será a tela inicial do sistema.

Ela deverá exibir todos os estudos cadastrados pelo usuário.

Exemplos de estudos:

* Angular
* .NET
* Inglês
* SQL
* Arquitetura de Software

---

## 4.2 Lista de estudos

Os estudos deverão ser exibidos em cards.

Cada card deverá mostrar:

* Imagem do estudo
* Nome do estudo
* Porcentagem de conclusão
* Opção para editar
* Opção para excluir

Ao clicar no card, o usuário deverá acessar a tela de tópicos e temas daquele estudo.

---

## 4.3 Porcentagem de conclusão

A porcentagem de conclusão de um estudo deverá ser calculada com base nos temas concluídos.

Fórmula:

```text
Temas concluídos / Total de temas × 100
```

Exemplo:

```text
Total de temas: 10
Temas concluídos: 4
Progresso: 40%
```

Caso o estudo ainda não possua temas, a porcentagem deverá ser apresentada como 0%.

A porcentagem deverá ser atualizada automaticamente quando um tema for marcado ou desmarcado como concluído.

---

## 4.4 Adicionar estudo

A tela deverá possuir um botão chamado:

```text
Adicionar estudo
```

Ao clicar no botão, deverá ser aberta uma janela modal simples.

Campos:

* Nome do estudo
* Imagem do estudo

O nome será obrigatório.

A imagem será opcional.

O usuário poderá selecionar uma imagem existente no computador.

Quando uma imagem for selecionada, o sistema deverá copiar o arquivo para uma pasta interna do aplicativo, evitando depender do caminho original da imagem.

Exemplo:

```text
Data/
└── Images/
    └── study-angular-guid.png
```

Caso nenhuma imagem seja selecionada, deverá ser exibida uma imagem padrão ou um placeholder simples.

---

## 4.5 Editar estudo

O usuário poderá editar:

* Nome
* Imagem

Ao salvar, as alterações deverão ser persistidas imediatamente no JSON.

---

## 4.6 Excluir estudo

Ao clicar em excluir, o sistema deverá apresentar uma confirmação.

Exemplo:

```text
Deseja realmente excluir o estudo "Angular"?

Todos os tópicos, temas, links e anotações relacionados também serão excluídos.
```

Opções:

* Cancelar
* Excluir

O estudo somente deverá ser excluído após a confirmação.

---

# 5. Tela 2 — Tópicos e temas

## 5.1 Objetivo

Essa tela será aberta quando o usuário clicar em um estudo.

Ela deverá exibir:

* Nome do estudo
* Botão para voltar
* Botão para adicionar tópico
* Botão para importar CSV
* Lista de tópicos
* Temas existentes dentro de cada tópico

---

## 5.2 Tópicos

Os tópicos deverão ser apresentados em blocos expansíveis.

Em WPF, esses blocos podem ser implementados utilizando `Expander`.

Exemplo:

```text
▼ Fundamentos da plataforma web

    HTML semântico
    CSS básico
    JavaScript básico
```

Ao fechar o bloco:

```text
▶ Fundamentos da plataforma web
```

Cada tópico deverá possuir:

* Nome
* Botão para adicionar tema
* Botão para editar o tópico
* Botão para excluir o tópico
* Lista de temas

---

## 5.3 Adicionar tópico

A tela deverá possuir um botão:

```text
Adicionar tópico
```

Ao clicar, deverá ser aberta uma modal contendo:

* Nome do tópico

O nome será obrigatório.

Exemplo:

```text
Fundamentos da plataforma web
```

---

## 5.4 Editar tópico

O usuário poderá alterar o nome do tópico.

A alteração deverá ser salva imediatamente no arquivo JSON.

---

## 5.5 Excluir tópico

Antes da exclusão, deverá ser apresentada uma mensagem de confirmação.

Exemplo:

```text
Deseja realmente excluir o tópico "Fundamentos da plataforma web"?

Todos os temas, links e anotações desse tópico também serão excluídos.
```

---

# 6. Temas

## 6.1 Exibição dos temas

Os temas deverão aparecer em uma lista dentro do tópico correspondente.

Cada tema deverá mostrar:

* Nome
* Botão ou checkbox de conclusão
* Quantidade de links
* Botão de edição
* Botão de exclusão

Exemplo:

```text
[✓] HTML semântico                  3 links
[ ] Estrutura básica do CSS         1 link
[ ] Introdução ao JavaScript        0 links
```

A palavra “link” deverá ser apresentada corretamente no singular ou plural.

Exemplos:

```text
0 links
1 link
2 links
```

Ao clicar no nome ou na área principal do tema, o sistema deverá abrir a tela de detalhes desse tema.

---

## 6.2 Adicionar tema

Cada tópico deverá possuir um botão:

```text
Adicionar tema
```

Ao clicar, deverá abrir uma modal com o campo:

* Nome do tema

Exemplo:

```text
HTML semântico
```

---

## 6.3 Marcar tema como concluído

O usuário poderá marcar ou desmarcar um tema como concluído.

Essa alteração deverá:

* Ser salva imediatamente
* Atualizar a porcentagem do estudo
* Atualizar visualmente o tema

Temas concluídos poderão apresentar uma indicação visual discreta, como:

* Ícone de check
* Texto levemente riscado
* Alteração sutil de opacidade

---

## 6.4 Editar tema

O usuário poderá alterar o nome do tema.

A alteração deverá ser salva no JSON.

---

## 6.5 Excluir tema

Antes da exclusão, deverá ser apresentada uma confirmação.

Exemplo:

```text
Deseja realmente excluir o tema "HTML semântico"?

Todos os links e anotações desse tema também serão excluídos.
```

---

# 7. Importação de tópicos e temas por CSV

## 7.1 Objetivo

Na tela de tópicos e temas deverá existir um botão:

```text
Importar CSV
```

Essa opção permitirá cadastrar vários tópicos e temas de uma única vez.

---

## 7.2 Formato do arquivo

O CSV deverá possuir duas colunas:

```csv
Topico,Tema
Fundamentos da plataforma web,HTML semântico
Fundamentos da plataforma web,CSS básico
Fundamentos da plataforma web,JavaScript básico
TypeScript,Tipos primitivos
TypeScript,Interfaces
TypeScript,Generics
Angular,Componentes
Angular,Serviços
```

O sistema também poderá aceitar ponto e vírgula como separador, facilitando a utilização de arquivos criados pelo Excel em português:

```csv
Topico;Tema
Fundamentos da plataforma web;HTML semântico
Fundamentos da plataforma web;CSS básico
TypeScript;Interfaces
```

---

## 7.3 Regras da importação

Durante a importação:

1. O sistema deverá ler cada linha.
2. Deverá localizar o tópico pelo nome.
3. Caso o tópico não exista, deverá criá-lo.
4. Deverá adicionar o tema ao tópico correspondente.
5. Temas importados deverão começar como não concluídos.
6. O sistema não deverá duplicar um tema que já exista no mesmo tópico.
7. Comparações para evitar duplicidade deverão ignorar diferenças entre letras maiúsculas e minúsculas.
8. Linhas vazias deverão ser ignoradas.
9. Linhas inválidas deverão ser contabilizadas, mas não deverão interromper toda a importação.

Após a importação, deverá ser apresentado um resumo.

Exemplo:

```text
Importação concluída.

3 tópicos criados.
12 temas adicionados.
2 temas ignorados por já existirem.
1 linha inválida ignorada.
```

A importação deverá ocorrer apenas dentro do estudo atualmente aberto.

---

# 8. Tela 3 — Detalhes do tema

## 8.1 Objetivo

Essa tela deverá ser aberta quando o usuário clicar em um tema.

Ela deverá mostrar:

* Nome do estudo
* Nome do tópico
* Nome do tema
* Botão para voltar
* Bloco de links
* Bloco de anotações

---

# 9. Bloco de links

## 9.1 Exibição

O bloco deverá mostrar os links relacionados ao tema.

Cada item deverá apresentar principalmente o nome do link.

Exemplo:

```text
Documentação sobre HTML semântico
Curso de HTML da plataforma X
Vídeo sobre tags semânticas
```

Cada item poderá apresentar um pequeno ícone indicando seu tipo:

* Vídeo
* Matéria ou artigo

---

## 9.2 Abrir link

Ao clicar no item, o endereço deverá ser aberto no navegador padrão do Windows.

A abertura poderá utilizar `Process.Start` com `UseShellExecute = true`.

---

## 9.3 Adicionar link

O bloco deverá possuir um botão:

```text
Adicionar link
```

Ao clicar, deverá abrir uma modal com os campos:

* Nome do link
* URL
* Tipo

Tipos disponíveis:

* Vídeo
* Matéria

Todos os campos serão obrigatórios.

A URL deverá passar por uma validação básica antes de ser salva.

Exemplos válidos:

```text
https://developer.mozilla.org/
https://www.youtube.com/watch?v=exemplo
```

---

## 9.4 Editar link

O usuário poderá editar:

* Nome
* URL
* Tipo

---

## 9.5 Excluir link

A exclusão deverá exigir confirmação.

Exemplo:

```text
Deseja realmente excluir o link "Documentação sobre HTML semântico"?
```

---

# 10. Bloco de anotações

## 10.1 Funcionamento

O bloco de anotações deverá possuir uma área de texto livre.

O usuário poderá escrever qualquer conteúdo, como:

* Resumos
* Exemplos
* Dúvidas
* Trechos de código
* Observações pessoais
* Conteúdo aprendido

Não será necessário utilizar um editor de texto avançado. Um `TextBox` multilinha é suficiente.

---

## 10.2 Salvamento automático

As anotações deverão ser salvas automaticamente enquanto o usuário digita.

Para evitar gravar o arquivo a cada tecla pressionada, deverá ser usado um pequeno atraso, por exemplo:

```text
500 a 800 milissegundos depois da última alteração
```

Esse comportamento é conhecido como debounce.

Quando o usuário parar de digitar, o sistema deverá atualizar o JSON automaticamente.

Poderá existir uma indicação discreta:

```text
Salvando...
```

e depois:

```text
Salvo
```

Não será necessário incluir um botão manual de salvar anotações.

---

# 11. Operações CRUD

As seguintes entidades deverão possuir operações completas de criação, leitura, edição e exclusão:

* Estudos
* Tópicos
* Temas
* Links

As anotações deverão permitir:

* Leitura
* Edição
* Salvamento automático

Todas as exclusões deverão possuir confirmação antes de serem executadas.

---

# 12. Armazenamento local

## 12.1 Arquivo JSON

Todos os dados poderão ser armazenados em um único arquivo:

```text
Data/app-data.json
```

Manter um único arquivo é suficiente para esse projeto e ajuda a reduzir a complexidade.

Exemplo simplificado:

```json
{
  "studies": [
    {
      "id": "guid",
      "name": "Angular",
      "imagePath": "Data/Images/angular.png",
      "createdAt": "2026-06-19T15:00:00",
      "updatedAt": "2026-06-19T15:00:00",
      "topics": [
        {
          "id": "guid",
          "name": "Fundamentos da plataforma web",
          "themes": [
            {
              "id": "guid",
              "name": "HTML semântico",
              "isCompleted": false,
              "notes": "Minhas anotações...",
              "links": [
                {
                  "id": "guid",
                  "name": "Documentação MDN",
                  "url": "https://developer.mozilla.org/",
                  "type": "Article"
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}
```

---

## 12.2 Salvamento seguro

O serviço de armazenamento deverá:

* Criar a pasta `Data` automaticamente
* Criar o arquivo JSON caso ele não exista
* Utilizar `System.Text.Json`
* Formatar o JSON para facilitar a leitura
* Tratar arquivo vazio ou inválido
* Não encerrar o aplicativo caso ocorra um erro de leitura
* Salvar os dados após cada alteração importante

Para reduzir o risco de corromper os dados, o sistema poderá:

1. Gravar primeiro em um arquivo temporário.
2. Substituir o arquivo principal depois da gravação.
3. Manter uma cópia simples chamada `app-data.backup.json`.

Essa proteção deverá continuar simples, sem criar um sistema complexo de versionamento.

---

# 13. Modelos de dados sugeridos

## Study

```csharp
public class Study
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public List<StudyTopic> Topics { get; set; } = [];
}
```

## StudyTopic

O nome `StudyTopic` pode ser utilizado para evitar conflito com classes do próprio .NET.

```csharp
public class StudyTopic
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<StudyTheme> Themes { get; set; } = [];
}
```

## StudyTheme

```csharp
public class StudyTheme
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<StudyLink> Links { get; set; } = [];
}
```

## StudyLink

```csharp
public class StudyLink
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public LinkType Type { get; set; }
}
```

## LinkType

```csharp
public enum LinkType
{
    Video,
    Article
}
```

---

# 14. Estrutura de pastas sugerida

A estrutura deverá continuar pequena:

```text
StudyManager/
├── Models/
│   ├── AppData.cs
│   ├── Study.cs
│   ├── StudyTopic.cs
│   ├── StudyTheme.cs
│   ├── StudyLink.cs
│   └── LinkType.cs
├── Services/
│   ├── JsonStorageService.cs
│   ├── CsvImportService.cs
│   └── ImageService.cs
├── Views/
│   ├── StudiesView.xaml
│   ├── StudyDetailsView.xaml
│   ├── ThemeDetailsView.xaml
│   └── Dialogs/
│       ├── StudyDialog.xaml
│       ├── TopicDialog.xaml
│       ├── ThemeDialog.xaml
│       └── LinkDialog.xaml
├── Data/
│   ├── app-data.json
│   └── Images/
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── App.xaml
└── StudyManager.csproj
```

Não deverão ser criadas várias camadas, projetos separados, repositórios genéricos, banco de dados, API ou infraestrutura desnecessária.

---

# 15. Interface e design

As imagens fornecidas junto ao projeto serão apenas protótipos visuais.

Elas deverão ser utilizadas como referência para:

* Distribuição dos elementos
* Estrutura das telas
* Hierarquia das informações
* Posição aproximada dos botões
* Organização dos cards e blocos

O design não precisa copiar exatamente:

* Cores
* Fontes
* Sombras
* Espaçamentos
* Bordas
* Tamanhos

As cores e o estilo visual poderão ser ajustados para melhorar:

* Legibilidade
* Contraste
* Consistência
* Usabilidade
* Aparência geral

O resultado deverá manter um visual moderno, simples e limpo, sem adicionar elementos excessivos.

---

# 16. Requisitos não funcionais

O sistema deverá:

* Rodar localmente no Windows
* Funcionar sem internet
* Iniciar rapidamente
* Ser fácil de usar
* Salvar dados automaticamente
* Manter código simples e legível
* Exibir mensagens de erro amigáveis
* Não perder todos os dados caso uma imagem deixe de existir
* Não travar quando o JSON estiver vazio
* Não utilizar banco de dados
* Não utilizar servidor
* Não exigir instalação de serviços externos

---

# 17. Fora do escopo

Não deverão ser implementados neste momento:

* Login
* Cadastro de usuários
* Sincronização em nuvem
* Banco de dados
* API
* Sistema web
* Aplicativo mobile
* Notificações
* Calendário
* Cronômetro Pomodoro
* Gamificação
* Inteligência artificial
* Markdown avançado
* Editor de código
* Relatórios complexos
* Gráficos complexos
* Compartilhamento de estudos
* Controle de permissões
* Sistema de plugins

Essas funcionalidades somente deverão ser adicionadas futuramente caso exista uma solicitação explícita.

---

# 18. Critérios gerais de conclusão

O projeto será considerado funcional quando for possível:

1. Criar, editar e excluir estudos.
2. Adicionar uma imagem a um estudo.
3. Visualizar estudos em cards.
4. Abrir um estudo.
5. Criar, editar e excluir tópicos.
6. Expandir e recolher tópicos.
7. Criar, editar e excluir temas.
8. Marcar temas como concluídos.
9. Calcular automaticamente o progresso do estudo.
10. Importar tópicos e temas por CSV.
11. Abrir a tela de detalhes de um tema.
12. Criar, editar, excluir e abrir links.
13. Classificar links como vídeo ou matéria.
14. Escrever anotações livremente.
15. Salvar anotações automaticamente.
16. Persistir todos os dados em JSON.
17. Recuperar os dados após fechar e abrir o aplicativo.
18. Solicitar confirmação antes de qualquer exclusão.
