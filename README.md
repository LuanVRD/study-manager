# Gerenciador de Estudos — Importador de CSV

Este projeto de Gerenciamento de Estudos permite importar tópicos e temas em lote a partir de arquivos no formato CSV diretamente na tela de tópicos e temas de um estudo selecionado.

## Como Usar o Importador

1. Abra o aplicativo.
2. Clique no card do estudo ao qual deseja adicionar o conteúdo (ex: *Angular*, *C#*, *TypeScript*).
3. Na barra superior do estudo, clique no botão **"Importar CSV"**.
4. Selecione o arquivo `.csv` desejado e confirme.
5. Uma janela com o resumo da importação será exibida, mostrando quantos tópicos e temas foram criados, ignorados ou linhas inválidas.

---

## Formato do Arquivo CSV

O arquivo deve conter **duas colunas** obrigatórias. O importador identifica automaticamente se o delimitador utilizado é **vírgula (`,`)** ou **ponto e vírgula (`;`)**.

### Colunas Requeridas
* **Topico**: O nome do tópico (ex: *Fundamentos*, *Banco de dados*).
* **Tema**: O assunto específico contido no tópico (ex: *HTML Semântico*, *Tipos Primitivos*).

> [!NOTE]
> A primeira linha do arquivo será sempre tratada como o cabeçalho (`Topico,Tema`) e ignorada no processo de importação. Linhas em branco também são ignoradas.

---

## Regras de Negócio e Validações

1. **Associação ao Estudo**: O conteúdo importado afetará apenas o estudo que está atualmente aberto no aplicativo.
2. **Criação Automática de Tópicos**: Se o tópico especificado na linha não existir, o sistema irá criá-lo automaticamente. Se já existir, o tema será inserido sob o tópico existente.
3. **Prevenção de Duplicidades**: O importador impede que temas repetidos sejam criados **dentro de um mesmo tópico**. A comparação é **case-insensitive** (ignora maiúsculas e minúsculas).
4. **Tratamento de Aspas Duplas**: Se o nome do tópico ou tema contiver o caractere separador (vírgula ou ponto e vírgula), envolva o texto entre aspas duplas (ex: `"Lógica, Algoritmos e Sintaxe"`).
5. **Robustez contra Erros**: Linhas vazias ou inválidas (que não possuam exatamente o par Tópico e Tema) serão ignoradas e não interromperão o restante da importação.
6. **Persistência**: Ao fim do processo, as mudanças são gravadas de forma segura no arquivo `app-data.json`.

---

## Exemplos de Arquivos Válidos

### Exemplo 1: Separador por Vírgula (`,`)
```csv
Topico,Tema
Fundamentos da plataforma web,HTML semântico
Fundamentos da plataforma web,CSS básico
TypeScript,Tipos primitivos
TypeScript,Interfaces
```

### Exemplo 2: Separador por Ponto e Vírgula (`;`)
```csv
Topico;Tema
Fundamentos da plataforma web;HTML semântico
TypeScript;Interfaces
WPF;XAML Básico
```

### Exemplo 3: Uso de Aspas Duplas (para nomes com separador)
```csv
Topico,Tema
"Lógica de Programação, Algoritmos",Operadores Lógicos
"Lógica de Programação, Algoritmos",Estruturas Condicionais
"Arquitetura, Design Patterns",Dependency Injection
```
