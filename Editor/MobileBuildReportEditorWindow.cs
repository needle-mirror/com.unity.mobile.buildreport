#if UNITY_ANDROID || UNITY_IOS
using Unity.Mobile.BuildReport.Tools;
using UnityEditor;
using UnityEngine;

namespace Unity.Mobile.BuildReport.UI
{
    internal class MobileBuildReportEditorWindow : EditorWindow
    {
        private const string buildSelectTooltip = "Select the compiled Android APK or iOS IPA file. It must be built with Mobile BuildReport enabled in ProjectSettings.";

        [MenuItem("Mobile BuildReport/Show")]
        private static void Init()
        {
            var window = (MobileBuildReportEditorWindow)GetWindow(typeof(MobileBuildReportEditorWindow));
            window.titleContent = new GUIContent("Mobile BuildReport");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            MobileBuildReportSettingsManager.settings.BuildPath = EditorGUILayout.TextField(new GUIContent("Build location", buildSelectTooltip), MobileBuildReportSettingsManager.settings.BuildPath);
            if (GUILayout.Button(new GUIContent("Select...", buildSelectTooltip), GUILayout.Width(80), GUILayout.Height(18)))
            {
                var newLocation = EditorUtility.OpenFilePanel("Select build", CommonTools.ProjectDirectory, "");
                MobileBuildReportSettingsManager.settings.BuildPath = !string.IsNullOrEmpty(newLocation) ? newLocation : MobileBuildReportSettingsManager.settings.BuildPath;
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Generate report"))
                MobileBuildReportHelper.Generate(MobileBuildReportSettingsManager.settings.BuildPath);

            EditorGUILayout.Space();
            if (!MobileBuildReportSettingsManager.settings.LabelBuildsEnabled)
                EditorGUILayout.HelpBox("Build labelling is disabled. Mobile BuildReport tool will not work for builds generated without this option. " +
                    "To enable build labelling, go to Edit > Project Settings > Mobile BuildReport", MessageType.Warning, true);
        }
    }
}
#endif
