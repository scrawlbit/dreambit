# DreamBit — Galeria de Exemplos

Uma cena curta por funcionalidade da engine. Cada exemplo existe de duas formas:

- **`.dbscene`** (nesta pasta) — abra no editor (DreamBit.Studio) e dê **Play**.
- **executável** — rode no Player para ver funcionando na hora.

## Rodar (ver funcionando)

Menu interativo:

```powershell
Examples\run.ps1
```

Direto por nome, ou listando:

```powershell
Examples\run.ps1 05-lights
Examples\run.ps1 -List
```

Sem o launcher, via Player:

```bash
DreamBit.Player\bin\Debug\net8.0\DreamBit.Player.exe --example 05-lights
DreamBit.Player\bin\Debug\net8.0\DreamBit.Player.exe --examples-list
```

## Abrir no editor

Abra qualquer `Examples\*.dbscene` no editor e dê Play. As cenas referenciam os assets em
`Examples\assets\` (spritesheet e tileset gerados proceduralmente, e os stems de áudio).

> Os `.dbscene` guardam caminhos de asset absolutos desta máquina. Para regerá-los em outro
> caminho, rode `DreamBit.Player.exe --examples-save <pasta>`.

## Exemplos

| # | Cena | Funcionalidade |
|---|------|----------------|
| 01 | Sprites e Z-Order | SpriteRenderer, ordem de desenho |
| 02 | Timeline / Tween | PropertyAnimator (posição/rotação/escala) |
| 03 | Animação de Sprite | SpriteAnimator (clipe em loop) |
| 04 | Tilemap | Tilemap + tileset |
| 05 | Luzes 2D + Sombras | Light2D, AmbientLight, ShadowCaster |
| 06 | Física + Joints | Rigidbody2D, colisão, Joint2D (pêndulo) |
| 07 | Câmera: follow + shake | CameraComponent seguindo alvo + tremor por evento |
| 08 | Partículas | ParticleEmitter (cor/tamanho ao longo da vida) |
| 09 | Música Adaptativa | LayeredMusic (bateria sobe com inimigo na tela) |
| 10 | Combate | Health/Hitbox/Hurtbox/SpriteFlash (hit por evento) |
| 11 | Pathfinding / NavChaser | NavChaser perseguindo um alvo |
| 12 | UI | Botão, barra, slider, toggle, texto |
| 13 | Pós-processamento | PostProcess (grayscale via shader) |
