namespace DreamBit.Pipeline.Translators
{
    internal interface ITranslator
    {
        void Read(string text);
        string Write();
    }
}