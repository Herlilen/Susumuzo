using UnityEngine;
using UnityEngine.InputSystem;

public class AimTrainingManager : MonoBehaviour
{
    public FirstPersonShooter player;
    public Transform arenaRoot;
    public Vector3 flickSpawnCenter = new Vector3(0f, 1.5f, 11f);
    public Vector3 trackCenter = new Vector3(0f, 1.4f, 10f);

    public bool IsPlaying { get; private set; }
    public FlickTrainingSession Flick { get; private set; }
    public TrackTrainingSession Track { get; private set; }
    public FlickMetrics LastBatchMetrics { get; private set; }
    public TrackMetrics LastTrackMetrics { get; private set; }
    public SensitivitySuggestion LastSuggestion { get; private set; }
    public string LastToast { get; private set; }
    public float ToastUntil { get; private set; }

    /// <summary>已自动调参批次数（每 3 靶 = 1 批）。</summary>
    public int AdaptationRound { get; private set; }

    /// <summary>当前批次内已完成目标数（0~3，满 3 就调参并清零）。</summary>
    public int BatchProgress { get; private set; }

    public int TargetsUntilTune =>
        Mathf.Max(0, SensitivityAdvisor.TargetsPerTune - BatchProgress);

    public enum UiScreen
    {
        Menu,
        Playing,
        Settings
    }

    public UiScreen Screen { get; private set; } = UiScreen.Menu;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Screen == UiScreen.Playing)
                AbortToMenu();
            else if (Screen == UiScreen.Settings)
                OpenMenu();
        }
    }

    // LateUpdate：在视角输入本帧更新后，用角速度判断「鼠标停下」
    void LateUpdate()
    {
        if (!IsPlaying) return;

        if (Flick != null)
            Flick.Tick(Time.deltaTime);
        else if (Track != null)
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
        BatchProgress = 0;
    }

    public void StartFlick()
    {
        StopSessions();
        BatchProgress = 0;
        IsPlaying = true;
        Screen = UiScreen.Playing;
        player.SetInputEnabled(true);

        Flick = new FlickTrainingSession(player, arenaRoot, flickSpawnCenter);
        Flick.OnTargetCompleted += HandleFlickTargetCompleted;
        Flick.Start();
        ShowToast($"每打完 {SensitivityAdvisor.TargetsPerTune} 个目标，自动调一次灵敏度");
    }

    public void StartTrack()
    {
        StopSessions();
        IsPlaying = true;
        Screen = UiScreen.Playing;
        player.SetInputEnabled(true);
        Track = new TrackTrainingSession(
            player,
            arenaRoot,
            trackCenter,
            halfWidth: 4.5f,
            speed: 2.5f,
            sessionSeconds: 25f);
        Track.Start();
    }

    void HandleFlickTargetCompleted()
    {
        if (Flick == null) return;

        BatchProgress++;
        Debug.Log($"[AimTraining] 目标完成 {BatchProgress}/{SensitivityAdvisor.TargetsPerTune}");

        if (BatchProgress < SensitivityAdvisor.TargetsPerTune)
        {
            ShowToast($"再打 {TargetsUntilTune} 个目标后调参（{BatchProgress}/{SensitivityAdvisor.TargetsPerTune}）");
            return;
        }

        // ===== 满 3 个目标：立刻调灵敏度 =====
        LastBatchMetrics = Flick.BatchMetrics.Clone();
        LastSuggestion = SensitivityAdvisor.FromFlickBatch(LastBatchMetrics, AdaptationRound);

        bool changes =
            !Mathf.Approximately(LastSuggestion.horizontalMul, 1f) ||
            !Mathf.Approximately(LastSuggestion.verticalMul, 1f) ||
            !Mathf.Approximately(LastSuggestion.adsMul, 1f) ||
            !Mathf.Approximately(LastSuggestion.accelDelta, 0f);

        if (changes)
        {
            float before = player.sensitivity.horizontal;
            player.sensitivity.ApplySuggestion(LastSuggestion);
            AdaptationRound++;
            ShowToast(
                $"【已调参】{LastSuggestion.summary}\n" +
                $"水平 {before:0.00} → {player.sensitivity.horizontal:0.00}  " +
                $"垂直 {player.sensitivity.vertical:0.00}");
            Debug.Log($"[AimTraining] 调参应用: {LastSuggestion.summary} H={player.sensitivity.horizontal}");
        }
        else
        {
            ShowToast($"【满3靶】{LastSuggestion.summary}");
            Debug.Log($"[AimTraining] 满3靶但未改参: {LastSuggestion.detail}");
        }

        Flick.BatchMetrics.Reset();
        BatchProgress = 0;
    }

    void FinishTrack()
    {
        LastTrackMetrics = Track.Metrics;
        LastSuggestion = SensitivityAdvisor.FromTrack(LastTrackMetrics, AdaptationRound);
        bool changes =
            !Mathf.Approximately(LastSuggestion.horizontalMul, 1f) ||
            !Mathf.Approximately(LastSuggestion.verticalMul, 1f) ||
            !Mathf.Approximately(LastSuggestion.adsMul, 1f) ||
            !Mathf.Approximately(LastSuggestion.accelDelta, 0f);

        if (changes)
        {
            player.sensitivity.ApplySuggestion(LastSuggestion);
            AdaptationRound++;
        }

        ShowToast(LastSuggestion.summary);
        IsPlaying = false;
        Screen = UiScreen.Menu;
        player.SetInputEnabled(false);
        Track = null;
    }

    void ShowToast(string msg)
    {
        LastToast = msg;
        ToastUntil = Time.unscaledTime + 3.5f;
    }

    void AbortToMenu()
    {
        StopSessions();
        OpenMenu();
    }

    void StopSessions()
    {
        if (Flick != null)
        {
            Flick.OnTargetCompleted -= HandleFlickTargetCompleted;
            Flick.Stop();
            Flick = null;
        }

        Track?.Stop();
        Track = null;
        IsPlaying = false;
        BatchProgress = 0;
    }
}
