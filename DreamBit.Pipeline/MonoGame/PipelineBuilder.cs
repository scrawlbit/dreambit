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
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            MgcbPath = Path.Combine(programFiles, @"MSBuild\MonoGame\v3.0\Tools\mgcb.exe");
        }

        public void Build(IPipeline pipeline, string outputPath, bool clean)
        {
            string arguments = GenerateArguments(pipeline, outputPath, clean);
            Process process = CreateProcess(pipeline, arguments);

            process.Start();
            process.WaitForExit();
        }

        private static string GenerateArguments(IPipeline pipeline, string outputPath, bool clean)
        {
            string file = Path.GetFileName(pipeline.Path);
            string argument = $"/@:\"{file}\" /outputDir:\"{outputPath}\"";

            if (clean)
                argument += " /clean";

            return argument;
        }
        private static Process CreateProcess(IPipeline pipeline, string arguments)
        {
            return new Process
            {
                StartInfo =
                {
                    FileName = MgcbPath,
                    WorkingDirectory = Path.GetDirectoryName(pipeline.Path),
                    Arguments = arguments,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
        }
    }
}