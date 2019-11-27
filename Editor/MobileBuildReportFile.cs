using System;
using UnityEngine;

namespace Unity.Mobile.BuildReport
{
    /// <summary>
    /// Contains information about a single file in the application bundle.
    /// </summary>
    [Serializable]
    public class MobileBuildReportFile
    {
        [SerializeField] private string path;
        [SerializeField] private long compressedSize;
        [SerializeField] private long uncompressedSize;

        /// <summary>
        /// The absolute path of the file produced by the build process.
        /// </summary>
        public string Path => path;
        /// <summary>
        /// The compressed size of the file inside the application bundle, in bytes.
        /// </summary>
        public long CompressedSize => compressedSize;
        /// <summary>
        /// The size of the file when uncompressed.
        /// </summary>
        public long UncompressedSize => uncompressedSize;

        internal MobileBuildReportFile(string path, long compressedSize, long uncompressedSize)
        {
            this.path = path;
            this.compressedSize = compressedSize;
            this.uncompressedSize = uncompressedSize;
        }

        /// <summary>
        /// Returns the name of the file.
        /// </summary>
        /// <returns>The path of file in the application bundle.</returns>
        public override string ToString()
        {
            return path;
        }
    }
}
