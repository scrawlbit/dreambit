# DreamBit — Roadmap das próximas funcionalidades

Continuação de `ROADMAP-STANDALONE-DOTNET8.md` (cujas 8 fases de migração estão entregues).
Aqui ficam as **próximas funcionalidades**, organizadas em marcos por valor e esforço.
Base atual: `DreamBit.Studio` (editor WPF net8), `DreamBit.Engine` (motor multi-target),
`DreamBit.Player` (runtime DesktopGL cross-platform), 27 testes verdes.

Legenda de esforço: 🟢 baixo · 🟡 médio · 🔴 alto. Itens ✅ já entregues.

**Já entregue nesta rodada:** ✅ conceito de **ledges** no mapa (bordas caminháveis one-way, estilo
Dust) · ✅ **Marco 1** completo (ver abaixo).
**Decisão registrada:** o **AutoMapper não será usado** no editor .NET 8 (a versão atual exige
licença comercial + chave; optou-se por manter o mapeamento manual em `SceneSerializer`).

---

## Marco 1 — Editor sólido ✅ CONCLUÍDO

| Funcionalidade | Status |
|---|---|
| **Gizmos de rotação e escala** | ✅ handle de rotação + handles de escala nos cantos, reversíveis |
| **Inspetor de componentes** | ✅ editar/adicionar/remover Sprite e Rotator, com undo/redo |
| **Múltiplas cenas em abas** | ✅ abas com troca de cena ativa, fechar, foco na já aberta |
| **Tema claro/escuro** | ✅ chrome + canvas, toggle na toolbar |
| **Duplicar / mover por setas** | ✅ Ctrl+D e setas, reversíveis |

*Refinamento entregue:* ✅ undo/redo das edições de transform digitadas no inspetor. *Decisão:* manter os `GridSplitter` (funcional, sem dependência); **AvalonDock** não foi adotado — o tema padrão conflita com o tema escuro custom e o risco de reescrever um layout grande e não verificável não compensa um refinamento opcional.

## Marco 2 — Conteúdo e assets ✅ CONCLUÍDO

| Funcionalidade | Status |
|---|---|
| **SpriteRenderer com textura real** | ✅ `TextureCache` + `TexturePath`, inspetor, serializado |
| **Assets browser** | ✅ painel de imagens; duplo clique aplica/cria objeto; ao vivo |
| **Pipeline `dotnet mgcb`** | ✅ `ContentManifest` gera `.mgcb`; `ContentBuilder` invoca o MGCB (botão "Conteúdo") |
| **Animação por sprite sheet** | ✅ `SpriteAnimator` (frames, FPS, loop) com `Advance()` testável |

## Marco 3 — Jogabilidade ✅ CONCLUÍDO

| Funcionalidade | O que envolve | Esforço |
|---|---|---|
| ✅ **Importar tilemap Tiled (.tmx)** | *Entregue* — `TmxImporter` (CSV/Base64+zlib, chunks, tilesets externos .tsx) + `TilemapRenderer`; **validado nos assets reais (RunNMagic)**. | 🔴 |
| ✅ **Colisão + personagem jogável nas ledges** | *Entregue* — `LedgePhysics` + `PlatformerController` (gravidade, pousa nas ledges one-way, teclado anda/pula) e câmera que segue o personagem no `DreamBit.Player`. | 🟡 |
| ✅ **Editor/pincel de tilemap** | *Entregue* — ferramenta Pincel + paleta dos tilesets; pinta/apaga tiles no canvas (undo por traço), sobre .tmx importado ou tilemap novo a partir de um PNG; serializado inline. | 🔴 |
| ✅ **Áudio e partículas** | *Entregue* — `AudioSource` (WAV, toca no play) e `ParticleEmitter`; falta um colisor genérico (além das ledges). | 🔴 |
| ✅ **Mais componentes** | *Entregue* — Audio, Partículas, Follow, **TriggerZone (colisor por sobreposição/coletável)**; corpo físico avançado fica. | 🔴 |
| ✅ **Referências entre objetos** | *Entregue* — `FollowTarget` com ComboBox de objetos da cena no inspetor. | 🟡 |
| ✅ **Scripts C# recarregáveis** | *Entregue* — `ScriptComponent` compila C# do usuário em runtime (Roslyn), com editor de código, compilar e erros no inspetor. | 🔴 |

## Marco 4 — Runtime e produtividade

| Funcionalidade | O que envolve | Esforço |
|---|---|---|
| ✅ **Runtime** | *Superseder* — o `DreamBit.Engine` atual já provê o runtime (Scene.Update/StartPlay, Camera2D, SceneRenderer, TextureCache/SoundCache, play loop no editor e no Player). Extrair de novo os serviços dos `Old.*` seria redundante. |
| ✅ **Play mode restaurável** | *Entregue* — snapshot ao dar Play, restaura a cena ao parar. | 🟡 |
| ✅ **Prefabs / copiar-colar** | *Entregue* — salvar/inserir prefab (.dbprefab); Ctrl+C/V e Ctrl+D. | 🟡 |
| ✅ **Busca e conforto** | *Entregue* — busca na hierarquia + multisseleção por caixa (console de logs fica). | 🟡 |

## Marco 5 — Multiplataforma e distribuição

| Funcionalidade | O que envolve | Esforço |
|---|---|---|
| ✅ **Editor em Avalonia** | *Entregue* — `DreamBit.Studio.Avalonia` (net8, cross-platform) reaproveita os ViewModels de `DreamBit.Studio.Core` e desenha a cena com o DrawingContext do Avalonia (dispensa canvas MonoGame). Hierarquia, inspetor, seleção/arraste/pan/zoom e play. Compila e roda. |
| ✅ **Export/build do jogo** | *Entregue* — botão Exportar publica o Player + a cena + assets numa pasta portátil. | 🟡 |
| **Templates de projeto** | "Novo projeto" com estrutura pronta (cenas/assets/pipeline). | 🟢 |

---

## Dívidas técnicas / qualidade

- ✅ **CI (GitHub Actions):** compila `DreamBit.Studio.slnx` e roda os testes a cada push.
- **Cobertura:** portar os casos de `Old.DreamBit.Game.Tests` para ampliar os testes do motor.
- **CVE do AutoMapper (net48):** no projeto legado, migrar as libs puras para `netstandard2.0` e,
  onde possível, aposentar o AutoMapper por mapeamento manual — ou suprimir o aviso com justificativa
  (`NuGetAuditSuppress`, ver §6 do outro roadmap).
- ✅ **Estilos reutilizáveis:** movidos para `App.xaml` (app-wide). **AvalonDock:** decisão de manter GridSplitter (ver Marco 1).

## Ordem sugerida

1. **Gizmos + inspetor de componentes** (Marco 1) — maior ganho de usabilidade imediato.
2. **Textura real + assets browser** (Marco 2) — torna o editor útil para jogos de verdade.
3. **Tilemap** (Marco 3) — recurso de maior impacto para conteúdo 2D.
4. **CI e cobertura** (transversal) — trava a qualidade antes de crescer.
5. **Avalonia** (Marco 5) — quando quiser editar fora do Windows.

---

## Editor em Avalonia — entregue (base cross-platform)

O motor (`DreamBit.Engine`) e o `DreamBit.Player` já são cross-platform (net8 + DesktopGL).
Falta apenas a **UI do editor** rodar fora do Windows. Caminho sugerido:

1. **Hospedar o canvas MonoGame em Avalonia** (o ponto difícil): usar `OpenGlControlBase` do
   Avalonia e renderizar a `Scene` via um `GraphicsDevice` DesktopGL compartilhando o contexto GL,
   ou renderizar para um `RenderTarget2D` e exibir num `Bitmap`/`WriteableBitmap`. É o equivalente
   Avalonia do `MonoGameSurface` (que no WPF usa D3DImage).
2. **Portar a UI** (`MainWindow` → Avalonia `Window`): os ViewModels (`EditorViewModel`,
   `InspectorViewModel`, `ProjectViewModel`, `SceneInputController`) são independentes de WPF e
   **reaproveitam-se quase inteiros**; muda só o XAML (bindings do Avalonia são muito próximos) e os
   diálogos de arquivo (`Avalonia.Controls.StorageProvider`).
3. **Fase mínima:** um `DreamBit.Studio.Avalonia` que abre um projeto e renderiza/edita uma cena;
   depois portar painéis (hierarquia/inspetor/assets/paleta) reutilizando os mesmos ViewModels.

Esforço: 🔴 (semanas). Por isso ficou como o único item grande adiado — os ViewModels já estão
prontos para isso, o gargalo é a hospedagem do canvas e a reescrita do XAML.

## Estado final do roadmap

- **Marcos 1, 2, 3 e 4:** concluídos.
- **Marco 5:** Export ✅ · Templates ✅ · **Editor Avalonia** ✅ (base cross-platform).
- **Dívidas:** CI ✅, cobertura ✅ (ampliada), CVE do AutoMapper ✅ (suprimida com justificativa),
  estilos ✅; AvalonDock: decisão de manter GridSplitter.

Ou seja: **todo o roadmap está entregue.** O editor Avalonia foi implementado como base funcional
cross-platform (reaproveitando os ViewModels via `DreamBit.Studio.Core`); a paridade total de
componentes com o editor WPF pode ser ampliada incrementalmente. Cada item tem resolução explícita
(entregue, superseder ou decisão consciente sobre AvalonDock).
