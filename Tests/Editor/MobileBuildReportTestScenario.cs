using NUnit.Framework;
using Unity.Mobile.BuildReport;
using UnityEditor.TestTools;
using System;
using UnityEditor;
using System.IO;

namespace MobileBuildReportTests
{
    [RequirePlatformSupport(BuildTarget.Android)]   //@TODO* This shouldn't be here, UNITY_ANDROID constraint not recognized in Tests.asmdef for some reason
    public abstract class MobileBuildReportTestScenario
    {
        protected virtual BuildTarget Target
        {
            get
            {
                if (!(Attribute.GetCustomAttribute(GetType(), typeof(RequirePlatformSupportAttribute)) is RequirePlatformSupportAttribute requirePlatformSupportAttribute))
                    throw new Exception("Couldn't determine target platform from the RequirePlatformSupportAttribute as this test fixture does not appear to have that attribute.");

                if (requirePlatformSupportAttribute.platforms.Length != 1)
                    throw new Exception("Couldn't determine target platform from the RequirePlatformSupportAttribute as it does not list exactly one platform.");

                return requirePlatformSupportAttribute.platforms[0];
            }
        }

        protected virtual ScriptingImplementation ScriptingRuntime => PlayerSettings.GetDefaultScriptingBackend(BuildPipeline.GetBuildTargetGroup(Target));

        protected virtual BuildOptions Options => BuildOptions.None;

        protected virtual void PreBuildSetup() {}
        protected virtual void PostBuildCleanup() {}

        private string artifactDirectory;
        protected virtual string ArtifactDirectory
        {
            get
            {
                if (!string.IsNullOrEmpty(artifactDirectory))
                    return artifactDirectory;
                artifactDirectory = Path.Combine(Path.GetTempPath(), $"MobileBuildReportTest-{GUID.Generate()}");
                if (!Directory.Exists(artifactDirectory))
                    Directory.CreateDirectory(artifactDirectory);
                return artifactDirectory;
            }
        }

        protected virtual string BuildLocation { get; set; }

        private string ReportLocation { get; set; }
        private MobileBuildReport Report { get; set; }

        [OneTimeSetUp]
        public void CreateBuildWithReportMarker()
        {
            MobileBuildReportSettingsManager.settings.LabelBuildsEnabled = true;
            MobileBuildReportSettingsManager.settings.ShowReport = false;
            PlayerSettings.SetScriptingBackend(BuildPipeline.GetBuildTargetGroup(Target), ScriptingRuntime);
            PreBuildSetup();
            var unityReport = BuildPipeline.BuildPlayer(new string[] { "Assets/MobileReportScene.unity" }, BuildLocation, Target, Options);
            PostBuildCleanup();

            ReportLocation = Path.Combine(ArtifactDirectory, "report.json");
            Report = MobileBuildReportHelper.Generate(unityReport.summary.outputPath, ReportLocation);
        }

        [Test]
        public void DownloadSizeIsEstimated()
        {
            foreach (var platformData in Report.PlatformData)
                Assert.Greater(platformData.DownloadSize, 0, $"Download size was not estimated for architecture {platformData.Architecture}.");
        }

        [Test]
        public void ReportDeserializesCorrectly()
        {
            var loadedReport = MobileBuildReport.Load(ReportLocation);
            Assert.AreEqual(Report.Platform.ToString(), loadedReport.Platform.ToString());
            Assert.AreEqual(Report.Options.ToString(), Report.Options.ToString());
            Assert.AreEqual(Report.OutputPath, loadedReport.OutputPath);
            Assert.AreEqual(Report.BuildEndedAt, loadedReport.BuildEndedAt);
            Assert.AreEqual(Report.BuildStartedAt, loadedReport.BuildStartedAt);
            Assert.AreEqual(Report.TotalTime, loadedReport.TotalTime);
            Assert.AreEqual(Report.TotalSize, loadedReport.TotalSize);

            for (int i = 0; i < Report.PlatformData.Length; i++)
            {
                Assert.AreEqual(Report.PlatformData[i].Architecture, loadedReport.PlatformData[i].Architecture);
                Assert.AreEqual(Report.PlatformData[i].DownloadSize, loadedReport.PlatformData[i].DownloadSize);

                if (Report.PlatformData[i].Segments != null)
                {
                    foreach (var item in Report.PlatformData[i].Segments)
                    {
                        Assert.That(loadedReport.PlatformData[i].Segments.ContainsKey(item.Key));
                        Assert.AreEqual(loadedReport.PlatformData[i].Segments[item.Key], Report.PlatformData[i].Segments[item.Key]);
                    }
                }
            }
            for (var i = 0; i < Report.Assets.Length; i++)
            {
                Assert.AreEqual(Report.Assets[i].DataPath, loadedReport.Assets[i].DataPath);
                Assert.AreEqual(Report.Assets[i].Overhead, loadedReport.Assets[i].Overhead);
                for (var j = 0; j < Report.Assets[i].Contents.Length; j++)
                {
                    Assert.AreEqual(Report.Assets[i].Contents[j].Id, loadedReport.Assets[i].Contents[j].Id);
                    Assert.AreEqual(Report.Assets[i].Contents[j].Offset, loadedReport.Assets[i].Contents[j].Offset);
                    Assert.AreEqual(Report.Assets[i].Contents[j].Size, loadedReport.Assets[i].Contents[j].Size);
                    Assert.AreEqual(Report.Assets[i].Contents[j].EstimatedSize, loadedReport.Assets[i].Contents[j].EstimatedSize);
                    Assert.AreEqual(Report.Assets[i].Contents[j].SourceAssetGUID, loadedReport.Assets[i].Contents[j].SourceAssetGUID);
                    Assert.AreEqual(Report.Assets[i].Contents[j].SourceAssetPath, loadedReport.Assets[i].Contents[j].SourceAssetPath);
                    Assert.AreEqual(Report.Assets[i].Contents[j].Type, loadedReport.Assets[i].Contents[j].Type);
                }
            }
        }

        [Test]
        public void AssetSizesAreEstimated()
        {
            for (var i = 0; i < Report.Assets.Length; i++)
                for (var j = 0; j < Report.Assets[i].Contents.Length; j++)
                    Assert.Greater(Report.Assets[i].Contents[j].EstimatedSize, 0, "Asset size was not estimated.");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Directory.Delete(ArtifactDirectory, true);
        }
    }
}
