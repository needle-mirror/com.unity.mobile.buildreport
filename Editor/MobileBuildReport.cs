using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.Mobile.BuildReport
{
    /// <summary>
    /// MobileBuildReport class holds the information about the build.
    /// </summary>
    [Serializable]
    public class MobileBuildReport
    {
        [SerializeField] private string platform;
        [SerializeField] private string options;
        [SerializeField] private string outputPath;
        [SerializeField] private long totalSize;
        [SerializeField] private string buildStartedAt;
        [SerializeField] private string buildEndedAt;
        [SerializeField] private string totalTime;
        [SerializeField] private MobileBuildReportPlatformData[] platformData;
        [SerializeField] private MobileBuildReportFile[] files;
        [SerializeField] private MobileBuildReportAssets[] assets;

        /// <summary>
        /// The platform that the build was created for.
        /// </summary>
        public BuildTarget Platform => (BuildTarget)Enum.Parse(typeof(BuildTarget), platform);
        /// <summary>
        /// The BuildOptions used for the build, as passed to BuildPipeline.BuildPlayer.
        /// </summary>
        public BuildOptions Options => string.IsNullOrEmpty(options) ? BuildOptions.None : (BuildOptions)Enum.Parse(typeof(BuildOptions), options);
        /// <summary>
        /// The output path for the build, as provided to BuildPipeline.BuildPlayer.
        /// </summary>
        public string OutputPath => outputPath;
        /// <summary>
        /// The total size of the application bundle on disk.
        /// </summary>
        public long TotalSize
        {
            get => totalSize;
            internal set => totalSize = value;
        }
        /// <summary>
        /// The time the build was started.
        /// </summary>
        public DateTime BuildStartedAt => DateTime.Parse(buildStartedAt);
        /// <summary>
        /// The time the build ended.
        /// </summary>
        public DateTime BuildEndedAt => DateTime.Parse(buildEndedAt);
        /// <summary>
        /// The total time taken by the build process.
        /// </summary>
        public TimeSpan TotalTime => TimeSpan.Parse(totalTime);
        /// <summary>
        /// Contains platform-specific data about the application bundle.
        /// </summary>
        public MobileBuildReportPlatformData[] PlatformData
        {
            get => platformData;
            internal set => platformData = value;
        }
        /// <summary>
        /// An array of all the files in the application bundle.
        /// </summary>
        public MobileBuildReportFile[] Files
        {
            get => files;
            internal set => files = value;
        }
        /// <summary>
        /// An array of all the assets generated by the build process.
        /// </summary>
        public MobileBuildReportAssets[] Assets
        {
            get => assets;
            internal set => assets = value;
        }

        internal MobileBuildReport() {}

        internal MobileBuildReport(MobileBuildReportBuildInfo info)
        {
            platform = info.Platform.ToString();
            options = GetBuildOptionsString(info.Options);
            outputPath = info.OutputPath;
            buildStartedAt = info.BuildStartedAt.ToString("u");
            buildEndedAt = info.BuildEndedAt.ToString("u");
            totalTime = info.TotalTime.ToString("c");
        }

        private static string GetBuildOptionsString(BuildOptions buildOptions)
        {
            var enumNames = Enum.GetNames(typeof(BuildOptions));
            var setValues = new List<string>();
            foreach (var name in enumNames)
            {
                if (!Enum.TryParse(name, out BuildOptions option))
                    continue;

                if ((buildOptions & option) == option && (int)option != 0)
                    setValues.Add(name);
            }
            return string.Join(", ", setValues);
        }

        /// <summary>
        /// Save the report into a JSON file.
        /// </summary>
        /// <param name="path"> The path of the file where the report should be saved. </param>
        public void Save(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.Write(EditorJsonUtility.ToJson(this, true));
            }
        }

        /// <summary>
        /// Load the report from a JSON file.
        /// </summary>
        /// <param name="path"> The path to the JSON file that contains the report data. </param>
        /// <returns> An instance of the MobileBuildReport class that contains information about the build. </returns>
        public static MobileBuildReport Load(string path)
        {
            using (var sr = new StreamReader(path))
            {
                var report = new MobileBuildReport();
                EditorJsonUtility.FromJsonOverwrite(sr.ReadToEnd(), report);
                return report;
            }
        }
    }
}
