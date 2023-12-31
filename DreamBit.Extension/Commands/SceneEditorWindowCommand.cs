﻿using DreamBit.Extension.Components;
using DreamBit.Extension.Windows;

namespace DreamBit.Extension.Commands
{
    internal interface ISceneEditorWindowCommand : IToolCommand { }
    internal sealed class SceneEditorWindowCommand : ToolCommand, ISceneEditorWindowCommand
    {
        private readonly IPackageBridge _package;

        public SceneEditorWindowCommand(IPackageBridge package)
        {
            _package = package;
        }

        protected override int Id => DreamBitPackage.Guids.SceneEditorWindowCommand;

        public override void Execute()
        {
            _package.ShowWindow<SceneEditorWindow>();
        }
    }
}