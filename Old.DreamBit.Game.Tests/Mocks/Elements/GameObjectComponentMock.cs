using DreamBit.Game.Elements.Components;

namespace DreamBit.Game.Tests.Mocks.Elements
{
    public class GameObjectComponentMock : GameObjectComponent
    {
        public bool StartCalled { get; set; }
        public bool UpdateCalled { get; set; }
        public bool PostUpdateCalled { get; set; }
        public bool DrawCalled { get; set; }

        protected internal override void Start()
        {
            StartCalled = true;
        }
        protected internal override void Update()
        {
            UpdateCalled = true;
        }
        protected internal override void PostUpdate()
        {
            PostUpdateCalled = true;
        }
        protected internal override void Draw()
        {
            DrawCalled = true;
        }
    }
}