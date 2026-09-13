using DreamBit.Engine.Elements;

namespace DreamBit.Engine.Messaging
{
    /// <summary>Uma mensagem nomeada disparada na cena (evento de jogo), com quem enviou.</summary>
    public readonly record struct GameMessage(string Name, GameObject? Sender);

    /// <summary>Componente que reage a mensagens da cena (barramento de eventos).</summary>
    public interface IMessageReceiver
    {
        void OnMessage(GameMessage message);
    }
}
