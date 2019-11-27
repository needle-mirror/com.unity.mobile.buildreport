using System;
using UnityEngine;

namespace Unity.Mobile.BuildReport
{
    /// <summary>
    /// Holds information about the Asset packs included in the application bundle.
    /// </summary>
    [Serializable]
    public class MobileBuildReportAssets
    {
        [SerializeField] private string dataPath;
        [SerializeField] private ulong overhead;
        [SerializeField] private MobileBuildReportAssetInfo[] contents;

        /// <summary>
        /// The file path to the Asset package, relative to the Data folder of the build.
        /// </summary>
        public string DataPath => dataPath;
        /// <summary>
        /// The header size of the packed Asset file.
        /// </summary>
        public ulong Overhead => overhead;
        /// <summary>
        /// Holds information about the Assets that are included in the pack.
        /// </summary>
        public MobileBuildReportAssetInfo[] Contents
        {
            get => contents;
            internal set => contents = value;
        }

        internal MobileBuildReportAssets(MobileBuildReportAssetPack pack)
        {
            dataPath = pack.ShortPath;
            overhead = pack.Overhead;
        }

        /// <summary>
        /// Returns the name of the Asset package file.
        /// </summary>
        /// <returns>The name of the Asset package file in the application bundle.</returns>
        public override string ToString()
        {
            return dataPath;
        }
    }
}
