# DreamBit

Engine e editor de jogos 2D **standalone em .NET 11**, com MonoGame. Nasceu como uma
extensão do Visual Studio (VSIX, .NET Framework) e foi migrado para uma solução
independente, multiplataforma e testada.

> O código do VSIX antigo (projetos `Old.*`, `Scrawlbit.*`, `DreamBit.Extension`, etc.)
> foi removido após a migração — o histórico continua no Git.

## Projetos

| Projeto | O que é |
|---|---|
| **DreamBit.Engine** | Modelo e runtime da engine (cena, objetos, componentes, física de ledges, tilemap, áudio, scripting, animação). Multi-target `net11.0-windows;net11.0`. |
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
  easing, eventos, **curvas step/bézier por chave**, **troca de sprite/ordem/cor/visibilidade
  por chave**, **skins**, **onion skin**), `IkConstraint` (**IK de dois ossos**),
  `AttachPoint` (pontos de fixação), `AnimatorController` (estados idle/walk/jump), `TweenComponent`,
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
- **Joints** (`Joint2D`: distância/dobradiça/solda) e **sombras nas luzes** (`ShadowCaster`
  bloqueia as `Light2D`). Todos os componentes são adicionáveis pelo dropdown do editor.
- **Blending de animação** (crossfade de sprite e rig), **Y-sort** e **tilemap isométrico**,
  **tremor/prioridade de câmera**, **áudio adaptativo** (ducking + fade/crossfade), **partículas
  ricas** (burst, cor/tamanho na vida) e **profiler** no overlay F3.
- **Editor**: aninhar GameObjects (hierarquia em árvore, **Ctrl+G**/**Ctrl+Shift+G**), **atalhos
  configuráveis** com exportar/importar, **prefabs com overrides**, **referências de asset por
  GUID** (à prova de renomear), **máquina de estados de animação** com editor visual nó-e-fio, e
  **shaders 2D** (pós-processamento via MGCB).
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

## Fazendo um jogo MonoGame com a DreamBit

Um jogo é um projeto MonoGame próprio que referencia `DreamBit.Engine` e herda de
`DreamBitGame` (ou usa o `SceneHost` direto). A DreamBit cuida de cenas/mapas/animação/
física/câmera/áudio/UI; a lógica fica em componentes C# do jogo.

```csharp
public sealed class MeuJogo : DreamBitGame
{
    protected override void Configure(GameSettings s)
    {
        s.Title = "As Crônicas de Lothar";
        s.VirtualResolution = new Point(1920, 1080); // letterbox em qualquer monitor
        s.AssetRoots.Add("Content");                 // caminhos relativos das cenas
    }

    protected override void OnStart()
    {
        ComponentRegistry.RegisterAssembly(typeof(MeuJogo).Assembly); // [GameComponent] do jogo
        Fonts.Register("default", "fonts/serif.ttf");
        Host.LoadScene("Cenas/Bosque_01.dbscene");
    }
}

[GameComponent("Lothar/Corrupção")]
public sealed class Corrupcao : SceneComponent
{
    public override string DisplayName => "Corrupção";
    [Tooltip("Vida + corrupção")] public float Total { get; set; } = 200;
}
```

No editor, **Arquivo › Componentes do jogo (DLL)…** aponta a DLL compilada do jogo: os
componentes aparecem em "Adicionar componente" (com inspector automático) e são salvos na
cena; o Play do editor passa a DLL e a pasta do projeto para o Player.

### Sistemas para jogos de plataforma/metroidvania
- **Input por jogador** (`Input.Players`, P1..P4, teclado inteiro ou dividido, controles,
  rebind, vibração) e `VirtualInput` para IA/cutscenes.
- **PlatformerController** com coyote time, buffer e pulo variável, aceleração e
  habilidades liberáveis (`MovementAbilities`): pulo duplo, planar, parede, agarrar borda,
  dash, agachar/deslizar, pancada no chão, empurrar, atravessar plataformas, quebrar
  paredes/pisos (`Breakable`), caixotes (`Pushable`), plataformas móveis.
- **Câmera de grupo** (várias tags): enquadra todos com zoom out até `MinZoom`; olhar à
  frente; `CameraZone` limita por sala.
- **Mundo**: `RoomDoor` + `SpawnPoint` + `Persistent` (o grupo atravessa as salas),
  `MapRoom`/`WorldMap`, `Pickup` (libera habilidades), `Switch`/`Gate` (puzzles
  cooperativos), `SavePoint` + `SaveSlots`, `WorldState` (flags persistentes).
- **Diálogos** (`DialogueAsset` JSON, `DialogueBox`, `TalkTrigger`) e **fontes TrueType**
  (`Fonts`, `FontText`).
- **IA e grupo**: `PlatformerFollower` (companheiro), `PartySwitcher` (troca de personagem,
  co-op com drop-in do P2), `PatrolEnemy`, `Projectile`.

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

.NET 11 SDK. No Rider (qualquer SO), abra `DreamBit.Studio.slnx`.
