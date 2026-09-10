using System.IO;
using Kasa.PostProcessingDemo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kasa.PostProcessingDemo.Editor
{
    public static class CreatePostProcessingDemoScene
    {
        private const string Root = "Assets/PostProcessingDemo";
        private const string ScenePath = Root + "/Scenes/PostProcessingComparison.unity";

        [MenuItem("KASA/Post Processing/Create Comparison Scene")]
        public static void Build()
        {
            EnsureFolder(Root + "/Scenes");
            EnsureFolder(Root + "/Materials");
            EnsureFolder(Root + "/RenderTextures");

            RenderTexture normalRt = CreateRenderTexture("NormalView", 640, 360, FilterMode.Bilinear);
            RenderTexture asciiRt = CreateRenderTexture("AsciiSource", 1920, 1080, FilterMode.Point);
            RenderTexture crtRt = CreateRenderTexture("CrtLowResolution", 320, 180, FilterMode.Point);
            Material asciiMaterial = CreateMaterial("AsciiPost", "KASA/PostProcessing/ASCII");
            Material crtMaterial = CreateMaterial("CrtRetroPost", "KASA/PostProcessing/CRT Retro");
            asciiMaterial.SetFloat("_CellSize", 8f);
            asciiMaterial.SetFloat("_Contrast", 1.2f);
            asciiMaterial.SetFloat("_BlockStrength", 0.58f);
            asciiMaterial.SetFloat("_ColorSteps", 6f);
            asciiMaterial.SetColor("_Tint", Color.white);
            asciiMaterial.SetColor("_Background", new Color(0.008f, 0.01f, 0.015f, 1f));
            EditorUtility.SetDirty(asciiMaterial);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "PostProcessingComparison";

            GameObject environment = new GameObject("Demo Environment");
            CreateLighting(environment.transform);
            Transform[] subjects = CreateSubjects(environment.transform);

            Vector3 cameraPosition = new Vector3(0f, 3.2f, -9.5f);
            Quaternion cameraRotation = Quaternion.Euler(11f, 0f, 0f);
            CreateCaptureCamera("Camera - Normal", cameraPosition, cameraRotation, normalRt);
            CreateCaptureCamera("Camera - ASCII", cameraPosition, cameraRotation, asciiRt);
            CreateCaptureCamera("Camera - CRT Low Resolution", cameraPosition, cameraRotation, crtRt);
            CreatePresentationCamera();

            Light accent = CreateAccentLight(environment.transform);
            GameObject controllerObject = new GameObject("Demo Animation");
            PostProcessingDemoController controller = controllerObject.AddComponent<PostProcessingDemoController>();
            UiReferences ui = CreateComparisonCanvas(normalRt);
            SerializedObject serializedController = new SerializedObject(controller);
            SerializedProperty rotatingSubjects = serializedController.FindProperty("rotatingSubjects");
            rotatingSubjects.arraySize = subjects.Length;
            for (int i = 0; i < subjects.Length; i++)
                rotatingSubjects.GetArrayElementAtIndex(i).objectReferenceValue = subjects[i];
            serializedController.FindProperty("accentLight").objectReferenceValue = accent;
            serializedController.FindProperty("output").objectReferenceValue = ui.Output;
            serializedController.FindProperty("modeLabel").objectReferenceValue = ui.ModeLabel;
            serializedController.FindProperty("detailLabel").objectReferenceValue = ui.DetailLabel;
            serializedController.FindProperty("normalSource").objectReferenceValue = normalRt;
            serializedController.FindProperty("asciiSource").objectReferenceValue = asciiRt;
            serializedController.FindProperty("crtSource").objectReferenceValue = crtRt;
            serializedController.FindProperty("asciiMaterial").objectReferenceValue = asciiMaterial;
            serializedController.FindProperty("crtMaterial").objectReferenceValue = crtMaterial;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = AppendScene(EditorBuildSettings.scenes, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log($"Created post-processing comparison scene at {ScenePath}");
        }

        private static void CreateLighting(Transform parent)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.22f, 0.3f, 0.48f);
            RenderSettings.ambientEquatorColor = new Color(0.08f, 0.09f, 0.13f);
            RenderSettings.ambientGroundColor = new Color(0.018f, 0.02f, 0.028f);

            GameObject sunObject = new GameObject("Key Light");
            sunObject.transform.SetParent(parent);
            sunObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.7f;
            sun.color = new Color(1f, 0.88f, 0.72f);
            sun.shadows = LightShadows.Soft;
        }

        private static Light CreateAccentLight(Transform parent)
        {
            GameObject lightObject = new GameObject("Animated Accent Light");
            lightObject.transform.SetParent(parent);
            lightObject.transform.position = new Vector3(-2.5f, 3.5f, -1f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 12f;
            light.intensity = 7f;
            light.color = new Color(0.25f, 0.75f, 1f);
            return light;
        }

        private static Transform[] CreateSubjects(Transform parent)
        {
            Material floorMaterial = CreateLitMaterial("Floor", new Color(0.025f, 0.035f, 0.055f), 0.1f, 0.72f);
            Material cyan = CreateLitMaterial("Cyan", new Color(0.03f, 0.65f, 0.9f), 0.35f, 0.28f);
            Material orange = CreateLitMaterial("Orange", new Color(1f, 0.2f, 0.035f), 0.15f, 0.4f);
            Material cream = CreateLitMaterial("Cream", new Color(0.95f, 0.78f, 0.42f), 0.5f, 0.22f);

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(parent);
            floor.transform.localScale = new Vector3(2.2f, 1f, 1.5f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;

            GameObject cube = CreatePrimitive(PrimitiveType.Cube, "Rotating Cube", new Vector3(-2.6f, 1.1f, 0f), new Vector3(1.6f, 1.6f, 1.6f), cyan, parent);
            GameObject sphere = CreatePrimitive(PrimitiveType.Sphere, "Rotating Sphere", new Vector3(0f, 1.15f, 0.3f), new Vector3(1.65f, 1.65f, 1.65f), orange, parent);
            GameObject capsule = CreatePrimitive(PrimitiveType.Capsule, "Rotating Capsule", new Vector3(2.6f, 1.25f, 0f), new Vector3(1.25f, 1.25f, 1.25f), cream, parent);

            for (int i = -5; i <= 5; i++)
            {
                GameObject marker = CreatePrimitive(PrimitiveType.Cube, $"Grid Marker {i + 6:00}", new Vector3(i * 0.75f, 0.08f, 3.1f), new Vector3(0.08f, 0.16f, 0.8f), i % 2 == 0 ? orange : cyan, parent);
                Object.DestroyImmediate(marker.GetComponent<Collider>());
            }

            return new[] { cube.transform, sphere.transform, capsule.transform };
        }

        private static GameObject CreatePrimitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            GameObject instance = GameObject.CreatePrimitive(type);
            instance.name = name;
            instance.transform.SetParent(parent);
            instance.transform.position = position;
            instance.transform.localScale = scale;
            instance.GetComponent<Renderer>().sharedMaterial = material;
            return instance;
        }

        private static void CreateCaptureCamera(string name, Vector3 position, Quaternion rotation, RenderTexture target)
        {
            GameObject cameraObject = new GameObject(name);
            cameraObject.transform.SetPositionAndRotation(position, rotation);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.targetTexture = target;
            camera.fieldOfView = 48f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.008f, 0.012f, 0.025f);
            camera.allowHDR = true;
        }

        private static void CreatePresentationCamera()
        {
            GameObject cameraObject = new GameObject("Presentation Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.cullingMask = 0;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.008f, 0.009f, 0.014f);
            camera.depth = 100f;
        }

        private static UiReferences CreateComparisonCanvas(RenderTexture normal)
        {
            GameObject canvasObject = new GameObject("Comparison UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            CreateBackdrop(canvas.transform);

            GameObject view = new GameObject("Fullscreen Filter Output", typeof(RectTransform), typeof(RawImage));
            view.transform.SetParent(canvas.transform, false);
            RectTransform viewRect = view.GetComponent<RectTransform>();
            viewRect.anchorMin = Vector2.zero;
            viewRect.anchorMax = Vector2.one;
            viewRect.offsetMin = Vector2.zero;
            viewRect.offsetMax = Vector2.zero;
            RawImage rawImage = view.GetComponent<RawImage>();
            rawImage.texture = normal;
            rawImage.raycastTarget = false;

            GameObject header = new GameObject("Mode Header", typeof(RectTransform), typeof(Image));
            header.transform.SetParent(canvas.transform, false);
            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(0f, 1f);
            headerRect.pivot = new Vector2(0f, 1f);
            headerRect.anchoredPosition = new Vector2(28f, -28f);
            headerRect.sizeDelta = new Vector2(570f, 118f);
            header.GetComponent<Image>().color = new Color(0.008f, 0.012f, 0.02f, 0.86f);

            Text modeLabel = CreateText(header.transform, "01  NORMAL", 30, FontStyle.Bold, TextAnchor.MiddleLeft);
            modeLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
            modeLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
            modeLabel.rectTransform.pivot = new Vector2(0.5f, 1f);
            modeLabel.rectTransform.anchoredPosition = new Vector2(0f, -12f);
            modeLabel.rectTransform.sizeDelta = new Vector2(-32f, 48f);
            modeLabel.color = new Color(0.94f, 0.97f, 1f);

            Text detailLabel = CreateText(header.transform, "Unprocessed reference", 17, FontStyle.Normal, TextAnchor.MiddleLeft);
            detailLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
            detailLabel.rectTransform.anchorMax = new Vector2(1f, 0f);
            detailLabel.rectTransform.pivot = new Vector2(0.5f, 0f);
            detailLabel.rectTransform.anchoredPosition = new Vector2(0f, 14f);
            detailLabel.rectTransform.sizeDelta = new Vector2(-32f, 36f);
            detailLabel.color = new Color(0.55f, 0.68f, 0.8f);

            Text controls = CreateText(canvas.transform, "[ 1 ] NORMAL     [ 2 ] ASCII     [ 3 ] CRT + LOW RES", 19, FontStyle.Bold, TextAnchor.MiddleCenter);
            controls.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            controls.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            controls.rectTransform.pivot = new Vector2(0.5f, 0f);
            controls.rectTransform.anchoredPosition = new Vector2(0f, 22f);
            controls.rectTransform.sizeDelta = new Vector2(720f, 46f);
            controls.color = new Color(0.86f, 0.9f, 0.96f);

            return new UiReferences(rawImage, modeLabel, detailLabel);
        }

        private static void CreateBackdrop(Transform parent)
        {
            GameObject backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(parent, false);
            RectTransform rect = backdrop.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            backdrop.GetComponent<Image>().color = new Color(0.008f, 0.009f, 0.014f, 1f);
        }

        private static Text CreateText(Transform parent, string value, int size, FontStyle style, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(value, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static RenderTexture CreateRenderTexture(string name, int width, int height, FilterMode filter)
        {
            string path = $"{Root}/RenderTextures/{name}.renderTexture";
            RenderTexture existing = AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
            if (existing != null)
            {
                if (existing.width != width || existing.height != height)
                {
                    existing.Release();
                    existing.width = width;
                    existing.height = height;
                    existing.Create();
                }
                existing.filterMode = filter;
                EditorUtility.SetDirty(existing);
                return existing;
            }

            RenderTexture texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                name = name,
                filterMode = filter,
                antiAliasing = 1,
                useMipMap = false,
                autoGenerateMips = false
            };
            AssetDatabase.CreateAsset(texture, path);
            return texture;
        }

        private static Material CreateMaterial(string name, string shaderName)
        {
            string path = $"{Root}/Materials/{name}.mat";
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
                throw new FileNotFoundException($"Shader not found: {shaderName}");

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
                EditorUtility.SetDirty(material);
            }
            return material;
        }

        private static Material CreateLitMaterial(string name, Color color, float metallic, float smoothness)
        {
            string path = $"{Root}/Materials/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolder(string path)
        {
            string[] segments = path.Split('/');
            string current = segments[0];
            for (int i = 1; i < segments.Length; i++)
            {
                string next = current + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, segments[i]);
                current = next;
            }
        }

        private static EditorBuildSettingsScene[] AppendScene(EditorBuildSettingsScene[] scenes, string path)
        {
            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (scene.path == path)
                    return scenes;
            }

            EditorBuildSettingsScene[] result = new EditorBuildSettingsScene[scenes.Length + 1];
            scenes.CopyTo(result, 0);
            result[^1] = new EditorBuildSettingsScene(path, true);
            return result;
        }

        private sealed class UiReferences
        {
            public readonly RawImage Output;
            public readonly Text ModeLabel;
            public readonly Text DetailLabel;

            public UiReferences(RawImage output, Text modeLabel, Text detailLabel)
            {
                Output = output;
                ModeLabel = modeLabel;
                DetailLabel = detailLabel;
            }
        }
    }
}
