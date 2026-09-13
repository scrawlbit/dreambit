namespace DreamBit.Engine.Components
{
    /// <summary>Foco de UI: qual campo de texto recebe o teclado. Um por vez.</summary>
    public static class UiFocus
    {
        public static UiTextField? Current { get; private set; }

        public static void Set(UiTextField? field) => Current = field;
        public static bool Has(UiTextField field) => ReferenceEquals(Current, field);
        public static void Clear() => Current = null;
    }
}
