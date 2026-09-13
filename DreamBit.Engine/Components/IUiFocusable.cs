using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Controle de UI que pode receber foco (navegação por teclado/gamepad e realce). Botão,
    /// toggle, slider e campo de texto implementam. O <see cref="UiNavigator"/> move o foco
    /// entre eles espacialmente e os ativa.
    /// </summary>
    public interface IUiFocusable
    {
        /// <summary>Pode ser focado agora (visível/habilitado).</summary>
        bool Focusable { get; }

        /// <summary>Retângulo do controle em coordenadas de tela (para navegação e realce).</summary>
        Rectangle FocusRect { get; }

        /// <summary>Confirmar (Enter/A): clicar o botão, alternar o toggle, etc.</summary>
        void Activate();

        /// <summary>Ajuste por seta esquerda/direita (dir = -1/+1); usado pelo slider.</summary>
        void Nudge(int dir);
    }
}
