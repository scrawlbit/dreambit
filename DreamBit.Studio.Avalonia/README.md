# DreamBit.Studio.Avalonia — editor cross-platform

Versão do editor DreamBit em **Avalonia** (net8), que roda em **Windows, macOS e Linux**.

Reaproveita integralmente os ViewModels de `DreamBit.Studio.Core`
(`EditorViewModel`, `InspectorViewModel`, `ProjectViewModel`, `SceneInputController`) — a
mesma lógica do editor WPF. A cena é desenhada com o **DrawingContext do Avalonia**
(`SceneView`), sem precisar hospedar um canvas MonoGame; o jogo em si roda pelo
`DreamBit.Player` (DesktopGL, também cross-platform).

## Rodar

```bash
dotnet run --project DreamBit.Studio.Avalonia/DreamBit.Studio.Avalonia.csproj -c Debug
```

Requisitos: SDK do .NET 8. No Rider (qualquer SO), abra a solução e rode o projeto.

## O que já faz

- Hierarquia de objetos, inspetor (nome, posição, rotação).
- Canvas com grid, objetos (sprites como retângulos), ledges e seleção.
- Selecionar/arrastar/rotacionar/escalar (mesmos gizmos do WPF, via `SceneInputController`),
  pan (botão do meio) e zoom (roda).
- Toolbar: novo objeto, excluir, desfazer/refazer, play.

## Diferença para o editor WPF

O editor WPF (`DreamBit.Studio`) usa o canvas MonoGame (D3DImage) e tem a UI completa de
todos os componentes. O editor Avalonia é a **base cross-platform** com desenho vetorial;
a paridade total de painéis de componente pode ser ampliada incrementalmente reusando os
mesmos ViewModels.
