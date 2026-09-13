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
  `TextRenderer` (fonte pixel embutida, mundo ou HUD), `SceneExit` (transição de fase).
- **Fixo na tela (HUD)**: qualquer objeto (com seus filhos e componentes — sprite, texto,
  barra de vida) pode ser marcado para não andar com a câmera, com a posição virando
  coordenada de tela.
- Barramento de mensagens (sinais de jogo); z-order global; input mapeável por ações
  (teclado + gamepad); play restaurável; hot-reload de assets; transição entre fases.
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
- **Camadas de render** além do z-order por objeto (Godot `CanvasLayer`, Unity Sorting Layers).
- **Parallax nativo** (Godot `ParallaxBackground`).
- **Luzes e shaders 2D** (Godot 2D lights, Unity URP 2D).

### Animação
- **Máquina de estados genérica** com parâmetros/transições/blend além do controlador
  idle/walk/jump atual (Unity Animator, Godot `AnimationTree`).
- **Auto-detecção de frames por transparência** e import Aseprite (o fatiador por grade
  já existe) — Unity Sprite Editor.

### Física
- **Física 2D com corpos rígidos, rotação, joints, raycast e camadas de colisão** —
  Godot/Unity usam Box2D/Chipmunk; no MonoGame há Aether.Physics2D.
- **Colisão direto do tilemap** (Godot TileMap physics, Tiled collision).

### UI
- **Sistema de UI** com botões, âncoras, layout e menus além do texto/HUD atual
  (Godot `Control`, Unity UGUI).

### Sistemas de jogo
- **Salvar/carregar progresso** (save game).
- **Recursos data-driven** para catálogos (Unity `ScriptableObject`, Godot `Resource`).
- **Timers/coroutines** (Godot `Timer`/await).
- **Object pooling**.
- **Prefabs aninhados com overrides** (cenas-como-prefab de Unity/Godot).

### Scripting e build
- **Hot-reload de script durante o play**.
- **Export para web (HTML5) e mobile** (hoje só desktop DesktopGL).
- **Overlay de debug / profiler / inspector remoto** (Godot remote debugger).
- **Localização**.
