using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 仅打包瞄准测试 Demo（独立场景），不影响项目其他内容。
/// </summary>
public static class DesktopBuild
{
    const string FolderName = "KASA_AimTrainingDemo";
    const string ScenePath = "Assets/AimTrainingDemo/Scenes/AimTrainingDemo.unity";
    const string RequestFileName = "REQUEST_DESKTOP_BUILD";

    [MenuItem("Aim Training/Build Windows Folder to Desktop", priority = 50)]
    public static void BuildToDesktop()
    {
        AimTrainingSceneMenu.EnsureSceneExists();

        string desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
        string outDir = Path.Combine(desktop, FolderName);
        string exePath = Path.Combine(outDir, "KASA.exe");

        try
        {
            if (Directory.Exists(outDir))
                Directory.Delete(outDir, true);
            Directory.CreateDirectory(outDir);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Cannot prepare output folder: {e.Message}");
            ExitIfBatch(1);
            return;
        }

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = exePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log($"[DesktopBuild] Building Aim Training Demo to: {outDir}");
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            File.WriteAllText(
                Path.Combine(outDir, "使用说明.txt"),
                "KASA 瞄准测试 Demo（跟枪）\r\n\r\n" +
                "本包仅包含瞄准测试内容，与主工程其他玩法隔离。\r\n\r\n" +
                "1. 双击 KASA.exe 运行\r\n" +
                "2. 开始跟枪测试，结束后可一键调参\r\n" +
                "3. 目标约 3 轮完成灵敏度适配\r\n" +
                "操作：鼠标瞄准 · 左键射击 · 右键开镜 · Esc 返回菜单\r\n");

            Debug.Log($"[DesktopBuild] SUCCESS -> {outDir} ({summary.totalSize} bytes)");
            EditorUtility.RevealInFinder(outDir);
            ExitIfBatch(0);
        }
        else
        {
            Debug.LogError($"[DesktopBuild] FAILED: {summary.result}");
            ExitIfBatch(1);
        }
    }

    static void ExitIfBatch(int code)
    {
        if (Application.isBatchMode)
            EditorApplication.Exit(code);
    }

    [InitializeOnLoadMethod]
    static void WatchBuildRequest()
    {
        EditorApplication.update -= PollBuildRequest;
        EditorApplication.update += PollBuildRequest;
    }

    static double _nextPoll;

    static void PollBuildRequest()
    {
        if (EditorApplication.timeSinceStartup < _nextPoll)
            return;
        _nextPoll = EditorApplication.timeSinceStartup + 1.0;

        string req = Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, "Temp", RequestFileName);
        if (!File.Exists(req))
            return;

        try { File.Delete(req); } catch { return; }

        Debug.Log("[DesktopBuild] Detected build request file, starting build...");
        EditorApplication.delayCall += BuildToDesktop;
    }
}
