using System;

namespace DreamBit.Extension.Components
{
    public interface ISolutionMonitor
    {
        event Action<string> SolutionOpened;
        event Action SolutionClosed;
        event Action<string[]> ItemsAdded;
        event Action<string[]> ItemsRemoved;
    }
}