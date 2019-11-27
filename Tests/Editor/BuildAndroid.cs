using System.IO;
using MobileBuildReportTests;
using UnityEditor;
using UnityEditor.TestTools;

[RequirePlatformSupport(BuildTarget.Android)]
public class BuildAndroid : MobileBuildReportTestScenario
{
    protected override void PreBuildSetup()
    {
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        BuildLocation = Path.Combine(ArtifactDirectory, "testBuild.apk");
    }
}
