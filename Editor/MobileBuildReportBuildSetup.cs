#if UNITY_ANDROID || UNITY_IOS
using Unity.Mobile.BuildReport.Tools;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Unity.Mobile.BuildReport
{
    internal class MobileBuildReportBuildSetup : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            MobileBuildReportSettingsManager.settings.ReportHash = null;
        }

        public void OnPostprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            if (!MobileBuildReportSettingsManager.settings.LabelBuildsEnabled)
                return;

            if (report.summary.result == BuildResult.Failed || report.summary.result == BuildResult.Cancelled ||
                (report.summary.platform != BuildTarget.Android && report.summary.platform != BuildTarget.iOS))
                return;

            var assetData = new MobileBuildReportBuildInfo(report.summary, report.packedAssets);
            if (!Directory.Exists(MobileBuildReportSettingsManager.settings.CacheLocation))
                Directory.CreateDirectory(MobileBuildReportSettingsManager.settings.CacheLocation);

            using (var stream = new FileStream(Path.Combine(MobileBuildReportSettingsManager.settings.CacheLocation, MobileBuildReportSettingsManager.settings.ReportHash), FileMode.Create))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, assetData);
            }

            if (report.summary.platform != BuildTarget.iOS)
                return;

            var hashPath = Path.Combine(report.summary.outputPath, "Data", MobileBuildReportSettingsManager.settings.HashFileName);
            CommonTools.WriteTextFile(MobileBuildReportSettingsManager.settings.ReportHash, hashPath);
        }
    }
}
#endif
