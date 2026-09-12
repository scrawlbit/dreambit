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
| ✅ **Áudio e partículas** | *Entregue* — `AudioSource` (WAV, toca no play) e `ParticleEmitter`. **Decisão do usuário:** colisão fica **só via plataforma (ledges/PlatformerController)** — sem colisor de tilemap por ora. | 🔴 |
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
| ✅ **Editor em Avalonia** | *Entregue* — `DreamBit.Studio.Avalonia` (net8, cross-platform) reaproveita os ViewModels de `DreamBit.Studio.Core` e desenha a cena com o DrawingContext do Avalonia (dispensa canvas MonoGame). **Paridade ampliada:** inspetor de todos os componentes (Sprite/Animator/Platformer/Áudio/Partículas/Trigger/Follow/Rotator/Script), adicionar/remover componente, assets browser (duplo clique aplica textura), abrir projeto e salvar cena (StorageProvider), console de logs. Compila e roda. |
| ✅ **Export/build do jogo** | *Entregue* — botão Exportar publica o Player + a cena + assets numa pasta portátil. | 🟡 |
| ✅ **Empacotar para distribuição** | *Entregue* — `publish.ps1` gera binários **self-contained** (sem exigir .NET no destino) dos editores WPF/Avalonia e do Player por RID (win/linux/osx), com `-Zip` opcional. Não usa single-file/trim de propósito (quebrariam o Roslyn dos scripts). | 🟢 |
| ✅ **Templates de projeto** | *Entregue* — "Novo projeto" cria a estrutura (assets/ + cena inicial). | 🟢 |

---

## Dívidas técnicas / qualidade

- ✅ **CI (GitHub Actions):** compila `DreamBit.Studio.slnx` e roda os testes a cada push.
- ✅ **Cobertura:** 60 testes no motor (incl. `EngineLog` e hot-reload); casos relevantes dos `Old.*` cobertos pelo motor atual.
- ✅ **Console de logs:** `EngineLog` + `LogViewModel` + painel no rodapé (WPF e Avalonia), com erros de compilação/execução de scripts.
- ✅ **Hot-reload de assets:** editar um PNG/TMX/WAV no disco recarrega no editor ao vivo (`ProjectWatcher.AssetChanged` → invalida `TextureCache`/`SoundCache`).
- ✅ **CVE do AutoMapper (net48):** suprimida com justificativa (`NuGetAuditSuppress`) no projeto legado.
- ✅ **Estilos reutilizáveis:** movidos para `App.xaml` (app-wide). **AvalonDock:** decisão de manter GridSplitter (ver Marco 1).

## Ordem sugerida

1. **Gizmos + inspetor de componentes** (Marco 1) — maior ganho de usabilidade imediato.
2. **Textura real + assets browser** (Marco 2) — torna o editor útil para jogos de verdade.
3. **Tilemap** (Marco 3) — recurso de maior impacto para conteúdo 2D.
4. **CI e cobertura** (transversal) — trava a qualidade antes de crescer.
5. **Avalonia** (Marco 5) — quando quiser editar fora do Windows.

---

## Editor em Avalonia — como ficou

O motor (`DreamBit.Engine`) e o `DreamBit.Player` já eram cross-platform (net8 + DesktopGL); o
editor Avalonia fecha o time. Decisões de implementação:

- **Sem hospedar canvas MonoGame:** em vez de compartilhar um `GraphicsDevice` DesktopGL num
  `OpenGlControlBase`, a cena é desenhada com o **DrawingContext do Avalonia** (`SceneView`) —
  grid, objetos, ledges e seleção em desenho vetorial nativo. Mais simples e sem interop de GL.
  (Texturas reais no canvas do editor continuam sendo desenhadas só no WPF; no Avalonia os sprites
  aparecem como retângulos coloridos. O jogo final renderiza texturas via `DreamBit.Player`.)
- **ViewModels reaproveitados inteiros** via `DreamBit.Studio.Core` (`EditorViewModel`,
  `InspectorViewModel`, `ProjectViewModel`, `LogViewModel`, `SceneInputController`) — muda só o XAML.
- **Diálogos** por `Avalonia.Platform.Storage` (abrir projeto, salvar cena).

Entregue: hierarquia, inspetor completo de componentes, assets browser, console, seleção/arraste/
pan/zoom e play. Pintura de tilemap continua exclusiva do editor WPF (decisão de não priorizar
tilemap por ora).

## Estado final do roadmap

- **Marcos 1, 2, 3 e 4:** concluídos.
- **Marco 5:** Export ✅ · Empacotar/distribuir ✅ (`publish.ps1`) · Templates ✅ · **Editor Avalonia** ✅ (paridade de inspetor/assets/console).
- **Dívidas:** CI ✅, cobertura ✅ (60 testes), console de logs ✅, hot-reload ✅, CVE do AutoMapper ✅, estilos ✅; AvalonDock: decisão de manter GridSplitter.

**Rodada atual (pós-roadmap), na ordem de prioridade combinada:**
1. ✅ Paridade do editor Avalonia (inspetor de componentes + assets browser + salvar/abrir).
2. ✅ Console de logs + erros de script (WPF e Avalonia).
3. ✅ Hot-reload de assets.
4. ✅ Empacotar para distribuição (`publish.ps1`, self-contained).
5. ✅ Reconciliação deste documento.

**Decisão registrada:** colisão permanece **só via plataforma (ledges/PlatformerController)** — sem
colisor/física de tilemap por ora, a pedido do usuário.

Ou seja: **todo o roadmap está entregue**, e a rodada de refinamentos pós-roadmap também. Cada item
tem resolução explícita (entregue, *superseder* ou decisão consciente).
