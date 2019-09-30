namespace DreamBit.Pipeline.Translators
{
    internal interface IObjectTranslator
    {
        bool TryRead(string text, out object value);
        bool TryWrite(object value, out string text);
    }
}