using System;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Messaging;

namespace DreamBit.Engine.Components
{
    /// <summary>Reação embutida do MessageListener ao receber a mensagem.</summary>
    public enum MessageReaction { None, Hide, Show, ToggleVisible, Destroy }

    /// <summary>
    /// Escuta uma mensagem nomeada da cena (barramento de eventos) e reage: aplica uma
    /// reação embutida no dono (esconder/mostrar/destruir) e dispara o evento C#
    /// <see cref="Received"/> para lógica adicional (scripts). Ex.: um botão dispara
    /// "abrirPorta" e a porta tem um MessageListener("abrirPorta", Hide).
    /// </summary>
    public sealed class MessageListener : SceneComponent, IMessageReceiver
    {
        private string _message = "evento";
        private MessageReaction _reaction = MessageReaction.None;

        public override string DisplayName => "Message Listener";

        /// <summary>Nome da mensagem que este componente escuta.</summary>
        public string Message
        {
            get => _message;
            set => Set(ref _message, value ?? string.Empty);
        }

        /// <summary>Reação embutida aplicada ao dono quando a mensagem chega.</summary>
        public MessageReaction Reaction
        {
            get => _reaction;
            set => Set(ref _reaction, value);
        }

        /// <summary>Disparado quando a mensagem escutada chega (para lógica adicional).</summary>
        public event Action<GameMessage>? Received;

        public void OnMessage(GameMessage message)
        {
            if (!string.Equals(message.Name, _message, StringComparison.OrdinalIgnoreCase))
                return;

            switch (_reaction)
            {
                case MessageReaction.Hide: Owner.IsVisible = false; break;
                case MessageReaction.Show: Owner.IsVisible = true; break;
                case MessageReaction.ToggleVisible: Owner.IsVisible = !Owner.IsVisible; break;
                case MessageReaction.Destroy: Owner.Scene?.Remove(Owner); break;
            }

            Received?.Invoke(message);
        }
    }
}
