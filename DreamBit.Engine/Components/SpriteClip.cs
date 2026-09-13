using System;
using System.Collections.Generic;
using System.Linq;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Um clipe de animação nomeado dentro de uma sprite sheet: uma lista de índices de
    /// frames (na sequência de frames do <see cref="SpriteAnimator"/> — grade ou frames
    /// explícitos detectados) mais taxa e repetição próprias. Equivale ao SpriteFrames do
    /// Godot / clipe de sprite do Unity: várias animações (andar, pular, bater) na mesma folha.
    /// </summary>
    public sealed class SpriteClip
    {
        public SpriteClip() { }

        public SpriteClip(string name, IEnumerable<int> frames, float fps = 8f, bool loop = true)
        {
            Name = name ?? string.Empty;
            Frames = frames?.ToArray() ?? Array.Empty<int>();
            Fps = fps;
            Loop = loop;
        }

        /// <summary>Nome do clipe (ex.: "walk", "jump", "attack").</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Índices dos frames que compõem o clipe, na ordem de reprodução.</summary>
        public int[] Frames { get; set; } = Array.Empty<int>();

        /// <summary>Frames por segundo do clipe.</summary>
        public float Fps { get; set; } = 8f;

        /// <summary>Se o clipe repete ao terminar (senão segura no último e dispara "terminou").</summary>
        public bool Loop { get; set; } = true;

        /// <summary>Cria um clipe a partir de uma faixa contígua de frames [start, start+count).</summary>
        public static SpriteClip Range(string name, int start, int count, float fps = 8f, bool loop = true)
            => new(name, Enumerable.Range(start, Math.Max(0, count)), fps, loop);
    }
}
