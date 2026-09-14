# DemoAssets — fase de demonstração "Floresta"

Cena de exemplo (`forest-demo.dbscene`) que exercita quase toda a engine com assets reais.

## Assets necessários (não versionados)

A arte é de **Dust: An Elysian Tail** (© Humble Hearts) — comercial, **não redistribuída**
aqui. Para rodar o demo, coloque nesta pasta:

| Arquivo | O que é | Origem |
|---|---|---|
| `hero.png` | Sprite sheet do herói (3900×3600) | `sprites_01_1.png` |
| `forest-atlas.png` | Atlas de objetos da floresta (4096×4096) | `... Forest Objects.png` |
| `ground.png` | Tile de chão 96×96 (recorte do atlas) | derivado do atlas |

O herói é animado por **grade** direto de `hero.png` (13×12 células de 300×300), sem recorte —
preserva tamanho e transparência de cada quadro.

Versionados (metadados, sem arte): `atlas-parts.json` (caixas dos objetos do atlas) e
`forest-demo.dbscene` (a cena).

## Rodar

Do diretório raiz do projeto (para os caminhos relativos resolverem):

```bash
dotnet run --project DreamBit.Player/DreamBit.Player.csproj -c Debug -- DemoAssets/forest-demo.dbscene
```

Controles: setas/A-D andam, Espaço/W pulam, Enter/E ataca (dispara o clipe de ataque).

## O que a cena exercita

Clipes de sprite (andar/pular/bater/parado) + flip e controlador de estado; plataforma
sobre tilemap sólido; câmera com follow/deadzone; parallax de fundo; decoração por recorte
de atlas com camadas de render; perseguição por pathfinding (NavChaser); física rígida
(caixas empilhando); timeline de propriedade (cristal flutuante); trigger/mensagem de meta
(chegar à tenda); áudio espacial + buses; e HUD (barra de vida, título, slider de volume).

O smoke test `ForestDemoSmokeTests` monta e valida tudo isso sem tela e regrava este
`.dbscene`.
