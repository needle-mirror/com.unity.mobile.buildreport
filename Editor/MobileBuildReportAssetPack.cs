using System;
using UnityEditor.Build.Reporting;

namespace Unity.Mobile.BuildReport
{
    [Serializable]
    internal class MobileBuildReportAssetPack
    {
        internal uint File { get; }
        internal string ShortPath { get; }
        internal ulong Overhead { get; }
        internal MobileBuildReportPackInfo[] PackInfos { get; }

        internal MobileBuildReportAssetPack(PackedAssets packedAssets)
        {
            File = packedAssets.file;
            ShortPath = packedAssets.shortPath;
            Overhead = packedAssets.overhead;

            PackInfos = new MobileBuildReportPackInfo[packedAssets.contents.Length];
            for (var i = 0; i < packedAssets.contents.Length; i++)
                PackInfos[i] = new MobileBuildReportPackInfo(packedAssets.contents[i]);
        }
    }
}
