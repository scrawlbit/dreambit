using System;

namespace DreamBit.Project.Registrations
{
    public interface IFileRegistration
    {
        string Type { get; }
        string Extension { get; }
        Type ObjectType { get; }

        ProjectFile CreateInstance();
    }
}