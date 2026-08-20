using UnityEngine;

/// <summary>
/// 菜单 / HUD / 设置。拉枪模式每 3 靶自动调参，用 Toast 提示。
/// </summary>
public class AimTrainingUI : MonoBehaviour
{
    public AimTrainingManager manager;
    public FirstPersonShooter player;

    GUIStyle _title;
    GUIStyle _body;
    GUIStyle _hint;
    GUIStyle _box;
    bool _stylesReady;
    Texture2D _panelTex;
    Texture2D _accentTex;

    void EnsureStyles()
    {
        if (_stylesReady) return;
        _stylesReady = true;

        _panelTex = MakeTex(2, 2, new Color(0.06f, 0.07f, 0.09f, 0.88f));
        _accentTex = MakeTex(2, 2, new Color(0.92f, 0.42f, 0.22f, 0.95f));

        _title = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };
        _body = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            wordWrap = true,
            normal = { textColor = new Color(0.9f, 0.92f, 0.95f) }
        };
        _hint = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            wordWrap = true,
            normal = { textColor = new Color(0.7f, 0.75f, 0.8f) }
        };
        _box = new GUIStyle(GUI.skin.box)
        {
            normal = { background = _panelTex },
            padding = new RectOffset(18, 18, 16, 16)
        };
    }

    void OnGUI()
    {
        if (manager == null || player == null) return;
        EnsureStyles();

        DrawCrosshair();

        switch (manager.Screen)
        {
            case AimTrainingManager.UiScreen.Menu:
                DrawMenu();
                break;
            case AimTrainingManager.UiScreen.Playing:
                DrawHud();
                DrawToast();
                break;
            case AimTrainingManager.UiScreen.Settings:
                DrawSettings();
                break;
        }
    }

    void DrawCrosshair()
    {
        if (manager.Screen != AimTrainingManager.UiScreen.Playing) return;
        float s = 8f;
        float t = 2f;
        Color prev = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.85f);
        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;
        GUI.DrawTexture(new Rect(cx - s, cy - t * 0.5f, s * 2f, t), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - t * 0.5f, cy - s, t, s * 2f), Texture2D.whiteTexture);
        GUI.color = prev;
    }

    void DrawMenu()
    {
        Rect r = CenterPanel(540, 380);
        GUI.Box(r, GUIContent.none, _box);
        GUILayout.BeginArea(new Rect(r.x + 20, r.y + 16, r.width - 40, r.height - 32));
        GUILayout.Label("射击场 · 瞄准测试 Demo", _title);
        GUILayout.Space(6);
        GUILayout.Label(
            $"规则：每打完 {SensitivityAdvisor.TargetsPerTune} 个目标 → 按过冲/欠冲自动改灵敏度，然后继续打。",
            _hint);
        GUILayout.Space(8);
        GUILayout.Label($"已调参批数：{manager.AdaptationRound}", _body);
        GUILayout.Space(12);

        if (BigButton($"开始拉枪（每{SensitivityAdvisor.TargetsPerTune}靶调一次）", 52))
            manager.StartFlick();
        GUILayout.Space(8);
        if (BigButton("灵敏度设置", 40))
            manager.OpenSettings();
        GUILayout.Space(8);
        if (manager.AdaptationRound > 0 && BigButton("重置适配进度", 36))
            manager.ResetAdaptation();

        GUILayout.FlexibleSpace();
        GUILayout.Label("Esc 打开菜单 · 左键射击 · 右键开镜", _hint);
        GUILayout.EndArea();
    }

    void DrawHud()
    {
        Rect r = new Rect(20, 20, 420, 170);
        GUI.Box(r, GUIContent.none, _box);
        GUILayout.BeginArea(new Rect(r.x + 14, r.y + 10, r.width - 28, r.height - 20));

        if (manager.Flick != null)
        {
            var batch = manager.Flick.BatchMetrics;
            GUILayout.Label("拉枪 · 每3靶调参", _title);
            GUILayout.Label(
                $"进度 {manager.BatchProgress}/{SensitivityAdvisor.TargetsPerTune}  →  再打 {manager.TargetsUntilTune} 个就调灵敏度",
                _body);
            GUILayout.Label(
                $"本批：过冲{batch.overshootEvents} 欠冲{batch.undershootEvents} 干净{batch.settleEvents}  命中{batch.hits}/{batch.shots}",
                _body);
            GUILayout.Label(
                $"灵敏度 水平{player.sensitivity.horizontal:0.00} / 垂直{player.sensitivity.vertical:0.00}  · 已调{manager.AdaptationRound}批",
                _hint);
        }
        else if (manager.Track != null)
        {
            var m = manager.Track.Metrics;
            GUILayout.Label($"跟枪测试  ·  剩余 {manager.Track.TimeLeft:0.0}s", _title);
            GUILayout.Label($"目标停留 {m.StayRatio:P0}", _body);
            GUILayout.Label($"过冲 {m.OvershootRate:P0}  欠冲 {m.UndershootRate:P0}", _body);
        }

        GUILayout.EndArea();
    }

    void DrawToast()
    {
        if (string.IsNullOrEmpty(manager.LastToast) || Time.unscaledTime > manager.ToastUntil)
            return;

        float w = 520f;
        float h = 72f;
        Rect r = new Rect((Screen.width - w) * 0.5f, Screen.height - h - 36f, w, h);
        GUI.Box(r, GUIContent.none, _box);
        GUILayout.BeginArea(new Rect(r.x + 14, r.y + 12, r.width - 28, r.height - 20));
        GUILayout.Label(manager.LastToast, _body);
        GUILayout.EndArea();
    }

    void DrawSettings()
    {
        Rect r = CenterPanel(620, 520);
        GUI.Box(r, GUIContent.none, _box);
        GUILayout.BeginArea(new Rect(r.x + 20, r.y + 16, r.width - 40, r.height - 32));
        GUILayout.Label("灵敏度设置", _title);
        GUILayout.Space(4);
        GUILayout.Label("每个参数都附带直观说明。", _hint);
        GUILayout.Space(12);

        var sens = player.sensitivity;
        sens.horizontal = LabeledSlider("水平灵敏度", sens.horizontal, 0.05f, 5f, SensitivityProfile.HorizontalHelp);
        sens.vertical = LabeledSlider("垂直灵敏度", sens.vertical, 0.05f, 5f, SensitivityProfile.VerticalHelp);
        sens.adsMultiplier = LabeledSlider("开镜灵敏度倍率", sens.adsMultiplier, 0.2f, 1.5f, SensitivityProfile.AdsHelp);
        sens.acceleration = LabeledSlider("鼠标加速", sens.acceleration, 0f, 1f, SensitivityProfile.AccelHelp);

        GUILayout.Space(16);
        if (BigButton("返回菜单", 40))
            manager.OpenMenu();
        GUILayout.EndArea();
    }

    float LabeledSlider(string label, float value, float min, float max, string help)
    {
        GUILayout.Label($"{label}：{value:0.00}", _body);
        value = GUILayout.HorizontalSlider(value, min, max, GUILayout.Height(18));
        GUILayout.Label(help, _hint);
        GUILayout.Space(10);
        return value;
    }

    bool BigButton(string text, float height)
    {
        var style = new GUIStyle(GUI.skin.button)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            fixedHeight = height,
            normal = { background = _accentTex, textColor = Color.white },
            hover = { background = _accentTex, textColor = Color.white },
            active = { background = _accentTex, textColor = Color.white }
        };
        return GUILayout.Button(text, style);
    }

    static Rect CenterPanel(float w, float h)
    {
        return new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
    }

    static Texture2D MakeTex(int w, int h, Color col)
    {
        var tex = new Texture2D(w, h);
        var pix = new Color[w * h];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        tex.SetPixels(pix);
        tex.Apply();
        return tex;
    }
}
