# DreamBit.Studio — editor standalone (.NET 8)

Editor do DreamBit rodando **fora do Visual Studio**, como aplicativo desktop
em **.NET 8 + WPF + MonoGame 3.8.2**. É o que destrava .NET 8, MonoGame atual,
o AutoMapper corrigido e a **versão Rider** (ver `ROADMAP-STANDALONE-DOTNET8.md`).

## O que já funciona

- **Canvas MonoGame ao vivo** dentro do WPF via `D3DImage` (`Hosting/MonoGameSurface`,
  portado de `ScrawlBit.MonoGame.Interop`).
- **3 painéis**: Hierarquia (lista de objetos) · Cena (canvas) · Inspetor (nome, posição,
  rotação, escala editáveis).
- **Edição de cena**: criar/excluir objetos, **selecionar e arrastar** com o mouse,
  **pan** (botão do meio/direito), **zoom** (roda), **snap ao grid**, **Delete** para excluir.
- **Play mode**: roda o laço de update; o componente `RotatorBehavior` anima a cena.
- **Salvar/abrir cena** em JSON (`.dbscene`) via `System.Text.Json`.

Arquitetura (motor `DreamBit.Engine` + shell `DreamBit.Studio`) e o que ainda falta
(gizmos, undo/redo, pipeline de conteúdo, multiplataforma) estão no roadmap na raiz.

## Como rodar

### Pelo Rider
Abra `DreamBit.Studio.slnx` (raiz do repositório) e rode o projeto `DreamBit.Studio`.

### Pela linha de comando
```bash
dotnet run --project DreamBit.Studio/DreamBit.Studio.csproj -c Debug
```

Requisitos: SDK do .NET 8+ e Windows (usa WindowsDX/D3DImage). A versão
multiplataforma (Rider no Mac/Linux) usaria MonoGame **DesktopGL** + Avalonia — ver o roadmap.

## Testes

```bash
dotnet test DreamBit.Engine.Tests/DreamBit.Engine.Tests.csproj
```
