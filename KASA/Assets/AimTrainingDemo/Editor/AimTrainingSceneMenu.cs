using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 瞄准测试 Demo 的隔离入口：独立场景，不侵入 SampleScene / 其他玩法。
/// </summary>
public static class AimTrainingSceneMenu
{
    const string ScenePath = "Assets/AimTrainingDemo/Scenes/AimTrainingDemo.unity";

    [MenuItem("Tools/Aim Training/Open Demo Scene", priority = 0)]
    [MenuItem("Aim Training/Open Demo Scene", priority = 0)]
    public static void OpenDemoScene()
    {
        EnsureSceneExists();
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    [MenuItem("Tools/Aim Training/Play Demo Scene", priority = 1)]
    [MenuItem("Aim Training/Play Demo Scene", priority = 1)]
    public static void PlayDemoScene()
    {
        EnsureSceneExists();
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (SceneManager.GetActiveScene().path != ScenePath)
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        EditorApplication.isPlaying = true;
    }

    [MenuItem("Tools/Aim Training/Create / Refresh Demo Scene", priority = 20)]
    [MenuItem("Aim Training/Create / Refresh Demo Scene", priority = 20)]
    public static void EnsureSceneExists()
    {
        string dir = Path.GetDirectoryName(ScenePath)?.Replace('\\', '/');
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        if (File.Exists(ScenePath))
        {
            // 确保场景里有 Bootstrap
            var existing = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (Object.FindFirstObjectByType<AimDemoBootstrap>() == null)
            {
                var go = new GameObject("AimTrainingDemo");
                go.AddComponent<AimDemoBootstrap>();
                EditorSceneManager.MarkSceneDirty(existing);
                EditorSceneManager.SaveScene(existing);
            }
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        // 保留默认灯光；Bootstrap 会替换主相机并自建场地
        var bootstrap = new GameObject("AimTrainingDemo");
        bootstrap.AddComponent<AimDemoBootstrap>();
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.Refresh();
        Debug.Log($"[AimTraining] Demo scene created: {ScenePath}");
    }
}
