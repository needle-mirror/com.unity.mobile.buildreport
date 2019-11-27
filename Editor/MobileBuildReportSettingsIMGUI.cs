using Unity.Mobile.BuildReport.Tools;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.Mobile.BuildReport
{
    internal static class MobileBuildReportSettingsIMGUI
    {
        [SettingsProvider]
        public static SettingsProvider CreateMobileBuildReportSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Mobile BuildReport", SettingsScope.Project)
            {
                guiHandler = (searchContext) =>
                {
                    EditorGUILayout.Space();

                    var labelBuildsEnabledToggle = EditorGUILayout.Toggle(new GUIContent("Build labelling",
                        "When this option is enabled, any new Android and iOS builds will be labelled and their report metadata will be saved in the provided cache location. " +
                        "This enables Mobile BuildReport generation for new builds."), MobileBuildReportSettingsManager.settings.LabelBuildsEnabled);
                    if (labelBuildsEnabledToggle != MobileBuildReportSettingsManager.settings.LabelBuildsEnabled)
                        MobileBuildReportSettingsManager.settings.LabelBuildsEnabled = labelBuildsEnabledToggle;

                    var showReportToggle = EditorGUILayout.Toggle(new GUIContent("Show report",
                        "Enable this to show the report in file browser."), MobileBuildReportSettingsManager.settings.ShowReport);
                    if (showReportToggle != MobileBuildReportSettingsManager.settings.ShowReport)
                        MobileBuildReportSettingsManager.settings.ShowReport = showReportToggle;

                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();

                    MobileBuildReportSettingsManager.settings.ReportLocationSetting = (MobileBuildReportSaveOptions)EditorGUILayout.EnumPopup(new GUIContent("Report location",
                        $"Select where new reports are saved. Default option will save the reports relative to the project, in " +
                        $"{MobileBuildReportSettingsManager.settings.ReportRelativeDefault}"), MobileBuildReportSettingsManager.settings.ReportLocationSetting, GUILayout.Width(250));

                    if ((MobileBuildReportSettingsManager.settings.ReportLocationSetting & MobileBuildReportSaveOptions.Custom) == MobileBuildReportSaveOptions.Custom)
                    {
                        EditorGUILayout.BeginHorizontal();
                        var reportLocation = EditorGUILayout.TextField(MobileBuildReportSettingsManager.settings.ReportLocation);
                        if (reportLocation != MobileBuildReportSettingsManager.settings.ReportLocation)
                            MobileBuildReportSettingsManager.settings.ReportLocation = reportLocation;
                        var saveSelectionButton = GUILayout.Button("Select...", GUILayout.Width(100), GUILayout.Height(18));
                        if (saveSelectionButton)
                        {
                            var newLocation = EditorUtility.OpenFolderPanel("Select report save location", "", "");
                            MobileBuildReportSettingsManager.settings.ReportLocation = !string.IsNullOrEmpty(newLocation) ? newLocation : MobileBuildReportSettingsManager.settings.ReportLocation;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else if ((MobileBuildReportSettingsManager.settings.ReportLocationSetting & MobileBuildReportSaveOptions.Relative) == MobileBuildReportSaveOptions.Relative)
                    {
                        EditorGUILayout.BeginHorizontal();
                        MobileBuildReportSettingsManager.settings.RelativeReportLocation = EditorGUILayout.TextField(MobileBuildReportSettingsManager.settings.RelativeReportLocation);
                        MobileBuildReportSettingsManager.settings.ReportLocation = Path.Combine(CommonTools.ProjectDirectory, MobileBuildReportSettingsManager.settings.RelativeReportLocation);
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        MobileBuildReportSettingsManager.settings.ReportLocation = Path.Combine(CommonTools.ProjectDirectory, MobileBuildReportSettingsManager.settings.ReportRelativeDefault);
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.TextField("");
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.SelectableLabel(MobileBuildReportSettingsManager.settings.ReportLocation, GUILayout.Height(14));

                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    MobileBuildReportSettingsManager.settings.CacheLocationSetting = (MobileBuildReportSaveOptions)EditorGUILayout.EnumPopup(new GUIContent("Cache location",
                        $"Select where report metadata is cached. Default option will store the cache relative to the project, in " +
                        $"{MobileBuildReportSettingsManager.settings.CacheRelativeDefault}"), MobileBuildReportSettingsManager.settings.CacheLocationSetting, GUILayout.Width(250));
                    if ((MobileBuildReportSettingsManager.settings.CacheLocationSetting & MobileBuildReportSaveOptions.Custom) == MobileBuildReportSaveOptions.Custom)
                    {
                        EditorGUILayout.BeginHorizontal();
                        MobileBuildReportSettingsManager.settings.CacheLocation = EditorGUILayout.TextField(MobileBuildReportSettingsManager.settings.CacheLocation);
                        var cacheSelectionButton = GUILayout.Button("Select...", GUILayout.Width(100), GUILayout.Height(18));
                        if (cacheSelectionButton)
                        {
                            var newLocation = EditorUtility.OpenFolderPanel("Select report cache location", "", "");
                            MobileBuildReportSettingsManager.settings.CacheLocation = !string.IsNullOrEmpty(newLocation) ? newLocation : MobileBuildReportSettingsManager.settings.CacheLocation;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else if ((MobileBuildReportSettingsManager.settings.CacheLocationSetting & MobileBuildReportSaveOptions.Relative) == MobileBuildReportSaveOptions.Relative)
                    {
                        EditorGUILayout.BeginHorizontal();
                        MobileBuildReportSettingsManager.settings.RelativeCacheLocation = EditorGUILayout.TextField(string.IsNullOrEmpty(MobileBuildReportSettingsManager.settings.RelativeCacheLocation) ?
                            string.Empty : MobileBuildReportSettingsManager.settings.RelativeCacheLocation);
                        MobileBuildReportSettingsManager.settings.CacheLocation = Path.Combine(CommonTools.ProjectDirectory, MobileBuildReportSettingsManager.settings.RelativeCacheLocation);
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        MobileBuildReportSettingsManager.settings.CacheLocation = Path.Combine(CommonTools.ProjectDirectory, MobileBuildReportSettingsManager.settings.CacheRelativeDefault);
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.TextField("");
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.SelectableLabel(MobileBuildReportSettingsManager.settings.CacheLocation, GUILayout.Height(14));
                },

                keywords = new HashSet<string>(new[] { "Mobile BuildReport", "Build Report" })
            };

            return provider;
        }
    }
}
