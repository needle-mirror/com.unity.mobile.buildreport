using System;
using UnityEngine;

namespace Unity.Mobile.BuildReport
{
    /// <summary>
    /// Holds information about the individual assets included in the build.
    /// </summary>
    [Serializable]
    public class MobileBuildReportAssetInfo
    {
        [SerializeField] private long id;
        [SerializeField] private string sourceAssetPath;
        [SerializeField] private string sourceAssetGuid;
        [SerializeField] private ulong size;
        [SerializeField] private ulong estimatedSize;
        [SerializeField] private ulong offset;
        [SerializeField] private string type;

        /// <summary>
        /// The unique identifier of the packed Asset.
        /// </summary>
        public long Id => id;
        /// <summary>
        /// The file path to the source Asset that the build process used to generate the package Asset, relative to the Project directory.
        /// </summary>
        public string SourceAssetPath => sourceAssetPath;
        /// <summary>
        /// The Global Unique Identifier (GUID) of the source Asset that the build process used to generate the packed Asset.
        /// </summary>
        public string SourceAssetGUID => sourceAssetGuid;
        /// <summary>
        /// The size of the packed Asset, when it is uncompressed from the application bundle.
        /// </summary>
        public ulong Size => size;
        /// <summary>
        /// The estimated size of the Asset inside the application bundle.
        /// </summary>
        /// <remarks>
        /// The Asset size is only estimated with the Default build compression option enabled.
        /// </remarks>
        public ulong EstimatedSize
        {
            get => estimatedSize;
            set => estimatedSize = value;
        }
        /// <summary>
        /// The offset in a PackedAssets file that indicates the beginning of the packed Asset.
        /// </summary>
        public ulong Offset => offset;
        /// <summary>
        /// The type of source Asset that the build process used to generate the package Asset, such as image, Mesh or audio types.
        /// </summary>
        public string Type => type;

        internal MobileBuildReportAssetInfo(MobileBuildReportPackInfo info)
        {
            id = info.Id;
            sourceAssetPath = info.AssetPath;
            sourceAssetGuid = info.GuidString;
            size = info.PackedSize;
            offset = info.Offset;
            type = info.TypeString;
        }

        /// <summary>
        /// Returns the path of the source Asset that was packed.
        /// </summary>
        /// <returns>Returns the path of the source Asset that was packed.</returns>
        public override string ToString()
        {
            return sourceAssetPath;
        }
    }
}
