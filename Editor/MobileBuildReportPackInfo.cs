using System;
using UnityEditor.Build.Reporting;

namespace Unity.Mobile.BuildReport
{
    [Serializable]
    internal class MobileBuildReportPackInfo
    {
        internal long Id { get; }
        internal string AssetPath { get; }
        internal string GuidString { get; }
        internal string TypeString { get; }
        internal ulong PackedSize { get; }
        internal ulong Offset { get; }

        internal MobileBuildReportPackInfo(PackedAssetInfo info)
        {
            Id = info.id;
            AssetPath = info.sourceAssetPath;
            GuidString = info.sourceAssetGUID.ToString();
            TypeString = info.type.ToString();
            PackedSize = info.packedSize;
            Offset = info.offset;
        }
    }
}
