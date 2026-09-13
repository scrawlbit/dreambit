# DreamBit — Roadmap

Estado da engine e do editor DreamBit (.NET 8 / MonoGame), e itens planejados.

## Entregue

### Migração e base
Extensão VSIX (.NET Framework) migrada para uma solução standalone em .NET 8:
`DreamBit.Engine` (modelo/runtime multi-target), `DreamBit.Studio.Avalonia` (editor
cross-platform), `DreamBit.Studio.Core` (lógica de editor), `DreamBit.Player` (runtime
DesktopGL), `Scrawlbit` (helpers compartilhados). Editor WPF antigo aposentado; código
legado (`Old.*`, VSIX, `Scrawlbit.*` antigos) removido. Vulnerabilidades corrigidas e
MonoGame atualizado.

### Engine
- Cena, hierarquia de `Transform`, serialização JSON (`.dbscene`/`.dbprefab`/`.dbproj`).
- Componentes: `SpriteRenderer` (com recorte de atlas), `SpriteAnimator` (sprite sheet
  com eventos por frame), `TilemapRenderer` (import Tiled `.tmx`), `PlatformerController`,
  `BoxCollider` (colisão sólida AABB), ledges one-way, `TriggerZone` (filtro por tag,
  enter/exit, envio de mensagem), `MessageListener`, `AudioSource`, `ParticleEmitter`,
  `FollowTarget`, `RotatorBehavior`, `ScriptComponent` (C# em runtime via Roslyn),
  `Bone` + `SkeletonAnimator` (rig cutout, keyframes de pose, clipes nomeados, easing,
  eventos por tempo), `AnimatorController` (estados idle/walk/jump), `TweenComponent`
  (interpola propriedade com easing), `CameraComponent` (follow/deadzone/bounds/zoom),
  `TextRenderer` (fonte pixel embutida, mundo ou HUD), `SceneExit` (transição de fase),
  `UiAnchor` (âncora de HUD), `UiButton` (botão clicável), `ParallaxLayer` (fundo com
  profundidade), `TimerComponent` (timer com mensagem).
- **Fixo na tela (HUD)**: qualquer objeto (com seus filhos e componentes — sprite, texto,
  barra de vida) pode ser marcado para não andar com a câmera, com a posição virando
  coordenada de tela.
- **Sistema de UI**: âncoras em relação às bordas da tela e botões clicáveis (mouse/toque)
  que disparam mensagens no barramento; funciona no jogo e no play do editor.
- **Camadas de render** (grossas, antes do z-order) e **parallax de fundo** por eixo.
- **Colisão direto do tilemap** (tiles sólidos por célula, opcionalmente por camada).
- **UI**: âncoras (`UiAnchor`), botões (`UiButton`), layout (`UiLayout`), **slider** (com
  ligação a bus de áudio), **toggle**, **barra de progresso**, **campo de texto** editável,
  **scroll container** (`UiScrollView`) e **navegação por foco** teclado/gamepad
  (`UiNavigator` + realce) — para menus, opções e HUD.
- **Localização**: `Localizer` (tabelas de texto por idioma, JSON) e chave de localização
  no `TextRenderer`.
- **Física 2D com corpos rígidos** (`Rigidbody2D` sobre Aether.Physics2D): gravidade,
  colisão com rotação, empilhamento, impulsos; formas caixa/círculo; **raycast** e
  **camadas de colisão** (categoria/máscara); alternativa ao `PlatformerController`.
- **Áudio com mixer/buses** (`AudioMixer`: Master/Music/SFX, volume por bus ao vivo).
- **Timeline de propriedades** (`PropertyAnimator`): anima posição/rotação/escala/cor por
  keyframes, além do `TweenComponent` (de-para simples).
- **Navegação/pathfinding A*** (`Pathfinding` + `NavGrid`): caminho em grade a partir dos
  tiles sólidos, para IA de inimigos/NPCs.
- Sistemas de runtime para scripts: `SaveGame` (salvar/carregar progresso, JSON),
  `Scheduler` (timers/coroutines por tempo), `ObjectPool` (reaproveitar objetos),
  `DataCatalog` (catálogos data-driven), `StateMachine` (máquina de estados genérica).
- **Hot-reload de script** durante o play (arquivo `.cs` externo recompila ao mudar);
  **overlay de debug** no Player (F3: FPS, objetos, cena).
- **Autodetecção de frames por transparência**: `FrameDetector` acha frames de tamanhos
  diferentes numa sprite sheet (regiões conexas), além do fatiador por grade.
- **Chroma key**: remove a cor de fundo de sprites/animações (automático pela borda ou por
  cor escolhida, com tolerância).
- **Editor de tilemap**: criar de um PNG, paleta visual para escolher o tile, pintar/apagar
  com undo, **camadas** (adicionar/selecionar/visibilidade/ordem) e colisão sólida por tile.
- Barramento de mensagens (sinais de jogo); z-order global; input mapeável por ações
  (teclado + gamepad + mouse/toque); play restaurável; hot-reload de assets; transição
  entre fases.
- Editor: fatiador de sprite sheet por grade; adição de componentes por dropdown.

### Editor (Avalonia, cross-platform)
- Cenas em abas, abrir/salvar, lista de cenas do projeto, prefabs.
- Ferramentas: seleção com gizmos (mover/rotacionar/escalar), ledge, pincel de tilemap,
  carimbo de atlas, multisseleção (hierarquia e caixa), snap.
- Inspetor de todos os componentes (adição por dropdown), timeline de rig, console de
  logs, assets browser, tema claro/escuro, preferências (localização da engine).
- Rodar no Player, exportar jogo (self-contained/portátil), build de conteúdo (MGCB).

## Planejado

Itens úteis para cobrir jogos 2D completos, com o equivalente em engines de mesmo
propósito (Godot, Unity 2D, GameMaker, Construct, Defold, Phaser) como referência.

### Rendering e cena
- **Luzes e shaders 2D** (Godot 2D lights, Unity URP 2D) — depende do pipeline de shaders (MGCB).
- **Áudio espacial** e música em camadas (o mixer/buses já existe).

### Animação
- **Import Aseprite** (`.ase`/`.json`) — a autodetecção por transparência e o fatiador por
  grade já cobrem sprite sheets PNG.

### Física
- **Joints** (juntas entre corpos) — raycast e camadas de colisão já existem.

### Sistemas de jogo
- **Prefabs aninhados com overrides** (cenas-como-prefab de Unity/Godot).

### Scripting e build
- **Export para web (HTML5) e mobile** (hoje só desktop DesktopGL).
- **Profiler / inspector remoto** (Godot remote debugger).
