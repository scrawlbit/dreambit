namespace DreamBit.Project.Registrations
{
    public interface IProjectRegistration
    {
        string Type { get; }
        string Extension { get; }

        ProjectFile CreateInstance();
    }
}