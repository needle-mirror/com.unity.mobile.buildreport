using Unity.Mobile.BuildReport.Tools;
using System.IO;
using UnityEditor;
using System.Text;

namespace Unity.Mobile.BuildReport
{
    internal static class MobileBuildReportSettingsManager
    {
        internal static MobileBuildReportSettings settings;
        private static readonly string SettingsPath = Path.Combine(CommonTools.ProjectDirectory, "ProjectSettings", "MobileReportSettings.asset");

        static MobileBuildReportSettingsManager()
        {
            LoadSettings();
        }

        internal static void UpdateSettings()
        {
            var settingsString = EditorJsonUtility.ToJson(settings, true);
            if (!string.IsNullOrEmpty(settingsString))
                File.WriteAllText(SettingsPath, settingsString, Encoding.UTF8);
        }

        private static void LoadSettings()
        {
            settings = new MobileBuildReportSettings();
            if (!File.Exists(SettingsPath))
                return;

            var settingsString = File.ReadAllText(SettingsPath, Encoding.UTF8);
            EditorJsonUtility.FromJsonOverwrite(settingsString, settings);
        }
    }
}
