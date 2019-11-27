using Unity.Mobile.BuildReport.Tools;
using System.IO;
using System;
using UnityEngine;

namespace Unity.Mobile.BuildReport
{
    [Serializable]
    internal class MobileBuildReportSettings
    {
        [SerializeField] private bool labelBuildsEnabled;
        [SerializeField] private bool showReport;
        [SerializeField] private string reportLocation;
        [SerializeField] private string relativeReportLocation;
        [SerializeField] private string cacheLocation;
        [SerializeField] private string relativeCacheLocation;
        [SerializeField] private MobileBuildReportSaveOptions reportLocationSetting;
        [SerializeField] private MobileBuildReportSaveOptions cacheLocationSetting;

        internal MobileBuildReportSettings()
        {
            CacheLocation = Path.Combine(CommonTools.ProjectDirectory, CacheRelativeDefault);
            ReportLocation = Path.Combine(CommonTools.ProjectDirectory, ReportRelativeDefault);
        }

        private static string s_ReportHash;
        internal string ReportHash
        {
            get
            {
                if (string.IsNullOrEmpty(s_ReportHash))
                    s_ReportHash = Guid.NewGuid().ToString();
                return s_ReportHash;
            }
            set => s_ReportHash = value;
        }
        internal string HashFileName => "ReportHash.txt";
        internal string CacheRelativeDefault => "Library/MobileBuildReport/Cache";
        internal string ReportRelativeDefault => "Library/MobileBuildReport";

        internal bool LabelBuildsEnabled
        {
            get => labelBuildsEnabled;
            set
            {
                labelBuildsEnabled = value;
                MobileBuildReportSettingsManager.UpdateSettings();
            }
        }

        internal bool ShowReport
        {
            get => showReport;
            set
            {
                showReport = value;
                MobileBuildReportSettingsManager.UpdateSettings();
            }
        }

        internal string BuildPath { get; set; }

        internal string ReportLocation
        {
            get => reportLocation;
            set
            {
                reportLocation = value;
                MobileBuildReportSettingsManager.UpdateSettings();
            }
        }

        internal string RelativeReportLocation
        {
            get => relativeReportLocation;
            set
            {
                relativeReportLocation = value;
                MobileBuildReportSettingsManager.UpdateSettings();
            }
        }

        internal string CacheLocation
        {
            get => cacheLocation;
            set
            {
                cacheLocation = value;
                MobileBuildReportSettingsManager.UpdateSettings();
            }
        }

        internal string RelativeCacheLocation
        {
            get => relativeCacheLocation;
            set
            {
                relativeCacheLocation = value;
                MobileBuildReportSettingsManager.UpdateSettings();
            }
        }

        internal MobileBuildReportSaveOptions ReportLocationSetting
        {
            get => reportLocationSetting;
            set
            {
                reportLocationSetting = value;
                MobileBuildReportSettingsManager.UpdateSettings();
            }
        }

        internal MobileBuildReportSaveOptions CacheLocationSetting
        {
            get => cacheLocationSetting;
            set
            {
                cacheLocationSetting = value;
                MobileBuildReportSettingsManager.UpdateSettings();
            }
        }
    }
}
