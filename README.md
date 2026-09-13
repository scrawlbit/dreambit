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
- **Sistema de UI**: âncoras, botões, layout, **slider** (liga a bus de áudio), **toggle**,
  **barra de progresso**, **campo de texto**, **scroll** e **navegação por foco**
  (teclado/gamepad) — para menus, opções e HUD; mais **camadas de render**, **colisão de
  tilemap** e **localização** (`Localizer`).
- **Animação de sprite por clipes** (`SpriteClip` + `SpriteAnimator`): várias animações
  nomeadas (andar/pular/bater/parado) na mesma folha, com FPS/loop por clipe, evento de fim,
  **flip** e `SpriteAnimatorController` (escolhe o clipe pelo estado e dispara o ataque).
- **Física 2D** com corpos rígidos (`Rigidbody2D` sobre Aether.Physics2D): gravidade,
  colisão com rotação, impulsos, **raycast** e **camadas de colisão** — alternativa ao
  `PlatformerController`.
- **IA de perseguição** (`NavChaser`): segue a tag alvo por pathfinding A* (ou linha reta).
- **Áudio espacial** (`AudioListener` + `AudioSource.Spatial`): atenuação e pan por distância.
- **Luzes 2D** (`Light2D` + `AmbientLight`): lightmap por render target que ilumina sprites e
  tilemap (sem shader). **Tiles animados** e **autotiling** (`Autotile`) no tilemap.
- **Ligar/desligar componente** (`SceneComponent.Enabled`) em runtime — ex.: desligar a
  gravidade numa fase de voo sem remover o componente.
- **Combate**: `Health`, `Hurtbox`/`Hitbox` (dano por time, ativado por evento de frame) e
  `SpriteFlash`; **movimento top-down** (`TopDownController`, 8 direções com colisão) para
  jogos de cima com tile.
- **Áudio** com mixer/buses (Master/Music/SFX) e **timeline de propriedades**
  (`PropertyAnimator`: anima posição/rotação/escala/cor por keyframes).
- Sistemas de runtime para scripts: `SaveGame` (**salvar/carregar**), `Scheduler`
  (**timers/coroutines**), `ObjectPool` (**pooling**), `DataCatalog` (**data-driven**),
  `StateMachine` (**máquina de estados**).
- **Hot-reload de script** (arquivo `.cs` externo recompila ao mudar) e **overlay de
  debug** no Player (F3).
- **Sprites**: recorte de atlas, autodetecção de frames por transparência e **chroma key**
  (remover cor de fundo, automático ou por cor).
- **Editor de tilemap**: novo mapa de um PNG, paleta visual, pintar/apagar com undo,
  **camadas** (visibilidade/ordem) e colisão sólida por tile.
- **Barramento de mensagens** (sinais de jogo) ligando tags, triggers, eventos de
  animação e scripts.
- **Hot-reload** de assets (editar PNG/TMX/WAV recarrega no editor); play restaurável;
  console de logs; export de jogo portátil.

## Fase de demonstração

`DemoAssets/forest-demo.dbscene` é uma fase completa que exercita quase toda a engine numa
cena só (herói animado por clipes, tilemap sólido, plataforma, câmera, parallax, decoração
por atlas, perseguição por pathfinding, física rígida, timeline, trigger de meta, áudio
espacial e HUD). A arte é de terceiros e não vai no repositório — veja
[DemoAssets/README.md](DemoAssets/README.md). O `ForestDemoSmokeTests` monta e valida a fase
inteira sem tela.

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
