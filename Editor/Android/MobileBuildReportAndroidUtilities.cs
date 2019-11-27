#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Unity.Mobile.BuildReport.Tools;
using UnityEditor;
using UnityEditor.Android;
using Debug = UnityEngine.Debug;

namespace Unity.Mobile.BuildReport.Android
{
    public class MobileBuildReportAndroidUtilities : IMobileBuildReportPlatformUtilities
    {
        private const string objdump = "/usr/bin/objdump";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            MobileBuildReportHelper.RegisterPlatformUtilities(new MobileBuildReportAndroidUtilities());
        }

        public MobileBuildReportPlatformData[] CollectPlatformData(string buildPath)
        {
            var downloadSize = EstimateDownloadSize(buildPath);
            var temporaryDir = CommonTools.GetTemporaryDirectory();
            var libraryFiles = new List<string>();
            using (var archive = ZipFile.OpenRead(buildPath))
            {
                foreach (var file in archive.Entries)
                {
                    if (file.Name == "libunity.so")
                    {
                        var parent = file.FullName.Replace("/libunity.so", string.Empty);
                        var architecture = parent.Substring(parent.LastIndexOf('/') + 1);
                        var targetFile = Path.Combine(temporaryDir, architecture);
                        file.ExtractToFile(targetFile);
                        libraryFiles.Add(targetFile);
                    }
                }
            }

            var androidData = new MobileBuildReportPlatformData[libraryFiles.Count];
            for (int i = 0; i < libraryFiles.Count; i++)
            {
                androidData[i] = new MobileBuildReportPlatformData
                {
                    Architecture = Path.GetFileName(libraryFiles[i]),
                    DownloadSize = downloadSize
                };
#if UNITY_EDITOR_OSX
                var objdumpArgs = $"-section-headers {libraryFiles[i]}";
                var objdumpOutput = CommonTools.RunProcessAndGetOutput(objdump, objdumpArgs, out string error, out int exitCode);
                if (exitCode != 0)
                {
                    Debug.LogWarning($"Failed to collect libunity.so data with command: {objdump} {objdumpArgs}");
                    return null;
                }

                using (var reader = new StringReader(objdumpOutput))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var lineParts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (lineParts.Length == 0 || lineParts.Length != 5 || !int.TryParse(lineParts[0], out int n))
                            continue;

                        if (androidData[i].Segments == null)
                            androidData[i].Segments = new Dictionary<string, long>();

                        if (!androidData[i].Segments.ContainsKey(lineParts[4]))
                            androidData[i].Segments.Add(lineParts[4], 0);

                        androidData[i].Segments[lineParts[4]] += Convert.ToInt64(lineParts[2], 16);
                    }
                }
#endif
            }

            Directory.Delete(temporaryDir, true);

            return androidData;
        }

        public long EstimateDownloadSize(string buildPath)
        {
            string apkAnalyzerPath;
            string javaPath;
            if (Environment.GetEnvironmentVariable("UNITY_THISISABUILDMACHINE") == "1")
            {
                javaPath = Environment.GetEnvironmentVariable("JAVA_HOME");
                if (!Directory.Exists(javaPath))
                    throw new Exception($"JAVA_HOME environment variable not pointing to a valid Java directory. Current value: {javaPath}");

                var sdkEnv = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
                if (!Directory.Exists(sdkEnv))
                    throw new Exception($"ANDROID_SDK_ROOT environment variable not pointing to a valid Android SDK directory. Current value: {sdkEnv}");
                apkAnalyzerPath = Path.Combine(sdkEnv, "tools", "bin", "apkanalyzer");
            }
            else
            {
                if (!Directory.Exists(AndroidExternalToolsSettings.sdkRootPath))
                    throw new Exception("Could not retrieve Android SDK location. Please set it up in Editor Preferences.");
                apkAnalyzerPath = Path.Combine(AndroidExternalToolsSettings.sdkRootPath, "tools", "bin", "apkanalyzer");

                if (!Directory.Exists(AndroidExternalToolsSettings.jdkRootPath))
                    throw new Exception("Could not retrieve JDK location. Please set it up in Editor Preferences.");
                javaPath = AndroidExternalToolsSettings.jdkRootPath;
            }

#if UNITY_EDITOR_WIN
            apkAnalyzerPath += ".bat";
            if (!File.Exists(apkAnalyzerPath))
                return RunApkAnalyzerInternal(AndroidExternalToolsSettings.jdkRootPath, AndroidExternalToolsSettings.sdkRootPath, buildPath);
#endif
            if (!File.Exists(apkAnalyzerPath))
                throw new Exception($"apkanalyzer doesn't exist at: {apkAnalyzerPath}.");

            var apkAnalyzerArgs = $"apk download-size \"{buildPath}\"";
            var apkAnalyzerOutput = CommonTools.RunProcessAndGetOutput(apkAnalyzerPath, apkAnalyzerArgs, out string error, out int exitCode);
            if (exitCode != 0)
                throw new Exception($"apkanalyzer failed. Error message:\n{error}");

            if (long.TryParse(apkAnalyzerOutput, out long result))
                return result;

            throw new Exception($"apkanalyzer failed to estimate the apk size. Output:\n{apkAnalyzerOutput}");
        }

        public static long RunApkAnalyzerInternal(string javaPath, string sdkPath, string apkPath)
        {
            var appHome = $"\"{Path.Combine(sdkPath, "tools")}\"";
            var defaultJvmOpts = $"-Dcom.android.sdklib.toolsdir={appHome}";
            var classPath = $"{appHome}\\lib\\dvlib-26.0.0-dev.jar;{appHome}\\lib\\util-2.2.1.jar;{appHome}\\lib\\jimfs-1.1.jar;{appHome}\\lib\\" +
                $"annotations-13.0.jar;{appHome}\\lib\\ddmlib-26.0.0-dev.jar;{appHome}\\lib\\repository-26.0.0-dev.jar;{appHome}\\lib\\" +
                $"sdk-common-26.0.0-dev.jar;{appHome}\\lib\\kotlin-stdlib-1.1.3-2.jar;{appHome}\\lib\\protobuf-java-3.0.0.jar;{appHome}\\lib\\" +
                $"apkanalyzer-cli.jar;{appHome}\\lib\\gson-2.3.jar;{appHome}\\lib\\httpcore-4.2.5.jar;{appHome}\\lib\\dexlib2-2.2.1.jar;{appHome}\\" +
                $"lib\\commons-compress-1.12.jar;{appHome}\\lib\\generator.jar;{appHome}\\lib\\error_prone_annotations-2.0.18.jar;{appHome}\\lib\\" +
                $"commons-codec-1.6.jar;{appHome}\\lib\\kxml2-2.3.0.jar;{appHome}\\lib\\httpmime-4.1.jar;{appHome}\\lib\\annotations-12.0.jar;{appHome}\\" +
                $"lib\\bcpkix-jdk15on-1.56.jar;{appHome}\\lib\\jsr305-3.0.0.jar;{appHome}\\lib\\explainer.jar;{appHome}\\lib\\builder-model-3.0.0-dev.jar;" +
                $"{appHome}\\lib\\baksmali-2.2.1.jar;{appHome}\\lib\\j2objc-annotations-1.1.jar;{appHome}\\lib\\layoutlib-api-26.0.0-dev.jar;{appHome}\\" +
                $"lib\\jcommander-1.64.jar;{appHome}\\lib\\commons-logging-1.1.1.jar;{appHome}\\lib\\annotations-26.0.0-dev.jar;{appHome}\\lib\\" +
                $"builder-test-api-3.0.0-dev.jar;{appHome}\\lib\\animal-sniffer-annotations-1.14.jar;{appHome}\\lib\\bcprov-jdk15on-1.56.jar;{appHome}\\lib\\" +
                $"httpclient-4.2.6.jar;{appHome}\\lib\\common-26.0.0-dev.jar;{appHome}\\lib\\jopt-simple-4.9.jar;{appHome}\\lib\\sdklib-26.0.0-dev.jar;{appHome}\\" +
                $"lib\\apkanalyzer.jar;{appHome}\\lib\\shared.jar;{appHome}\\lib\\binary-resources.jar;{appHome}\\lib\\guava-22.0.jar";
            var appArgs = $"apk download-size \"{apkPath}\"";
            var apkAnalyzerArgs = $"{defaultJvmOpts} -classpath {classPath} com.android.tools.apk.analyzer.ApkAnalyzerCli {appArgs}";
            var javaExec = Path.Combine(javaPath, "bin", "java.exe");

            var apkAnalyzerOutput = CommonTools.RunProcessAndGetOutput(javaExec, apkAnalyzerArgs, out string error, out int exitCode);

            if (exitCode != 0)
                throw new Exception($"Error message:\n{error}");

            if (long.TryParse(apkAnalyzerOutput, out long result))
                return result;

            throw new Exception($"apkanalyzer failed to estimate the apk size. Output:\n{apkAnalyzerOutput}");
        }

        public void PrepareSharedAssets(string assetsPath)
        {
            foreach (var split in Directory.GetFiles(assetsPath, "sharedassets*.split*"))
            {
                var target = Path.Combine(assetsPath, split.Substring(0, split.LastIndexOf(".split", StringComparison.InvariantCulture)));
                if (File.Exists(target))
                {
                    File.Delete(split);
                    continue;
                }

#if UNITY_EDITOR_WIN
                var shell = "cmd.exe";
                var args = $"/C copy /B {target}* {target}";
#else
                var shell = "bash";
                var scriptPath = Path.GetFullPath("Packages/com.unity.mobile.buildreport/Editor/Apple/combine_splits.sh");
                var args = $"{scriptPath} {target}";
#endif
                CommonTools.RunProcessAndGetOutput(shell, args, out string error, out int exitCode);

                if (exitCode != 0)
                    throw new Exception($"Failed to combine sharedassets with command: '{shell} {args}'");

                File.Delete(split);
            }
        }
    }
}
#endif
