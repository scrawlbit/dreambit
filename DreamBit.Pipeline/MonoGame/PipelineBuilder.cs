using System;
using System.Diagnostics;
using System.IO;

namespace DreamBit.Pipeline.MonoGame
{
    internal interface IPipelineBuilder
    {
        void Build(IPipeline pipeline, string outputPath, bool clean);
    }

    internal class PipelineBuilder : IPipelineBuilder
    {
        private static readonly string MgcbPath;

        static PipelineBuilder()
        {
            MgcbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                @"MSBuild\MonoGame\v3.0\Tools\mgcb.exe"
            );
        }

        public void Build(IPipeline pipeline, string outputPath, bool clean)
        {
            var arguments = GenerateArguments(pipeline, outputPath, clean);
            var process = CreateProcess(arguments);

            process.Start();
            process.WaitForExit();
        }

        private static string GenerateArguments(IPipeline pipeline, string outputPath, bool clean)
        {
            var argument = $"/@:\"{pipeline.Path}\" /outputDir:\"{outputPath}\"";

            if (clean)
                argument += " /clean";

            return argument;
        }
        private static Process CreateProcess(string arguments)
        {
            return new Process
            {
                StartInfo =
                {
                    FileName = MgcbPath,
                    Arguments = arguments,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
        }
    }
}