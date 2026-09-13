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
| **DreamBit.Studio.Avalonia** | **O editor** — cross-platform (Windows/macOS/Linux, Rider). Canvas com texturas, tilemap, ledges, carimbo de atlas, timeline de rig, console. |
| **DreamBit.Studio.Core** | ViewModels e lógica de editor (sem dependência de UI): ViewModels, `SceneInputController`, exportador/launcher/content-builder. |
| **DreamBit.Player** | Runtime do jogo (DesktopGL), roda um `.dbscene` (com transição entre fases). Cross-platform. |
| **Scrawlbit** | Helpers genéricos compartilhados (engine + jogo + scripts): `Mathf`, `Easing`, coleções, comparadores. |
| **DreamBit.Engine.Tests** | Testes do motor (MSTest). |

## Funcionalidades da engine

- Cena com hierarquia de `Transform` pai→filho, **z-order** e serialização JSON.
- Componentes: `SpriteRenderer` (**recorte de atlas**), `SpriteAnimator` (sprite sheet com
  **eventos por frame**), `TilemapRenderer` (Tiled `.tmx`), `PlatformerController`,
  `BoxCollider` (**colisão sólida AABB**), ledges one-way, `TriggerZone` (filtro por **tag**,
  enter/exit, envio de mensagem), `MessageListener`, `AudioSource`, `ParticleEmitter`,
  `FollowTarget`, `RotatorBehavior`, `ScriptComponent` (C# em runtime via Roslyn),
  `Bone` + `SkeletonAnimator` (**rig cutout**: keyframes de pose, **clipes nomeados**,
  easing, eventos), `AnimatorController` (estados idle/walk/jump), `TweenComponent`,
  `CameraComponent` (follow/deadzone/bounds/zoom), `TextRenderer` (fonte pixel, mundo ou
  **HUD**), `SceneExit` (**transição de fase**), `UiAnchor` + `UiButton` (**UI clicável**),
  `ParallaxLayer` (**parallax**), `TimerComponent`.
- **Sistema de UI**: âncoras às bordas da tela, botões clicáveis (mouse/toque) que
  disparam mensagens e contêiner de layout (menus); **camadas de render** e **colisão de
  tilemap** (tiles sólidos); **localização** (`Localizer` + chave no texto).
- **Física 2D** com corpos rígidos (`Rigidbody2D` sobre Aether.Physics2D): gravidade,
  colisão com rotação, impulsos — alternativa ao `PlatformerController`.
- Sistemas de runtime para scripts: `SaveGame` (**salvar/carregar**), `Scheduler`
  (**timers/coroutines**), `ObjectPool` (**pooling**), `DataCatalog` (**data-driven**),
  `StateMachine` (**máquina de estados**).
- **Hot-reload de script** (arquivo `.cs` externo recompila ao mudar) e **overlay de
  debug** no Player (F3).
- **Barramento de mensagens** (sinais de jogo) ligando tags, triggers, eventos de
  animação e scripts.
- **Hot-reload** de assets (editar PNG/TMX/WAV recarrega no editor); play restaurável;
  console de logs; export de jogo portátil.

Estado detalhado e itens planejados: veja [ROADMAP.md](ROADMAP.md).

## Rodar

Editor (Avalonia, cross-platform):

```bash
dotnet run --project DreamBit.Studio.Avalonia/DreamBit.Studio.Avalonia.csproj -c Debug
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

`publish.ps1` gera binários self-contained (sem exigir .NET no destino) do editor e do
Player por RID (win/linux/osx).

## Requisitos

.NET 8 SDK. No Rider (qualquer SO), abra `DreamBit.Studio.slnx`.
