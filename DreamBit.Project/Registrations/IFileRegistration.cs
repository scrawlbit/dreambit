namespace DreamBit.Project.Registrations
{
    public interface IFileRegistration
    {
        string Type { get; }
        string Extension { get; }

        ProjectFile CreateInstance();
    }
}