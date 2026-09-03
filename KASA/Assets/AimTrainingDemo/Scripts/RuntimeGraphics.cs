using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 构建版安全的材质/相机初始化：避免 CreatePrimitive 默认材质在 Player 中丢失。
/// </summary>
public static class RuntimeGraphics
{
    static Material _template;

    public static Material Template
    {
        get
        {
            if (_template != null)
                return _template;

            // URP Asset 引用的默认材质一定会被打进包，不会被剥离
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp != null && urp.defaultMaterial != null)
            {
                _template = urp.defaultMaterial;
                return _template;
            }

            Shader shader =
                Shader.Find("Universal Render Pipeline/Lit") ??
                Shader.Find("Universal Render Pipeline/Simple Lit") ??
                Shader.Find("Unlit/Color") ??
                Shader.Find("Sprites/Default") ??
                Shader.Find("Standard");

            _template = shader != null
                ? new Material(shader) { name = "RuntimeFallbackLit" }
                : new Material(Shader.Find("Hidden/InternalErrorShader"));
            return _template;
        }
    }

    public static Material CreateColored(Color color)
    {
        var mat = new Material(Template) { name = "RuntimeColor" };
        ApplyColor(mat, color);
        return mat;
    }

    public static void ApplyColor(Material mat, Color color)
    {
        if (mat == null) return;
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);
        mat.color = color;
    }

    public static void SetRendererColor(Renderer renderer, Color color)
    {
        if (renderer == null) return;
        // 换掉可能已坏掉的默认材质实例
        renderer.sharedMaterial = CreateColored(color);
    }

    public static void SetGameObjectColor(GameObject go, Color color)
    {
        if (go == null) return;
        SetRendererColor(go.GetComponent<Renderer>(), color);
    }

    public static void SetupUrpCamera(Camera cam)
    {
        if (cam == null) return;
        if (cam.GetComponent<UniversalAdditionalCameraData>() == null)
            cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
    }
}
