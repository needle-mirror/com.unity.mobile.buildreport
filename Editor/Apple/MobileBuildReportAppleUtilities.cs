#if UNITY_IOS
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using Unity.Mobile.BuildReport.Tools;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;

namespace Unity.Mobile.BuildReport.Apple
{
    internal class MobileBuildReportAppleUtilities : IMobileBuildReportPlatformUtilities
    {
        private const string unityFrameworkRelativePath = "Frameworks/UnityFramework.framework/UnityFramework";
        private const string size = "/usr/bin/size";
        private const string file = "/usr/bin/file";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            MobileBuildReportHelper.RegisterPlatformUtilities(new MobileBuildReportAppleUtilities());
        }

        public MobileBuildReportPlatformData[] CollectPlatformData(string buildPath)
        {
            var temporaryDir = CommonTools.GetTemporaryDirectory();
            var frameworkFile = Path.Combine(temporaryDir, "UnityFramework");
            long appSizeNoFramework;
            using (var archive = ZipFile.OpenRead(buildPath))
            {
                var unityFramework = archive.Entries.FirstOrDefault(x => x.FullName.EndsWith(unityFrameworkRelativePath, StringComparison.InvariantCulture));
                if (unityFramework == null)
                    throw new FileNotFoundException($"Framework {unityFrameworkRelativePath} not found in the app archive!");

                unityFramework.ExtractToFile(frameworkFile);
                appSizeNoFramework = new FileInfo(buildPath).Length - unityFramework.CompressedLength;
            }

            var foundArchs = new List<string>();
            {
                var archOutput = CommonTools.RunProcessAndGetOutput(file, $"-b {frameworkFile}", out string error, out int exitCode);
                if (exitCode != 0)
                {
                    Debug.LogWarning($"Failed to collect UnityFramework data with command: {size} -m {frameworkFile}");
                    return null;
                }

                using (var reader = new StringReader(archOutput))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var archString = line.Substring(line.LastIndexOf(' ') + 1);
                        if (archString.StartsWith("arm", StringComparison.InvariantCulture))
                            foundArchs.Add(archString.Replace("_", string.Empty));
                    }
                }
            }

            var appleData = new MobileBuildReportPlatformData[foundArchs.Count];
            {
                for (var i = 0; i < foundArchs.Count; i++)
                {
                    var sizeArgs = $"-m -arch {foundArchs[i]} {frameworkFile}";
                    var sizeOutput = CommonTools.RunProcessAndGetOutput(size, sizeArgs, out string error, out int exitCode);
                    if (exitCode != 0)
                    {
                        Debug.LogWarning($"Failed to collect UnityFramework data with command: {size} {sizeArgs}");
                        return null;
                    }

                    appleData[i] = new MobileBuildReportPlatformData { Architecture = foundArchs[i] };
                    using (var reader = new StringReader(sizeOutput))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.StartsWith("Segment __", StringComparison.InvariantCulture))
                            {
                                if (appleData[i].Segments == null)
                                    appleData[i].Segments = new Dictionary<string, long>();

                                var segmentName = line.Substring(8, line.IndexOf(':') - 8);
                                var segmentSize = long.Parse(line.Substring(line.LastIndexOf(' ') + 1));
                                appleData[i].Segments.Add(segmentName, segmentSize);
                            }
                        }
                    }

                    var textSize = appleData[i].Segments.ContainsKey("__TEXT") ? appleData[i].Segments["__TEXT"] : 0;
                    var dataSize = appleData[i].Segments.ContainsKey("__DATA") ? appleData[i].Segments["__DATA"] : 0;
                    appleData[i].DownloadSize = appSizeNoFramework + textSize + dataSize / 5;
                }
            }

            Directory.Delete(temporaryDir, true);

            return appleData;
        }

        public void PrepareSharedAssets(string assetsPath)
        {
        }
    }
}
#endif
