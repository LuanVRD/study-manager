# Plano de desenvolvimento

## Etapa 1 — Estrutura inicial e persistência

- [x] Task 01 — Criar projeto, modelos e armazenamento JSON
  - [x] Criar o projeto WPF (.NET 8) com Nullable habilitado.
  - [x] Criar os modelos de dados (`AppData`, `Study`, `StudyTopic`, `StudyTheme`, `StudyLink`, `LinkType`).
  - [x] Criar o serviço de armazenamento JSON (`JsonStorageService`) com salvamento seguro (arquivo temporário, backup, tratamento de falhas).
  - [x] Criar pastas locais para armazenamento de dados (`Data`) e imagens (`Data/Images`).
  - [x] Confirmar que o projeto compila e executa sem erros.

## Etapa 2 — Tela de estudos

- [ ] Task 02 — Implementar listagem e criação de estudos
  - [ ] Criar a tela inicial (`StudiesView`).
  - [ ] Exibir estudos cadastrados em formato de cards (com imagem/placeholder e percentual de conclusão).
  - [ ] Implementar a modal `StudyDialog` para adicionar novos estudos.
  - [ ] Implementar a seleção e cópia de imagem via `OpenFileDialog` salvando na pasta local com Guid único.

- [ ] Task 03 — Implementar edição e exclusão de estudos
  - [ ] Permitir a edição de estudos (nome e imagem) por meio da modal.
  - [ ] Implementar exclusão de estudos com mensagem de confirmação detalhada.
  - [ ] Garantir que todas as alterações persistam no JSON.
  - [ ] Configurar a navegação na `MainWindow` para permitir a abertura de um estudo.

## Etapa 3 — Tela de tópicos e temas

- [ ] Task 04 — Implementar tópicos e temas
  - [ ] Criar a tela `StudyDetailsView` com navegação de volta.
  - [ ] Exibir os tópicos em blocos expansíveis (`Expander`).
  - [ ] Implementar CRUD completo de Tópicos com confirmação na exclusão.
  - [ ] Implementar CRUD completo de Temas com confirmação na exclusão.
  - [ ] Implementar a marcação/desmarcação de conclusão de temas, atualizando o progresso no JSON e na UI em tempo real.

- [ ] Task 05 — Implementar importação de CSV
  - [ ] Criar parser de CSV local suportando delimitadores `,` e `;` e aspas.
  - [ ] Implementar o fluxo de importação evitando duplicidades case-insensitive por tópico.
  - [ ] Exibir o resumo da importação com as estatísticas de linhas válidas, criadas, ignoradas e inválidas.
  - [ ] Persistir no JSON as atualizações da importação.

## Etapa 4 — Tela de detalhes do tema

- [ ] Task 06 — Implementar links do tema
  - [ ] Criar a tela `ThemeDetailsView` com navegação de volta.
  - [ ] Implementar CRUD completo de links com a modal `LinkDialog` (nome, URL e tipo: Vídeo/Matéria).
  - [ ] Adicionar validação básica da URL (deve iniciar com http/https).
  - [ ] Implementar a abertura do link no navegador padrão com Process.Start e ShellExecute.

- [ ] Task 07 — Implementar anotações com salvamento automático
  - [ ] Criar o TextBox multilinha para anotações livres.
  - [ ] Implementar mecanismo de debounce de 600ms após digitação para salvamento automático.
  - [ ] Exibir indicação discreta de "Salvando..." e "Salvo".
  - [ ] Garantir que alterações pendentes sejam salvas ao sair da tela ou fechar o app.

## Etapa 5 — Revisão final

- [ ] Task 08 — Revisar e testar o sistema completo
  - [ ] Verificar todos os fluxos de CRUD (Estudo, Tópico, Tema, Link) e suas mensagens de confirmação de exclusão.
  - [ ] Verificar navegação entre as três telas principales.
  - [ ] Validar importação de CSV com arquivo de teste.
  - [ ] Validar que o aplicativo inicializa e recupera dados mesmo na ausência do JSON ou com JSON corrompido (usando backup).
  - [ ] Polimento de design escuro moderno e responsividade de textos longos.
