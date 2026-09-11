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

*Refinamentos opcionais que ficam para depois:* undo/redo das edições digitadas no inspetor
(agrupadas por foco) e trocar os `GridSplitter` por **AvalonDock** (docking flutuante + layout salvável).

## Marco 2 — Conteúdo e assets (em andamento)

| Funcionalidade | O que envolve | Esforço |
|---|---|---|
| ✅ **SpriteRenderer com textura real** | *Entregue* — `TextureCache` + `TexturePath`, campo no inspetor, serializado. | 🟡 |
| ✅ **Assets browser** | *Entregue* — painel de imagens do projeto; duplo clique aplica ao sprite/cria objeto; atualiza ao vivo. | 🟡 |
| **Pipeline `dotnet mgcb`** | Integrar o Content Pipeline do MonoGame (build incremental) e **hot reload** de textura ao salvar. Aproveita `DreamBit.Pipeline`. | 🔴 |
| **Sprite sheets / animação** | Import de atlas, recorte de frames e uma timeline simples de animação por frames. | 🔴 |

> Falta o miolo pesado do Marco 2 (pipeline MGCB e animação por frames), ambos 🔴.

## Marco 3 — Jogabilidade

| Funcionalidade | O que envolve | Esforço |
|---|---|---|
| **Editor de tilemap** | Pintar tiles num grid — grande alavanca para jogos 2D. Novo componente `Tilemap` + ferramenta de pincel. | 🔴 |
| **Mais componentes** | Colisor, corpo físico, animador, áudio, partículas — cada um com Draw/Update e inspetor. | 🔴 |
| **Referências entre objetos** | Propriedade do inspetor que aceita outro `GameObject` (arrastar da hierarquia). | 🟡 |
| **Scripts C# recarregáveis** | Evoluir o `RotatorBehavior`/`ScriptBehavior` para scripts do usuário compilados via Roslyn, com props no inspetor (como o `ScriptProperty` do `DreamBit.Game`). | 🔴 |

## Marco 4 — Runtime e produtividade

| Funcionalidade | O que envolve | Esforço |
|---|---|---|
| **`DreamBit.Engine.Runtime` completo** | Extrair dos `Old.*` os serviços de runtime (`SceneManager`, `CameraService`, `DrawBatchService`, `ContentManager`+loaders) e adaptá-los ao modelo atual. | 🔴 |
| **Play mode restaurável** | Rodar a cena no editor e voltar ao estado anterior (snapshot/rollback), sem sair para o Player. | 🟡 |
| **Prefabs / duplicar / copiar-colar** | Templates reutilizáveis de objetos; `Ctrl+D`, copiar/colar entre cenas. | 🟡 |
| **Busca e conforto** | Busca na hierarquia; console de logs/erros; multiseleção com caixa. | 🟡 |

## Marco 5 — Multiplataforma e distribuição

| Funcionalidade | O que envolve | Esforço |
|---|---|---|
| **Editor em Avalonia** | Portar a UI de WPF para **Avalonia + DesktopGL** para editar no Mac/Linux (o motor e o Player já são cross-platform). | 🔴 |
| **Export/build do jogo** | Empacotar o jogo por plataforma (win/mac/linux) a partir do projeto. | 🟡 |
| **Templates de projeto** | "Novo projeto" com estrutura pronta (cenas/assets/pipeline). | 🟢 |

---

## Dívidas técnicas / qualidade

- **CI (GitHub Actions):** compilar `DreamBit.Studio.slnx` e rodar os testes a cada push.
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
