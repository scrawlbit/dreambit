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

*Refinamento entregue:* ✅ undo/redo das edições de transform digitadas no inspetor. *Fica:* trocar os `GridSplitter` por **AvalonDock** (docking flutuante + layout salvável).

## Marco 2 — Conteúdo e assets ✅ CONCLUÍDO

| Funcionalidade | Status |
|---|---|
| **SpriteRenderer com textura real** | ✅ `TextureCache` + `TexturePath`, inspetor, serializado |
| **Assets browser** | ✅ painel de imagens; duplo clique aplica/cria objeto; ao vivo |
| **Pipeline `dotnet mgcb`** | ✅ `ContentManifest` gera `.mgcb`; `ContentBuilder` invoca o MGCB (botão "Conteúdo") |
| **Animação por sprite sheet** | ✅ `SpriteAnimator` (frames, FPS, loop) com `Advance()` testável |

## Marco 3 — Jogabilidade (em andamento)

| Funcionalidade | O que envolve | Esforço |
|---|---|---|
| ✅ **Importar tilemap Tiled (.tmx)** | *Entregue* — `TmxImporter` (CSV/Base64+zlib, chunks, tilesets externos .tsx) + `TilemapRenderer`; **validado nos assets reais (RunNMagic)**. | 🔴 |
| ✅ **Colisão + personagem jogável nas ledges** | *Entregue* — `LedgePhysics` + `PlatformerController` (gravidade, pousa nas ledges one-way, teclado anda/pula) e câmera que segue o personagem no `DreamBit.Player`. | 🟡 |
| ✅ **Editor/pincel de tilemap** | *Entregue* — ferramenta Pincel + paleta dos tilesets; pinta/apaga tiles no canvas (undo por traço), sobre .tmx importado ou tilemap novo a partir de um PNG; serializado inline. | 🔴 |
| ✅ **Áudio e partículas** | *Entregue* — `AudioSource` (WAV, toca no play) e `ParticleEmitter`; falta um colisor genérico (além das ledges). | 🔴 |
| ✅ **Mais componentes** | *Entregue* — Audio, Partículas, Follow, **TriggerZone (colisor por sobreposição/coletável)**; corpo físico avançado fica. | 🔴 |
| ✅ **Referências entre objetos** | *Entregue* — `FollowTarget` com ComboBox de objetos da cena no inspetor. | 🟡 |
| **Scripts C# recarregáveis** | Evoluir o `RotatorBehavior`/`ScriptBehavior` para scripts do usuário compilados via Roslyn, com props no inspetor (como o `ScriptProperty` do `DreamBit.Game`). | 🔴 |

## Marco 4 — Runtime e produtividade

| Funcionalidade | O que envolve | Esforço |
|---|---|---|
| **`DreamBit.Engine.Runtime` completo** | Extrair dos `Old.*` os serviços de runtime (`SceneManager`, `CameraService`, `DrawBatchService`, `ContentManager`+loaders) e adaptá-los ao modelo atual. | 🔴 |
| ✅ **Play mode restaurável** | *Entregue* — snapshot ao dar Play, restaura a cena ao parar. | 🟡 |
| ✅ **Prefabs / copiar-colar** | *Entregue* — salvar/inserir prefab (.dbprefab); Ctrl+C/V e Ctrl+D. | 🟡 |
| ✅ **Busca e conforto** | *Entregue* — busca na hierarquia + multisseleção por caixa (console de logs fica). | 🟡 |

## Marco 5 — Multiplataforma e distribuição

| Funcionalidade | O que envolve | Esforço |
|---|---|---|
| **Editor em Avalonia** | Portar a UI de WPF para **Avalonia + DesktopGL** para editar no Mac/Linux (o motor e o Player já são cross-platform). | 🔴 |
| **Export/build do jogo** | Empacotar o jogo por plataforma (win/mac/linux) a partir do projeto. | 🟡 |
| **Templates de projeto** | "Novo projeto" com estrutura pronta (cenas/assets/pipeline). | 🟢 |

---

## Dívidas técnicas / qualidade

- ✅ **CI (GitHub Actions):** compila `DreamBit.Studio.slnx` e roda os testes a cada push.
- **Cobertura:** portar os casos de `Old.DreamBit.Game.Tests` para ampliar os testes do motor.
- **CVE do AutoMapper (net48):** no projeto legado, migrar as libs puras para `netstandard2.0` e,
  onde possível, aposentar o AutoMapper por mapeamento manual — ou suprimir o aviso com justificativa
  (`NuGetAuditSuppress`, ver §6 do outro roadmap).
- **AvalonDock/temas:** extrair estilos para dicionários de recursos reutilizáveis.

## Ordem sugerida

1. **Gizmos + inspetor de componentes** (Marco 1) — maior ganho de usabilidade imediato.
2. **Textura real + assets browser** (Marco 2) — torna o editor útil para jogos de verdade.
3. **Tilemap** (Marco 3) — recurso de maior impacto para conteúdo 2D.
4. **CI e cobertura** (transversal) — trava a qualidade antes de crescer.
5. **Avalonia** (Marco 5) — quando quiser editar fora do Windows.
