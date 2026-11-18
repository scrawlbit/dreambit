namespace DreamBit.Engine.Input
{
    /// <summary>
    /// Fonte de comandos por ação ("Jump", "Attack"…) e eixos. Componentes de controle
    /// (plataforma, top-down) leem daqui, então o mesmo personagem pode ser dirigido por um
    /// jogador (<see cref="PlayerInput"/>), pela IA (<see cref="VirtualInput"/>) ou pelo mapa
    /// global (<see cref="Input.Global"/>).
    /// </summary>
    public interface IInputSource
    {
        bool IsDown(string action);
        bool JustPressed(string action);
        bool JustReleased(string action);

        /// <summary>Eixo horizontal em [-1,1].</summary>
        float Horizontal();

        /// <summary>Eixo vertical em [-1,1] (baixo positivo, coordenada de tela).</summary>
        float Vertical();
    }

    /// <summary>Adapta o mapa estático de <see cref="Input"/> (teclado + controle 1).</summary>
    internal sealed class GlobalInputSource : IInputSource
    {
        public bool IsDown(string action) => Input.IsDown(action);
        public bool JustPressed(string action) => Input.JustPressed(action);
        public bool JustReleased(string action) => Input.JustReleased(action);
        public float Horizontal() => Input.Horizontal();
        public float Vertical() => Input.Vertical();
    }
}
