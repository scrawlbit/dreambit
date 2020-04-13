﻿using DreamBit.Pipeline.Files;
using DreamBit.Project;

namespace DreamBit.Game.Content
{
    public interface IContentManager
    {
        IContent Load(IProjectFile file);
        IImage Load(IPipelineImage file);
        IFont Load(IPipelineFont file);
    }
}
