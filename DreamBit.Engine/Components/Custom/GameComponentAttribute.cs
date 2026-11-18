using System;

namespace DreamBit.Engine.Components.Custom
{
    /// <summary>
    /// Marca um <see cref="Elements.SceneComponent"/> de jogo como componente customizado: ele
    /// aparece no "Adicionar componente" do editor, é salvo na cena (.dbscene) e editado no
    /// inspector pelas suas propriedades públicas (get + set).
    /// </summary>
    /// <example>
    /// <code>
    /// [GameComponent("Lothar/Corrupção")]
    /// public sealed class Corrupcao : SceneComponent
    /// {
    ///     public override string DisplayName => "Corrupção";
    ///     [Tooltip("Pontos totais (Vida + Corrupção)")] public float Total { get; set; } = 200;
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class GameComponentAttribute : Attribute
    {
        public GameComponentAttribute(string? menuPath = null) => MenuPath = menuPath;

        /// <summary>Caminho no menu do editor ("Categoria/Nome"). Null = nome da classe.</summary>
        public string? MenuPath { get; }

        /// <summary>Identificador gravado na cena. Padrão: nome completo do tipo. Defina-o para
        /// poder renomear/mover a classe sem quebrar cenas salvas.</summary>
        public string? Id { get; set; }
    }

    /// <summary>Esconde uma propriedade pública do inspector e da serialização.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HideInInspectorAttribute : Attribute { }

    /// <summary>Texto de ajuda mostrado no inspector para a propriedade.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TooltipAttribute : Attribute
    {
        public TooltipAttribute(string text) => Text = text;
        public string Text { get; }
    }

    /// <summary>Limites sugeridos para campos numéricos no inspector (o valor é limitado ao editar).</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RangeAttribute : Attribute
    {
        public RangeAttribute(float min, float max) { Min = min; Max = max; }
        public float Min { get; }
        public float Max { get; }
    }

    /// <summary>Marca uma propriedade string como caminho de asset (o inspector oferece "Procurar...").</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AssetPathAttribute : Attribute
    {
        public AssetPathAttribute(string filter = "*.*") => Filter = filter;
        /// <summary>Filtro de extensões, ex.: "*.png;*.jpg".</summary>
        public string Filter { get; }
    }
}
