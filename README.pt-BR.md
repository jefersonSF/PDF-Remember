# PDF Remember

Um leitor de PDF para Windows que memoriza a última página visualizada de cada documento, permitindo retomar a leitura exatamente de onde você parou.

## Visão geral

O **PDF Remember** é uma aplicação desktop desenvolvida com WPF e C#, utilizando .NET 10.

Seu principal objetivo é tornar a leitura de documentos PDF mais conveniente, armazenando automaticamente a última página visualizada. Ao abrir novamente o mesmo documento, a aplicação restaura a posição anterior de leitura.

Em vez de procurar manualmente a página em que parou, basta abrir novamente o documento e continuar a leitura.

## Funcionalidades

- **Memorização automática de página:** registra a última página visualizada de cada documento PDF.
- **Restauração da posição de leitura:** retorna automaticamente à página anteriormente visualizada ao reabrir um documento.
- **Persistência do progresso de leitura:** mantém o registro da posição de leitura entre diferentes sessões da aplicação.

## Tecnologias utilizadas

- C#
- .NET 10
- WPF (Windows Presentation Foundation)
- XAML

## Download

Você pode baixar a versão compilada da aplicação na seção [GitHub Releases](../../releases).

A versão publicada é distribuída como um **executável autocontido (self-contained) em arquivo único (single-file)**, portanto, não é necessário instalar o .NET 10 Runtime separadamente.

**Plataforma de destino:** Windows x64.

## Requisitos

- Windows 10 ou superior (64 bits).

Não é necessária a instalação separada do .NET Runtime para executar a versão publicada como self-contained.

## Como executar

1. Baixe a versão mais recente na seção [Releases](../../releases).
2. Extraia o pacote baixado, caso necessário.
3. Execute o arquivo executável da aplicação.
4. Abra um documento PDF e comece a leitura.

A aplicação memorizará a última página visualizada para as próximas sessões.

## Compilação a partir do código-fonte

Para compilar o projeto localmente, você precisará de:

- .NET 10 SDK
- Ambiente Windows com suporte ao WPF.

Clone o repositório:

```bash
git clone https://github.com/jefersonSF/PDF-Remember.git
```

Acesse o diretório do projeto e execute a publicação:

```bash
dotnet publish -c Release
```

A aplicação gerada estará disponível no diretório de saída da publicação.

## Licença

Este repositório não possui uma licença explícita. A disponibilização pública do código-fonte não concede automaticamente permissão para copiá-lo, modificá-lo ou redistribuí-lo.