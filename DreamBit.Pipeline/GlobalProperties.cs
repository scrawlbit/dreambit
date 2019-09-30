namespace DreamBit.Pipeline
{
    public interface IGlobalProperties
    {
        string OutputDir { get; set; }
        string IntermediateDir { get; set; }
        Platform Platform { get; set; }
        string Config { get; set; }
        Profile Profile { get; set; }
        bool Compress { get; set; }

        void Reset();
    }

    internal class GlobalProperties : IGlobalProperties
    {
        internal GlobalProperties()
        {
            Reset();
        }

        public string OutputDir { get; set; }
        public string IntermediateDir { get; set; }
        public Platform Platform { get; set; }
        public string Config { get; set; }
        public Profile Profile { get; set; }
        public bool Compress { get; set; }

        public void Reset()
        {
            OutputDir = "bin";
            IntermediateDir = "obj";
            Platform = Platform.Windows;
            Config = "";
            Profile = Profile.Reach;
            Compress = false;
        }
    }
}