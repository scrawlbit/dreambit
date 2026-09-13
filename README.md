# DreamBit

Engine e editor de jogos 2D **standalone em .NET 8**, com MonoGame. Nasceu como uma
extensão do Visual Studio (VSIX, .NET Framework) e foi migrado para uma solução
independente, multiplataforma e testada.

> O código do VSIX antigo (projetos `Old.*`, `Scrawlbit.*`, `DreamBit.Extension`, etc.)
> foi removido após a migração — o histórico continua no Git.

## Projetos

| Projeto | O que é |
|---|---|
| **DreamBit.Engine** | Modelo e runtime da engine (cena, objetos, componentes, física de ledges, tilemap, áudio, scripting, animação). Multi-target `net8.0-windows;net8.0`. |
| **DreamBit.Studio.Avalonia** | Editor **cross-platform** (Windows/macOS/Linux, Rider). Editor principal daqui pra frente. |
| **DreamBit.Studio** | Editor WPF (Windows). Mais completo hoje (canvas MonoGame, pincel de tilemap); em processo de aposentadoria conforme o Avalonia alcança paridade. |
| **DreamBit.Studio.Core** | ViewModels/lógica de editor compartilhados pelos dois editores (sem dependência de UI). |
| **DreamBit.Player** | Runtime do jogo (DesktopGL), roda um `.dbscene`. Cross-platform. |
| **DreamBit.Engine.Tests** | Testes do motor (MSTest). |

## Funcionalidades da engine

- Cena com hierarquia de objetos e `Transform` pai→filho.
- Componentes: `SpriteRenderer` (com **recorte de atlas** via source rect), `SpriteAnimator`
  (sprite sheet, com **eventos por frame**), `PlatformerController`, `TriggerZone`
  (colisão por sobreposição, filtra por **tag**, dispara eventos enter/exit), `AudioSource`,
  `ParticleEmitter`, `FollowTarget`, `RotatorBehavior`, `ScriptComponent` (C# em runtime via
  Roslyn), `TilemapRenderer` (importa Tiled `.tmx`), `Bone` (rig 2D cutout) e
  `SkeletonAnimator` (**keyframes de pose por osso** com timeline).
- **Ledges**: bordas caminháveis one-way (estilo Dust: An Elysian Tail) — a colisão é via
  plataforma, não por tilemap.
- **Hot-reload** de assets (editar PNG/TMX/WAV recarrega no editor).
- Serialização em JSON (`.dbscene`, `.dbprefab`, `.dbproj`), play mode restaurável,
  console de logs, export de jogo portátil.

## Rodar

Editor cross-platform (Avalonia):

```bash
dotnet run --project DreamBit.Studio.Avalonia/DreamBit.Studio.Avalonia.csproj -c Debug
```

Editor WPF (Windows):

```bash
dotnet run --project DreamBit.Studio/DreamBit.Studio.csproj -c Debug
```

Rodar um jogo (uma cena) no runtime:

```bash
dotnet run --project DreamBit.Player/DreamBit.Player.csproj -c Debug -- caminho/para/fase.dbscene
```

## Build e testes

```bash
dotnet build DreamBit.Studio.slnx -c Debug
dotnet test DreamBit.Engine.Tests/DreamBit.Engine.Tests.csproj -c Debug
```

## Empacotar para distribuição

`publish.ps1` gera binários self-contained (sem exigir .NET no destino) dos editores e do
Player por RID (win/linux/osx). Ver `ROADMAP-PROXIMAS-FUNCIONALIDADES.md` para o status.

## Requisitos

.NET 8 SDK. No Rider (qualquer SO), abra `DreamBit.Studio.slnx`.
