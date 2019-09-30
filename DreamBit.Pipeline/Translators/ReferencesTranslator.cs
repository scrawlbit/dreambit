using System.Text;

namespace DreamBit.Pipeline.Translators
{
    internal interface IReferencesTranslator : ITranslator { }

    internal class ReferencesTranslator : IReferencesTranslator
    {
        public void Read(string text)
        {
        }
        public string Write()
        {
            var builder = new StringBuilder();

            builder.AppendLine("#-------------------------------- References --------------------------------#");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}