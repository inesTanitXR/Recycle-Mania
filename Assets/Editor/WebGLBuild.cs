using UnityEditor;
using UnityEngine;
using System.Linq;

// Headless WebGL build used for CI / command-line rebuilds:
//   Unity -batchmode -quit -projectPath . -executeMethod WebGLBuild.Build
// Outputs into ./docs, which GitHub Pages serves for the Recyclepedia app.
public static class WebGLBuild
{
    public static void Build()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        var report = BuildPipeline.BuildPlayer(scenes, "docs", BuildTarget.WebGL, BuildOptions.None);
        var summary = report.summary;
        Debug.Log("WebGL build result: " + summary.result + " (" + summary.totalSize + " bytes)");
        if (summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
        }
    }
}
