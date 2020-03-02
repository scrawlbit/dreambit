namespace DreamBit.Game.Drawing
{
    internal class DrawBatchDefinitionRequest
    {
        public DrawBatchDefinitionRequest(DrawBatchDefinition definition)
        {
            Definition = definition;
            RequestCount = 1;
        }

        public DrawBatchDefinition Definition { get; }
        public int RequestCount { get; set; }
    }
}