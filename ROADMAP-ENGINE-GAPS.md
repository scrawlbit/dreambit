# DreamBit — o que falta na engine (com referências)

Análise do que é interessante adicionar à engine, comparando com engines 2D de mesmo
propósito: **Godot, Unity (2D), GameMaker, Construct 3, Defold e Phaser**. Foco no que
mais destrava jogos reais, na ordem de valor.

## O que a DreamBit já tem
Cena + hierarquia de `Transform`; componentes (Sprite com recorte de atlas, Animator,
Platformer, **colisor sólido AABB + ledges one-way**, Trigger com tags, Audio, Partículas,
Follow, Rotator, **Bone + rig cutout com keyframes de pose, clipes nomeados, easing e
eventos**, Tilemap/Tiled); **barramento de mensagens** (sinais); **scripts C# em runtime
(Roslyn)**; serialização JSON; play restaurável; hot-reload de assets; **transição de fase
(SceneExit)**; export self-contained; editor cross-platform (Avalonia) + Player.

Ou seja: já cobre o "esqueleto" de uma engine data-driven com ECS-leve e scripting. As
lacunas abaixo são o que separa de um Godot/GameMaker.

---

## 🥇 Alto valor (destrava a maioria dos jogos)

| Lacuna | O que é / referência | Por que na DreamBit |
|---|---|---|
| **Câmera como componente** | `Camera2D` com follow, **deadzone**, look-ahead, limites e zoom — Godot `Camera2D`, Unity Cinemachine 2D. | Hoje a câmera é *hardcoded* no Player ("segue o 1º PlatformerController"). Deveria ser um componente configurável na cena. |
| **Z-order / camadas** | Ordem de desenho e camadas de render — Godot `z_index`/`CanvasLayer`, Unity Sorting Layers, Defold render order. | Hoje a ordem = ordem dos objetos. Sem controle de profundidade nem HUD por cima. |
| **Sistema de UI / HUD** | Texto, botões, barras de vida, menus, âncoras — Godot `Control`, Unity UGUI, GameMaker GUI layer. | Nenhum jogo real fecha sem HUD (pontuação, vidas, menu). Falta **renderização de texto** inclusive. |
| **Input mapeável (actions)** | "Jump"/"Move" mapeados a teclas/gamepad/touch — Godot `InputMap`, Unity Input System. | Hoje as teclas estão *fixas* no `PlatformerController`. Sem gamepad, sem remapear. |
| **Fatiador de sprite sheet / import de animação** | Detectar frames (grid ou por transparência), importar Aseprite/JSON — Unity Sprite Editor, Godot `SpriteFrames`, GameMaker. | Ao montar a demo tive que **adivinhar** `FrameWidth/Count`. Um editor de sheet resolveria. |

## 🥈 Médio valor (qualidade e produtividade)

| Lacuna | O que é / referência | Nota |
|---|---|---|
| **Tweening** | Interpolar posição/cor/escala por curva ao longo do tempo — Godot `Tween`, DOTween (Unity). | "Juice" barato; já temos `Scrawlbit.Easing` como base. |
| **Máquina de estados de animação** | Transição idle→walk→jump por condições/blend — Unity Animator, Godot `AnimationTree`. | Temos clipes nomeados; falta o **controlador** que troca entre eles por parâmetros. |
| **Física 2D de verdade (opcional)** | Corpos rígidos, rotação, joints, raycast, camadas/máscaras de colisão — Godot/Unity usam Box2D/Chipmunk; no MonoGame há **Aether.Physics2D**. | Hoje só AABB/ledges (ótimo p/ platformer). Jogos com empilhamento/rotação pedem isso. |
| **Colisão de tilemap** | Tiles sólidos direto do tilemap — Godot TileMap physics, Tiled collision. | Você despriorizou; fica registrado. |
| **Salvar/carregar progresso** | Estado de jogo (save game), não só a cena — comum a todas. | Precisa de um formato de save à parte da cena. |
| **Recursos data-driven** | Assets de dados reutilizáveis — Unity `ScriptableObject`, Godot `Resource`. | Para catálogos (inimigos, itens) sem código. |
| **Timers / coroutines** | Esperar N segundos, sequência de ações — Godot `await`/`Timer`, Unity Coroutines. | Hoje só `Update(dt)`; padrões temporais são manuais. |

## 🥉 Menor valor / mais adiante

- **Luzes e shaders 2D** (Godot 2D lights, Unity URP 2D) — atmosfera/normal maps.
- **Parallax nativo** (Godot `ParallaxBackground`) — hoje dá para fazer manual com camadas.
- **Prefabs aninhados + overrides** (Unity/Godot cenas-como-prefab) — temos prefab simples.
- **Object pooling** (tiros/inimigos) — performance.
- **Hot-reload de script no play** (temos compilação Roslyn; falta recarregar sem reiniciar o play).
- **Export para web (HTML5) e mobile** — Godot/Unity/Phaser exportam; hoje só desktop (DesktopGL).
- **Overlay de debug / profiler / inspector remoto** (Godot remote debugger).
- **Localização** (Godot/Unity translation).
- **Navegação/pathfinding e IA** (Godot `NavigationServer`) — para inimigos mais espertos.

---

## Recomendação (o que eu faria primeiro)
Na ordem, pelo maior ganho por esforço:

1. **Câmera como componente** (follow + deadzone + limites) — tira o hardcode do Player e serve todo jogo.
2. **Z-order/camadas + renderização de texto** — base para HUD.
3. **UI/HUD mínima** (texto, barra, contador) sobre as camadas.
4. **Input mapeável (actions)** + gamepad — desacopla do teclado fixo.
5. **Fatiador de sprite sheet** no editor — corrige o "adivinhar frames".
6. **Tweening** + **máquina de estados de animação** — feel e animação de verdade.
7. **Física 2D via Aether.Physics2D** como opção — abre gêneros além de platformer.

Isso levaria a DreamBit de "esqueleto sólido de engine" para "faz jogos 2D completos"
sem depender de Unity/Godot para os casos comuns.
