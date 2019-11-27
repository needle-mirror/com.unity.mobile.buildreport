namespace Unity.Mobile.BuildReport
{
    internal interface IMobileBuildReportPlatformUtilities
    {
        void PrepareSharedAssets(string assetsPath);
        MobileBuildReportPlatformData[] CollectPlatformData(string buildPath);
    }
}
