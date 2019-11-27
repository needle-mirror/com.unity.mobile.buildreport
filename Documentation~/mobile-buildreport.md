# About Mobile BuildReport
Use the Mobile BuildReport package to generate and store data about mobile builds. The Mobile BuildReport package stores the generated data in JSON files, allowing it to be re-used in various applications such as historical build size analysis, asset tracking etc.
Mobile BuildReport enables collection of the following data:
* Size of the build and individual files inside the application bundle.
* Estimated user-side download size of the app from stores.
* List of assets that comprise the build.
* Other useful information like build duration, BuildOptions etc.

## Preview package
This package is available as a preview, so it is not ready for production use. The features and documentation in this package might change before it is verified for release.

## Installation
To install this package, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Manual/upm-ui-install.html).

## Requirements
This version of Mobile BuildReport is compatible with the following versions of the Unity Editor:
* 2019.3 and later.

## Known limitations
Mobile BuildReport includes the following known limitations:
* Asset sizes cannot be estimated when LZ4 or LZ4HC compression methods are selected in BuildSettings.

# Using Mobile BuildReport
To use Mobile BuildReport, first enable build labelling in 'ProjectSettings > Mobile BuildReport'.
After build labelling is enabled, any new Android and iOS builds produced from the project will be labelled, which enables the build report to be generated.
To generate the report:
1. In the the menu bar, go to 'Mobile BuildReport > Show'.
2. Use the file browser to select the .apk/.ipa bundle of the build.
3. Click 'Generate'.

The generated report in JSON format will appear in directory, selected in 'ProjectSettings > Mobile BuildReport'.

Alternatively, the path to the labelled build can be passed to `MobileBuildReportHelper.Generate(string buildPath, string outputPath = null)` - this will generate the report and will optionally store it in a custom output path (it will be saved in the directory provided in Mobile BuildReport settings by default).

Mobile BuildReport uses a local cache on the development machine to store information on previous builds, that were produced with build labelling enabled. The information includes data about assets included in the build, as this information cannot be deducted directly from the application bundle. The cache is stored in `Library/MobileBuildReport` by default, but it can be set to any location in Mobile BuildReport settings (multiple Unity project can share the same cache directory).
> **Note:** The Mobile BuildReport entry only appears in the menu bar when Android or iOS platforms are targeted.
