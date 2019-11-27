#if UNITY_ANDROID
using Unity.Mobile.BuildReport.Tools;
using System.IO;
using UnityEditor;
using UnityEditor.Android;

namespace Unity.Mobile.BuildReport.Android
{
    [InitializeOnLoad]
    internal class MobileBuildReportGradlePreprocess : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 0;
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            if (!MobileBuildReportSettingsManager.settings.LabelBuildsEnabled)
                return;

            var filePath = Path.Combine(CommonTools.ProjectDirectory, path, "src", "main", "assets", "bin", "Data", MobileBuildReportSettingsManager.settings.HashFileName);
            CommonTools.WriteTextFile(MobileBuildReportSettingsManager.settings.ReportHash, filePath);
        }
    }
}
#endif
