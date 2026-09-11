# DreamBit.Studio — PoC do editor standalone (.NET 8)

Prova de conceito que demonstra o **editor DreamBit rodando fora do Visual Studio**,
como um aplicativo desktop comum em **.NET 8 + WPF + MonoGame 3.8.2**.

## Por que isso existe

A extensão original (`DreamBit.Extension`) é um **VSIX in-process**: roda dentro do
`devenv.exe`, que é um processo **.NET Framework**. Isso prende todo o editor em
`net48` e, por tabela, prende o **MonoGame em 3.8.0** e o **AutoMapper em 10.1.1**
(a CVE do AutoMapper só é corrigida em versões que exigem .NET 6+).

Um editor **standalone** não tem essa trava. Ele pode usar .NET 8, MonoGame atual e
AutoMapper corrigido — **e é exatamente o que permite abrir/rodar o projeto no Rider**
(ou em qualquer IDE, ou sem IDE nenhum).

## O que o PoC prova

O ponto técnico mais arriscado da migração é **hospedar um canvas MonoGame ao vivo
numa janela desktop sem o Visual Studio**. Isso está resolvido aqui:

- `Hosting/MonoGameSurface.cs` — superfície WPF que renderiza MonoGame via `D3DImage`
  (portado de `ScrawlBit.MonoGame.Interop/Controls/DrawingSurface.cs`).
- `Hosting/GraphicsDeviceService.cs` / `DeviceService.cs` — device D3D compartilhado
  (portado do mesmo projeto de interop, adaptado para net8.0-windows).
- `Editor/SceneEditorRenderer.cs` — mini editor de cena: grid quadriculado + um
  "GameObject" que você **seleciona e arrasta com o mouse**, reproduzindo o laço
  central do editor (render contínuo + input).

> O código de interop portou **quase sem alterações** do projeto original — a mesma
> técnica SharpDX + D3DImage funciona igual em net48 e net8.0-windows. Essa é a prova
> de que a migração é viável sem reescrever o motor de render.

## Como rodar

### Pelo Rider
Abra `DreamBit.Studio.slnx` (na raiz do repositório) e rode o projeto `DreamBit.Studio`.

### Pela linha de comando
```bash
dotnet run --project DreamBit.Studio/DreamBit.Studio.csproj -c Debug
```

Requisitos: SDK do .NET 8 (ou superior) e Windows (usa WindowsDX/D3DImage).
A versão multiplataforma (Rider no Mac/Linux) usaria MonoGame **DesktopGL** + Avalonia —
ver o roadmap na raiz do repositório.
