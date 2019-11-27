using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Mobile.BuildReport
{
    /// <summary>
    /// Contains platform-specific data about the build.
    /// </summary>
    [Serializable]
    public class MobileBuildReportPlatformData : ISerializationCallbackReceiver
    {
        [SerializeField] private string architecture;
        [SerializeField] private long downloadSize;
        [SerializeField] private string[] segments;

        /// <summary>
        /// A target architecture enabled in the build.
        /// </summary>
        public string Architecture
        {
            get => architecture;
            internal set => architecture = value;
        }
        /// <summary>
        /// The estimated download size of the bundle, when downloading the app from the platform's stores.
        /// </summary>
        public long DownloadSize
        {
            get => downloadSize;
            internal set => downloadSize = value;
        }
        /// <summary>
        /// Contains executable segment names and sizes. Only available when generating the report on macOS.
        /// </summary>
        public Dictionary<string, long> Segments { get; internal set; }

        public void OnAfterDeserialize()
        {
            if (segments.Length == 0)
                return;

            Segments = new Dictionary<string, long>();
            foreach (var entry in segments)
            {
                var keyValue = entry.Split(new string[] { ": " }, StringSplitOptions.None);
                Segments.Add(keyValue[0], long.Parse(keyValue[1]));
            }
        }

        public void OnBeforeSerialize()
        {
            if (Segments == null)
                return;

            segments = new string[Segments.Count];
            var i = 0;
            foreach (var entry  in Segments)
                segments[i++] = $"{entry.Key}: {entry.Value}";
        }
    }
}
