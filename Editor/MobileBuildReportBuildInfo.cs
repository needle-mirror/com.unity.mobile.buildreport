using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Unity.Mobile.BuildReport
{
    [Serializable]
    internal class MobileBuildReportBuildInfo
    {
        internal BuildTarget Platform { get; }
        internal BuildOptions Options { get; }
        internal string OutputPath { get; }
        internal string ProductName { get; }
        internal DateTime BuildStartedAt { get; }
        internal DateTime BuildEndedAt { get; }
        internal TimeSpan TotalTime { get; }
        internal MobileBuildReportAssetPack[] AssetPacks { get; }

        internal MobileBuildReportBuildInfo(BuildSummary summary, IReadOnlyList<PackedAssets> packedAssets)
        {
            Platform = summary.platform;
            Options = summary.options;
            OutputPath = summary.outputPath;
            ProductName = Application.productName;
            BuildStartedAt = summary.buildStartedAt;
            BuildEndedAt = DateTime.UtcNow;
            TotalTime = BuildEndedAt - BuildStartedAt;

            AssetPacks = new MobileBuildReportAssetPack[packedAssets.Count];
            for (var i = 0; i < packedAssets.Count; i++)
                AssetPacks[i] = new MobileBuildReportAssetPack(packedAssets[i]);
        }
    }
}
