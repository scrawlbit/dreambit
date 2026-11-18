namespace DreamBit.Engine.Input
{
    /// <summary>
    /// Componente dirigido por uma fonte de input: <see cref="PlayerIndex"/> escolhe o jogador
    /// (-1 = mapa global) e <see cref="InputOverride"/> permite trocar em runtime (IA, cutscene,
    /// troca de personagem). Outros componentes do mesmo objeto (ex.: animação) leem
    /// <see cref="InputSource"/> para reagir aos mesmos comandos.
    /// </summary>
    public interface IInputControlled
    {
        int PlayerIndex { get; set; }
        IInputSource? InputOverride { get; set; }
        IInputSource InputSource { get; }
    }
}
