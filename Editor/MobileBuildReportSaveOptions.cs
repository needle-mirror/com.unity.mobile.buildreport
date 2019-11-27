using System;

namespace Unity.Mobile.BuildReport
{
    [Flags]
    internal enum MobileBuildReportSaveOptions
    {
        Default = 0,
        Custom = 1,
        Relative = 2
    }
}
