#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Unity.Mobile.BuildReport.Tools
{
    internal static class CommonTools
    {
        internal static string ProjectDirectory => Directory.GetParent(Application.dataPath).FullName;

        internal static void WriteTextFile(string text, string path)
        {
            using (var writer = File.CreateText(path))
            {
                writer.WriteLine(text);
            }
        }

        internal static string GetTemporaryDirectory()
        {
            return Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;
        }

        internal static string RunProcessAndGetOutput(string executable, string arguments, out string error, out int exitCode)
        {
            using (var p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                error = p.StandardError.ReadToEnd();
                p.WaitForExit();

                exitCode = p.ExitCode;
                return output;
            }
        }
    }
}
#endif
