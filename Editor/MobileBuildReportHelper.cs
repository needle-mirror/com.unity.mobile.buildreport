using Unity.Mobile.BuildReport.Tools;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.IO.Compression;
using System;
using UnityEditor;

namespace Unity.Mobile.BuildReport
{
    /// <summary>
    /// A helper class for generating Mobile BuildReport.
    /// </summary>
    public static class MobileBuildReportHelper
    {
        private static IMobileBuildReportPlatformUtilities s_PlatformUtilities;

        /// <summary>
        /// Generates a new instance of MobileBuildReeport.
        /// </summary>
        /// <param name="buildPath"> The path to the labelled build (.apk or .ipa bundle). </param>
        /// <param name="outputPath"> The path where the report json should be saved. If not specified, the file will be saved in the location specified in ProjectSettings > MobileBuildReport. </param>
        /// <returns> An instance of the MobileBuildReport class that contains the information about the build. </returns>
        /// <exception cref="FileNotFoundException"> Thrown if the build bundle is not found. </exception>
        public static MobileBuildReport Generate(string buildPath, string outputPath = null)
        {
            if (!ValidateBuild(buildPath))
                throw new Exception("The provided build is invalid.");

            var reportHash = GetReportHash(buildPath);
            var buildInfo = LoadReportMetadata(reportHash);
            CollectArchiveData(buildPath, out MobileBuildReport report, buildInfo);

            var reportSavePath = string.IsNullOrEmpty(outputPath) ?
                Path.Combine(MobileBuildReportSettingsManager.settings.ReportLocation, $"{buildInfo.ProductName}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.json") : outputPath;
            report.Save(reportSavePath);
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "Mobile BuildReport saved at {0}", reportSavePath);

            if (MobileBuildReportSettingsManager.settings.ShowReport)
                EditorUtility.RevealInFinder(reportSavePath);

            return report;
        }

        private static bool ValidateBuild(string buildPath)
        {
            if (!File.Exists(buildPath))
                return false;
            try
            {
                using (var archive = ZipFile.OpenRead(buildPath))
                {
                    if (!archive.Entries.Any(x => x.Name == "BundleConfig.pb" || x.Name == "AndroidManifest.xml" || x.Name == "Info.plist"))
                        return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static void CollectArchiveData(string buildPath, out MobileBuildReport report, MobileBuildReportBuildInfo buildInfo)
        {
            report = new MobileBuildReport(buildInfo) { TotalSize = new FileInfo(buildPath).Length };
            report.PlatformData = s_PlatformUtilities.CollectPlatformData(buildPath);

            var temporaryDir = CommonTools.GetTemporaryDirectory();
            using (var archive = ZipFile.OpenRead(buildPath))
            {
                report.Files = new MobileBuildReportFile[archive.Entries.Count(x => x.Length > 0)];
                var i = 0;
                foreach (var file in archive.Entries)
                {
                    if (file.Length == 0)
                        continue;

                    report.Files[i] = new MobileBuildReportFile(file.FullName, file.CompressedLength, file.Length);
                    if (buildInfo.AssetPacks.Any(t => t.ShortPath == file.Name))
                        file.ExtractToFile(Path.Combine(temporaryDir, file.Name));
                    i++;
                }
            }
            s_PlatformUtilities.PrepareSharedAssets(temporaryDir);

            report.Assets = new MobileBuildReportAssets[buildInfo.AssetPacks.Length];
            for (var i = 0; i < buildInfo.AssetPacks.Length; i++)
            {
                report.Assets[i] = new MobileBuildReportAssets(buildInfo.AssetPacks[i])
                {
                    Contents = new MobileBuildReportAssetInfo[buildInfo.AssetPacks[i].PackInfos.Length]
                };
                for (var j = 0; j < buildInfo.AssetPacks[i].PackInfos.Length; j++)
                    report.Assets[i].Contents[j] = new MobileBuildReportAssetInfo(buildInfo.AssetPacks[i].PackInfos[j]);

                if ((buildInfo.Options & BuildOptions.CompressWithLz4) == 0 && (buildInfo.Options & BuildOptions.CompressWithLz4HC) == 0)
                    EstimateAssetSize(buildInfo.AssetPacks[i], report.Assets[i], temporaryDir);
            }

            Directory.Delete(temporaryDir, true);
        }

        private static void EstimateAssetSize(MobileBuildReportAssetPack assetPack, MobileBuildReportAssets mobileAssets, string artifactDir)
        {
            var temporaryZip = $"{Path.Combine(artifactDir, assetPack.ShortPath)}.zip";
            using (var file = File.Open(Path.Combine(artifactDir, assetPack.ShortPath), FileMode.Open))
            {
                using (var archive = ZipFile.Open(temporaryZip, ZipArchiveMode.Create))
                {
                    var overheadPart = new byte[assetPack.Overhead];
                    file.Read(overheadPart, 0, (int)assetPack.Overhead);
                    var overheadPath = Path.Combine(artifactDir, "overhead");
                    File.WriteAllBytes(overheadPath, overheadPart);
                    archive.CreateEntryFromFile(overheadPath, "overhead");
                    File.Delete(overheadPath);
                    for (var i = 0; i < assetPack.PackInfos.Length; i++)
                    {
                        var part = new byte[assetPack.PackInfos[i].PackedSize];
                        file.Seek((long)assetPack.PackInfos[i].Offset, SeekOrigin.Begin);
                        file.Read(part, 0, (int)assetPack.PackInfos[i].PackedSize);
                        var partPath = Path.Combine(artifactDir, $"{i}");
                        File.WriteAllBytes(partPath, part);
                        archive.CreateEntryFromFile(partPath, $"{i}");
                        File.Delete(partPath);
                    }
                }

                using (var archive = ZipFile.OpenRead(temporaryZip))
                {
                    for (var i = 1; i < archive.Entries.Count; i++)
                    {
                        int.TryParse(archive.Entries[i].Name, out var index);
                        mobileAssets.Contents[index] = new MobileBuildReportAssetInfo(assetPack.PackInfos[index])
                        {
                            EstimatedSize = (ulong)archive.Entries[i].CompressedLength
                        };
                    }
                }
            }
        }

        private static MobileBuildReportBuildInfo LoadReportMetadata(string reportHash)
        {
            if (!Directory.Exists(MobileBuildReportSettingsManager.settings.CacheLocation))
                throw new DirectoryNotFoundException($"Cache directory not found at {MobileBuildReportSettingsManager.settings.CacheLocation}. Please generate a build with Mobile BuildReport enabled.");

            var metadataFilePath = Directory.EnumerateFiles(MobileBuildReportSettingsManager.settings.CacheLocation, reportHash).ToList().FirstOrDefault();
            if (string.IsNullOrEmpty(metadataFilePath))
                throw new FileNotFoundException($"Report metadata file not found at {MobileBuildReportSettingsManager.settings.CacheLocation}." +
                    $"Maybe the build was produced on another machine or the cache location has changed?");

            using (var stream = new FileStream(metadataFilePath, FileMode.Open))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(stream) as MobileBuildReportBuildInfo;
            }
        }

        private static string GetReportHash(string buildPath)
        {
            using (var archive = ZipFile.OpenRead(buildPath))
            {
                var hashFile = archive.Entries.FirstOrDefault(x => x.Name == MobileBuildReportSettingsManager.settings.HashFileName);
                if (hashFile == null)
                    throw new FileNotFoundException($"Build report marker with the name {MobileBuildReportSettingsManager.settings.HashFileName} was not found in build archive {buildPath}.");

                using (var reader = new StreamReader(hashFile.Open()))
                    return reader.ReadLine();
            }
        }

        internal static void RegisterPlatformUtilities(IMobileBuildReportPlatformUtilities utilities)
        {
            if (s_PlatformUtilities != null)
                throw new Exception("IMobileBuildReportPlatformUtilities already registered!");

            s_PlatformUtilities = utilities;
        }
    }
}
