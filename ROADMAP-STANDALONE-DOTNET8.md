# DreamBit → Editor standalone .NET 8 (e versão Rider)

Documento de planejamento. Cobre: (1) o que já foi corrigido no repositório, (2) a trava
arquitetural que prende tudo em .NET Framework, (3) o plano de migração para um editor
standalone em .NET 8 reaproveitando o máximo de código, (4) como isso vira a "versão Rider",
(5) funcionalidades que faltam no editor e (6) a vulnerabilidade residual.

---

## 1. O que já foi corrigido e implementado (feito e verificado com build/testes)

**Correções no projeto original (net48):**

| Item | Antes | Agora | Observação |
|---|---|---|---|
| **Newtonsoft.Json** | 12.0.3 | **13.0.3** | Corrige a CVE de DoS GHSA-5crp-9r3c-p9vr. |
| **MonoGame.Framework.WindowsDX** | 3.7.1.189 | **3.8.0.1641** | Última versão compatível com .NET Framework. Compila sem quebrar API. |
| **AutoMapper** | 9.0.0 | **10.1.1** | Teto do net48 (ver §6). |
| **`DreamBit.Project.Tests`** | não compilava | **8 testes verdes** | Mocks e testes reescritos contra a API atual (`InternalsVisibleTo`). |

As bibliotecas de núcleo (`Scrawlbit.MonoGame`, `DreamBit.General/Pipeline/Project/Game`,
`Util`, `Json`) compilam limpas com o MonoGame 3.8.

**Editor standalone .NET 8 implementado** (novos projetos, `DreamBit.Studio.slnx`):

| Projeto | Papel | Status |
|---|---|---|
| `DreamBit.Studio` (net8.0-windows, WPF) | Shell do editor: 3 painéis (Hierarquia/Cena/Inspetor), toolbar, canvas MonoGame ao vivo, seleção/arraste, pan/zoom, snap, play, salvar/abrir | ✅ |
| `DreamBit.Engine` (net8.0-windows) | Motor: Scene/GameObject/Transform/SceneComponent, Camera2D, SceneRenderer, SpriteRenderer, RotatorBehavior, serialização `.dbscene` | ✅ |
| `DreamBit.Engine.Tests` | 12 testes (transform hierárquico, câmera, hierarquia, roundtrip de serialização) | ✅ verdes |

**Ambiente:** a `DreamBit.Extension` (VSIX) não compila nesta máquina porque falta a workload
*"Visual Studio extension development"* (erro `Microsoft.VsSDK.targets não encontrado`). Não é bug
de código; é workload ausente. O editor standalone **não depende** dessa workload.

---

## 2. A trava arquitetural (por que nada passa de net48 hoje)

`DreamBit.Extension` é uma **extensão VSIX clássica**, que roda **in-process dentro do
`devenv.exe`**. O shell do Visual Studio — mesmo o 2026 — é um processo **.NET Framework**, e
toda assembly carregada in-process precisa ser .NET Framework também. Por isso o projeto inteiro
está preso em **net48**. Consequências:

- MonoGame trava na **3.8.0** (a 3.8.1+ exige .NET 6+).
- AutoMapper trava na **10.1.1** (a 11+ virou netstandard2.1, incompatível com net48).
- A **CVE do AutoMapper só é corrigida na 15.1.1+**, que exige .NET 6+ → **insolúvel enquanto for
  VSIX in-process**.

Os três tetos são o mesmo teto. Um **editor standalone** remove todos de uma vez.

---

## 3. Migração para editor standalone .NET 8 — **estratégia híbrida**

### 3.0 Old vs atual: por que combinar os dois

Existem duas implementações do mesmo conceito (`Scene`/`GameObject`/`Transform`), e **nenhum
projeto atual referencia os `Old.*`** — eles são a versão anterior, mantida como referência. Cada
uma amadureceu em uma direção diferente:

- **`DreamBit.Game` (atual)** é um **modelo de editor**: bindável (`NotificationObject` /
  `INotifyPropertyChanged`), coleções que se autogerenciam (`GameObjectCollection`/
  `GameComponentCollection`), transform hierárquico (`ITransform` + `BaseTransform`), serialização
  **desacoplada** em `Serialization/Converters/*`, `ScriptProperty` (reflexão das props do `.cs` no
  inspector) e integração com `DreamBit.Project`/`Pipeline` e com o undo/redo (`DreamBit.General/State`).
  **Porém não tem loop de execução nem testes.**
- **`Old.DreamBit.Game`** é um **motor executável**: `BaseGame : Microsoft.Xna.Framework.Game`
  (game loop), `SceneManager`, `CameraService`, `DrawBatchService`, `ContentManagerService` com
  loaders reais (`FontLoader`/`ImageLoader`/`SceneLoader`) e **uma suíte de testes extensa**
  (`Old.DreamBit.Game.Tests`). Porém o modelo é de runtime puro (sem binding, `[JsonProperty]`
  acoplado, sem consciência de editor).

**Conclusão:** não portar um "por cima" do outro. Usar o **modelo do atual** e trazer do **Old só
o runtime que falta** (loop + serviços) e os **testes** como base para reconstruir a cobertura.

### 3.1 Mapa de reuso (estratégia híbrida)

| Projeto atual | Papel na migração | Destino | Esforço |
|---|---|---|---|
| `DreamBit.Game` (atual) | **Base do modelo/edição** — cena, objetos, componentes, serialização, script props | **Reusar** → `net8.0` | Baixo |
| `Old.DreamBit.Game` → *runtime* | **Extrair só o que falta no atual:** `SceneManager`, `CameraService`/`SceneCamera`, `DrawBatchService`, `DeltaTime`, `ContentManager` + loaders, `BaseGame`/game loop | **Portar seletivo** → `net8.0` | Médio — adaptar ao modelo atual e ao MonoGame 3.8 |
| `Old.DreamBit.Game.Tests` | **Fonte de casos de teste** para reconstruir a suíte (Scene, GameObject, Camera, DrawBatch, Content) | **Adaptar** → net8 | Médio |
| `Old.DreamBit.Game.Edition` | Referência do que era "editável"; conceito já absorvido pelo modelo atual | **Descartar** (consultar) | — |
| `DreamBit.Project` (+ `DreamBit.Project.Tests`) | Modelo de projeto/arquivos/serialização | **Reusar** → `netstandard2.0`; reescrever os testes (§5) | Baixo/Médio |
| `DreamBit.Pipeline` | Pipeline de conteúdo (`.mgcb`, imports de fonte/textura) | **Reusar** → `netstandard2.0`/`net8` | Baixo |
| `Scrawlbit.MonoGame` (helpers) | Math/sprite/texture | **Reusar** → `net8.0` | Baixo |
| `ScrawlBit.MonoGame.Interop` (D3DImage) | Hospedagem WPF do canvas | **Reusar** → `net8.0-windows` | **Já portado** no PoC |
| `Scrawlbit.Util/Json/AutoMapper/SimpleInjector/Presentation` | Infra/MVVM | **Reusar** → `netstandard2.0` | Baixo |
| `DreamBit.Extension/Controls`, `/ViewModels`, `/Converters`, `/Components` | UI/MVVM do editor (WPF puro) | **Portar** → app WPF | Médio |
| `DreamBit.Extension/Commands` | Comandos de menu do VS | **Reescrever** como comandos/atalhos da app | Médio |
| `DreamBit.Extension/Module`, `/Windows` | Tool windows + registro no VS | **Reescrever** como shell + docking | Médio/Alto |
| Integração Solution Explorer / pipeline | Hooks do VS | **Reescrever** como project system + `FileSystemWatcher` | Médio |

O **modelo (atual), o runtime (Old), o domínio, o render e a maior parte da UI são
reaproveitáveis**. O que é genuinamente VS-específico é a *casca* (shell, tool windows, comandos e
a ponte com o Solution Explorer).

### 3.2 Arquitetura alvo

```
DreamBit.Studio.slnx
├─ DreamBit.Studio            (net8.0-windows, WPF)  ← shell do editor + docking
│    └─ Hosting/              ← MonoGameSurface (D3DImage)              [PoC: pronto]
├─ DreamBit.Game              (net8.0)      ← modelo/edição ATUAL (cena, objetos, serialização)
├─ DreamBit.Engine.Runtime    (net8.0)      ← runtime PORTADO do Old (SceneManager, Camera,
│                                              DrawBatch, ContentManager+loaders, game loop)
├─ DreamBit.Project           (netstandard2.0)  ← modelo de projeto
├─ DreamBit.Pipeline          (netstandard2.0)  ← pipeline de conteúdo (.mgcb)
├─ Scrawlbit.*                (netstandard2.0/net8) ← infra reaproveitada
└─ DreamBit.Player            (net8.0)      ← host que roda o jogo (usa Engine.Runtime + MonoGame)
```

- **`DreamBit.Game`** = o que você *edita* (bindável, undo/redo, serialização).
- **`DreamBit.Engine.Runtime`** = o que *executa* (loop, câmera, batch, conteúdo) — extraído do Old.
- O editor usa `DreamBit.Game` para montar a cena e delega o "play mode" ao `Engine.Runtime`,
  renderizando ambos no mesmo `MonoGameSurface`.

Docking: usar **AvalonDock** (Dirkster.AvalonDock) ou **Dock** (Wpf) para recriar as tool windows
(Hierarquia, Inspector, Editor de Cena, Assets) fora do VS.

### 3.3 Plano faseado (híbrido) — status

Legenda: ✅ concluído · 🟡 parcial · ◻ pendente

- **✅ Fase 0 — PoC:** canvas MonoGame ao vivo numa janela WPF .NET 8. → `DreamBit.Studio/`.
- **✅ Fase 1 — Shell:** janela com 3 painéis (Hierarquia | Cena | Inspetor) via `GridSplitter`,
  toolbar (nova/abrir/salvar, novo objeto, rotator, excluir, play, snap, zoom). *AvalonDock fica
  como upgrade para docking flutuante/arrastável.*
- **✅ Fase 2 — Modelo:** `DreamBit.Engine` porta o design do modelo de `DreamBit.Game` para net8
  e renderiza uma `Scene` real no `MonoGameSurface` (grid + objetos + seleção), editável pela
  hierarquia e pelo inspetor (X/Y, rotação, escala).
- **🟡 Fase 3 — Runtime:** o laço de update roda no play mode e o `RotatorBehavior` demonstra o
  runtime dirigido pelo editor. *Falta extrair de `Old.DreamBit.Game` os serviços completos
  (`SceneManager`, `CameraService`, `DrawBatchService`, `ContentManager`+loaders) para um
  `DreamBit.Engine.Runtime` dedicado.*
- **🟡 Fase 4 — Domínio/projeto:** salvar/abrir cena em JSON (`.dbscene`) via `SceneSerializer`.
  *Falta o conceito de "projeto" (múltiplas cenas/assets) e o `FileSystemWatcher`.*
- **🟡 Fase 5 — Editor:** seleção, arraste, pan, zoom, snap ao grid e Delete implementados.
  *Faltam os gizmos visuais de rotação/escala e o undo/redo (`StateManager`).*
- **🟡 Fase 6 — Conteúdo + Play:** play mode funcionando (`Scene.Update` por frame).
  *Falta o pipeline via `dotnet mgcb` (fontes/sprites) e um `DreamBit.Player` separado.*
- **✅ Fase 7 — Testes:** `DreamBit.Engine.Tests` (12 verdes) + `DreamBit.Project.Tests` corrigido
  (8 verdes). *Falta CI (GitHub Actions) e portar os casos de `Old.DreamBit.Game.Tests`.*
- **◻ Fase 8 — Multiplataforma (opcional):** trocar WPF/D3DImage por **Avalonia + MonoGame
  DesktopGL** para Rider no Mac/Linux.

**Resumo:** o editor standalone .NET 8 está **funcional de ponta a ponta** (criar/selecionar/mover
objetos, editar no inspetor, salvar/abrir cena, play mode), com testes verdes. As partes 🟡/◻ são
aprofundamentos (runtime completo, gizmos, undo/redo, pipeline de conteúdo, projeto multi-cena e
multiplataforma) construídos sobre esta base.

---

## 4. Versão Rider

**A versão standalone .NET 8 já é a versão Rider.** Uma vez desacoplado do VSIX, o editor é uma
solução .NET comum — o Rider abre, compila e depura sem nada especial:

1. Abrir `DreamBit.Studio.slnx` no Rider (ele lê `.slnx` nativamente).
2. `Run/Debug` no projeto `DreamBit.Studio` — roda a app WPF diretamente.
3. No **Windows**, o PoC funciona como está (WindowsDX + D3DImage).
4. Para Rider em **Mac/Linux**, seguir a Fase 8 (Avalonia + DesktopGL), já que D3DImage é
   Windows-only.

> Não existe "plugin de Rider" a construir. A confusão comum é achar que seria um plugin do Rider
> (como o VSIX é do VS). Não precisa: o editor standalone **é** o produto, e qualquer IDE .NET o
> abre. Um plugin de IDE só faria sentido se o objetivo fosse reintegrar o editor *dentro* do Rider
> — e aí seria um plugin na plataforma IntelliJ (Kotlin/JVM), um projeto completamente diferente e
> desnecessário para o objetivo.

---

## 5. Funcionalidades que faltam para o editor ficar melhor

Com base no que já existe (seleção, mover/rotacionar/escalar, hierarquia, inspector, undo/redo,
zoom, componentes ImageRenderer/TextRenderer/Script) e no que falta para um editor 2D competitivo:

**Edição de cena**
- Gizmos visuais de transformação com snapping (grid/ângulo) e pivô ajustável.
- Réguas, guias e *snap* a outros objetos; alinhamento/distribuição.
- Seleção múltipla com caixa + operações em lote (alinhar, agrupar).
- Duplicar (Ctrl+D), copiar/colar entre cenas, *prefabs*/templates de objetos reutilizáveis.

**Pipeline e assets**
- Janela de Assets (browser) com preview de sprites/fontes.
- Pipeline MGCB integrado (build incremental, *hot reload* de textura ao salvar o arquivo).
- Import de *sprite sheets* / atlas e animação por frames (timeline simples).

**Componentes e jogabilidade**
- Mais componentes: colisor, corpo físico, animador, áudio, partículas, tilemap.
- Editor de *tilemap* (pintar tiles num grid) — grande alavanca para jogos 2D.
- Campo de referência entre objetos no inspector (arrastar objeto → propriedade).

**Produtividade**
- *Play mode* embutido (rodar a cena e voltar ao estado anterior).
- Console de logs/erros de script; recarregar scripts sem reabrir o projeto.
- Múltiplas cenas abertas em abas; busca na hierarquia.
- Temas (claro/escuro) e layout de docking salvável.

**Qualidade**
- ✅ `DreamBit.Project.Tests` reativado (8 verdes) e `DreamBit.Engine.Tests` criado (12 verdes).
- Portar os casos de `Old.DreamBit.Game.Tests` para ampliar a cobertura do motor.
- CI (GitHub Actions) compilando a solução standalone e rodando os testes.

---

## 6. Vulnerabilidade residual (AutoMapper) e segurança

A CVE **CVE-2026-32933 / GHSA-rvv3-g6hj-g44x** (DoS por recursão sem limite de profundidade no
AutoMapper) **não tem correção para .NET Framework** — as versões corrigidas (15.1.1 / 16.1.1)
exigem .NET 6+. Situação atual:

- No projeto **atual (net48)**: fixamos o AutoMapper na **10.1.1** (máximo compatível). O aviso
  `NU1903` permanece e **não há como eliminá-lo via versão** sob net48.
- **Risco prático é baixo aqui:** a exploração exige mapear um grafo de objetos hostil e
  profundamente aninhado (25.000+ níveis). O AutoMapper do DreamBit mapeia **modelos locais do
  editor** (cena/objetos criados pelo próprio desenvolvedor), não entrada de rede não confiável.
- **Correção definitiva = migração para .NET 8** (este roadmap): lá o AutoMapper vai para 15.1.1+
  e a CVE some. Alternativa sem migrar: substituir o wrapper de AutoMapper por mapeamento manual
  (as classes mapeadas são poucas).
- Se quiser silenciar o aviso de forma documentada no projeto atual, adicione ao
  `Scrawlbit.AutoMapper.csproj` (decisão consciente, não correção):
  ```xml
  <ItemGroup>
    <!-- CVE-2026-32933 sem fix para net48; risco baixo: mapeia só modelos locais do editor. -->
    <NuGetAuditSuppress Include="https://github.com/advisories/GHSA-rvv3-g6hj-g44x" />
  </ItemGroup>
  ```
