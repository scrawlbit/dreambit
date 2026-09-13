namespace DreamBit.Engine.Components
{
    /// <summary>Foco de UI: qual controle recebe o teclado/confirmação. Um por vez.</summary>
    public static class UiFocus
    {
        public static IUiFocusable? Current { get; private set; }

        public static void Set(IUiFocusable? control) => Current = control;
        public static bool Has(IUiFocusable control) => ReferenceEquals(Current, control);
        public static void Clear() => Current = null;
    }
}
