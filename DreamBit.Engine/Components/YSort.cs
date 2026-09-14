using DreamBit.Engine.Elements;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Ordenação por profundidade em jogos top-down: objetos mais "abaixo" na tela (maior Y de
    /// mundo) são desenhados na frente. Marque personagens/objetos com este componente e a cena
    /// usa a posição Y (mais <see cref="Offset"/>, para o pivô nos "pés") no lugar do SortOrder.
    /// </summary>
    public sealed class YSort : SceneComponent
    {
        private float _offset;

        public override string DisplayName => "Y Sort";

        /// <summary>Deslocamento somado ao Y para o ponto de ordenação (ex.: base/pés do sprite).</summary>
        public float Offset { get => _offset; set => Set(ref _offset, value); }

        /// <summary>Chave de ordenação (Y de mundo + offset).</summary>
        public float SortKey => Owner.Transform.WorldPosition.Y + _offset;
    }
}
