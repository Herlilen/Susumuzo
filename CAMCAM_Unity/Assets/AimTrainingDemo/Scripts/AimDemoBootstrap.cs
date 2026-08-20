using UnityEngine;

/// <summary>
/// 瞄准测试 Demo 入口。仅在 AimTrainingDemo 场景中挂载，不会污染其他场景。
/// </summary>
public class AimDemoBootstrap : MonoBehaviour
{
    void Start()
    {
        BuildDemo();
    }

    void BuildDemo()
    {
        foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            if (cam.CompareTag("MainCamera") || cam.gameObject.name.Contains("Main Camera"))
                Destroy(cam.gameObject);
        }

        var root = new GameObject("AimArena").transform;
        root.SetParent(transform, false);

        BuildEnvironment(root);
        var player = BuildPlayer(root);
        var manager = gameObject.AddComponent<AimTrainingManager>();
        manager.player = player;
        manager.arenaRoot = root;
        manager.flickSpawnCenter = new Vector3(0f, 1.5f, 11f);
        manager.trackCenter = new Vector3(0f, 1.5f, 11f);

        var ui = gameObject.AddComponent<AimTrainingUI>();
        ui.manager = manager;
        ui.player = player;

        // 直接开始拉枪：每 3 靶自动调参
        manager.StartFlick();
    }

    void BuildEnvironment(Transform root)
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(root, false);
        floor.transform.position = new Vector3(0f, -0.05f, 8f);
        floor.transform.localScale = new Vector3(28f, 0.1f, 28f);
        RuntimeGraphics.SetGameObjectColor(floor, new Color(0.18f, 0.2f, 0.22f));

        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "BackWall";
        wall.transform.SetParent(root, false);
        wall.transform.position = new Vector3(0f, 3f, 16f);
        wall.transform.localScale = new Vector3(20f, 6f, 0.4f);
        RuntimeGraphics.SetGameObjectColor(wall, new Color(0.28f, 0.3f, 0.34f));

        CreateWall(root, "LeftWall", new Vector3(-10f, 3f, 8f), new Vector3(0.4f, 6f, 18f));
        CreateWall(root, "RightWall", new Vector3(10f, 3f, 8f), new Vector3(0.4f, 6f, 18f));

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "TargetZone";
        ring.transform.SetParent(root, false);
        ring.transform.position = new Vector3(0f, 0.02f, 11f);
        ring.transform.localScale = new Vector3(12f, 0.02f, 8f);
        RuntimeGraphics.SetGameObjectColor(ring, new Color(0.25f, 0.35f, 0.4f));
        Destroy(ring.GetComponent<Collider>());

        var lightGo = new GameObject("RangeLight");
        lightGo.transform.SetParent(root, false);
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        light.color = new Color(1f, 0.97f, 0.92f);

        var fillGo = new GameObject("FillLight");
        fillGo.transform.SetParent(root, false);
        fillGo.transform.position = new Vector3(0f, 4f, 4f);
        var fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.range = 30f;
        fill.intensity = 1.4f;
        fill.color = new Color(0.7f, 0.85f, 1f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.35f, 0.38f, 0.42f);
    }

    FirstPersonShooter BuildPlayer(Transform root)
    {
        var playerGo = new GameObject("Player");
        playerGo.transform.SetParent(root, false);
        playerGo.transform.position = new Vector3(0f, 1.6f, 0f);

        var camGo = new GameObject("PlayerCamera");
        camGo.transform.SetParent(playerGo.transform, false);
        camGo.transform.localPosition = Vector3.zero;
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.nearClipPlane = 0.05f;
        cam.fieldOfView = 75f;
        camGo.AddComponent<AudioListener>();
        RuntimeGraphics.SetupUrpCamera(cam);

        var fps = playerGo.AddComponent<FirstPersonShooter>();
        fps.playerCamera = cam;
        fps.sensitivity = new SensitivityProfile
        {
            horizontal = 1.2f,
            vertical = 1.2f,
            adsMultiplier = 0.75f,
            acceleration = 0.15f
        };

        var gun = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gun.name = "GunView";
        gun.transform.SetParent(camGo.transform, false);
        gun.transform.localPosition = new Vector3(0.25f, -0.22f, 0.45f);
        gun.transform.localScale = new Vector3(0.08f, 0.1f, 0.35f);
        RuntimeGraphics.SetGameObjectColor(gun, new Color(0.12f, 0.12f, 0.14f));
        Destroy(gun.GetComponent<Collider>());

        return fps;
    }

    void CreateWall(Transform root, string name, Vector3 pos, Vector3 scale)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(root, false);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        RuntimeGraphics.SetGameObjectColor(wall, new Color(0.22f, 0.24f, 0.27f));
    }
}
