using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Comportamento de runtime que gira o objeto durante o play mode. Demonstra o
    /// laço de update do motor (papel dos ScriptBehavior/serviços dos projetos Old.*),
    /// agora dirigido pelo editor standalone sem o Visual Studio.
    /// </summary>
    public sealed class RotatorBehavior : SceneComponent
    {
        private float _speed = 1.2f;

        public override string DisplayName => "Rotator (runtime)";

        /// <summary>Velocidade angular em radianos por segundo.</summary>
        public float Speed
        {
            get => _speed;
            set => Set(ref _speed, value);
        }

        protected internal override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Owner.Transform.Rotation += _speed * delta;
        }
    }
}
