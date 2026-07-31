using UnityEngine;
using UnityEngine.InputSystem;

public class AimTrainingManager : MonoBehaviour
{
    public FirstPersonShooter player;
    public Transform arenaRoot;
    public Vector3 trackCenter = new Vector3(0f, 1.4f, 10f);
    public float sessionSeconds = 25f;

    public bool IsPlaying { get; private set; }
    public TrackTrainingSession Track { get; private set; }
    public TrackMetrics LastTrackMetrics { get; private set; }
    public SensitivitySuggestion LastSuggestion { get; private set; }
    public bool HasResults { get; private set; }
    public string ResultsTitle { get; private set; }

    /// <summary>已完成的自动调参次数（0~3），用于三轮适配步长。</summary>
    public int AdaptationRound { get; private set; }

    public enum UiScreen
    {
        Menu,
        Playing,
        Results,
        Settings
    }

    public UiScreen Screen { get; private set; } = UiScreen.Menu;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Screen == UiScreen.Playing)
                AbortToMenu();
            else if (Screen == UiScreen.Settings || Screen == UiScreen.Results)
                OpenMenu();
        }

        if (IsPlaying && Track != null)
        {
            Track.Tick(Time.deltaTime);
            if (!Track.IsRunning)
                FinishTrack();
        }
    }

    public void OpenMenu()
    {
        StopSessions();
        IsPlaying = false;
        Screen = UiScreen.Menu;
        if (player != null)
            player.SetInputEnabled(false);
    }

    public void OpenSettings()
    {
        StopSessions();
        IsPlaying = false;
        Screen = UiScreen.Settings;
        if (player != null)
            player.SetInputEnabled(false);
    }

    public void ResetAdaptation()
    {
        AdaptationRound = 0;
    }

    public void StartTrack()
    {
        StopSessions();
        HasResults = false;
        IsPlaying = true;
        Screen = UiScreen.Playing;
        player.SetInputEnabled(true);
        Track = new TrackTrainingSession(
            player,
            arenaRoot,
            trackCenter,
            halfWidth: 4.5f,
            speed: 2.5f,
            sessionSeconds: sessionSeconds);
        Track.Start();
    }

    public void ApplyAutoTune()
    {
        if (!HasResults || player == null) return;

        bool changes =
            !Mathf.Approximately(LastSuggestion.horizontalMul, 1f) ||
            !Mathf.Approximately(LastSuggestion.verticalMul, 1f) ||
            !Mathf.Approximately(LastSuggestion.adsMul, 1f) ||
            !Mathf.Approximately(LastSuggestion.accelDelta, 0f);

        if (!changes) return;

        player.sensitivity.ApplySuggestion(LastSuggestion);

        if (AdaptationRound < SensitivityAdvisor.TargetAdaptationRounds)
            AdaptationRound++;

        LastSuggestion = SensitivitySuggestion.Identity(
            $"已应用调参（{AdaptationRound}/{SensitivityAdvisor.TargetAdaptationRounds}）",
            "请再测一轮，根据新的过冲/欠冲继续适配。目标约三轮完成。");
    }

    void FinishTrack()
    {
        LastTrackMetrics = Track.Metrics;
        LastSuggestion = SensitivityAdvisor.FromTrack(LastTrackMetrics, AdaptationRound);
        ResultsTitle = "跟枪测试结果";
        HasResults = true;
        IsPlaying = false;
        Screen = UiScreen.Results;
        player.SetInputEnabled(false);
    }

    void AbortToMenu()
    {
        StopSessions();
        OpenMenu();
    }

    void StopSessions()
    {
        Track?.Stop();
        Track = null;
        IsPlaying = false;
    }
}
