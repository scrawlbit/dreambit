namespace DreamBit.Game.Components
{
    internal interface ICameraService
    {
        ICamera CurrentCamera { get; set; }

        void UpdateSceneCamera();
    }
}