using System;
using UnityEngine;

[Serializable]
public class FlickMetrics
{
    public int shots;
    public int hits;
    public int targetsPresented;
    public int firstShotHits;
    public int overshootEvents;
    public int undershootEvents;
    public int settleEvents;

    public float TotalAccuracy => shots <= 0 ? 0f : (float)hits / shots;
    public float FirstShotAccuracy => targetsPresented <= 0 ? 0f : (float)firstShotHits / targetsPresented;

    public int LeadLagCount => overshootEvents + undershootEvents + settleEvents;

    public float OvershootRate
    {
        get
        {
            int n = LeadLagCount;
            return n <= 0 ? 0f : (float)overshootEvents / n;
        }
    }

    public float UndershootRate
    {
        get
        {
            int n = LeadLagCount;
            return n <= 0 ? 0f : (float)undershootEvents / n;
        }
    }

    public float NetBias
    {
        get
        {
            int n = LeadLagCount;
            if (n <= 0) return 0f;
            return (undershootEvents - overshootEvents) / (float)n;
        }
    }

    public void Reset()
    {
        shots = 0;
        hits = 0;
        targetsPresented = 0;
        firstShotHits = 0;
        overshootEvents = 0;
        undershootEvents = 0;
        settleEvents = 0;
    }

    public FlickMetrics Clone()
    {
        return new FlickMetrics
        {
            shots = shots,
            hits = hits,
            targetsPresented = targetsPresented,
            firstShotHits = firstShotHits,
            overshootEvents = overshootEvents,
            undershootEvents = undershootEvents,
            settleEvents = settleEvents
        };
    }
}

[Serializable]
public class TrackMetrics
{
    public float totalSeconds;
    public float onTargetSeconds;
    public float overshootSeconds;
    public float undershootSeconds;
    public float alignedSeconds;
    public float signedLagSum;
    public int lagSamples;

    public float StayRatio => totalSeconds <= 0.001f ? 0f : onTargetSeconds / totalSeconds;

    float LeadLagSampleSeconds => overshootSeconds + undershootSeconds + alignedSeconds;

    public float OvershootRate =>
        LeadLagSampleSeconds <= 0.001f ? 0f : overshootSeconds / LeadLagSampleSeconds;

    public float UndershootRate =>
        LeadLagSampleSeconds <= 0.001f ? 0f : undershootSeconds / LeadLagSampleSeconds;

    public float MeanLagDegrees => lagSamples <= 0 ? 0f : signedLagSum / lagSamples;
}

/// <summary>
/// 按过冲/欠冲调参。拉枪默认每 3 靶一批自动调一次。
/// </summary>
public static class SensitivityAdvisor
{
    public const int TargetAdaptationRounds = 3;
    public const int TargetsPerTune = 3;

    const float RateGapTrigger = 0.08f;
    const float FlickBiasDeadzone = 0.22f;
    const float TrackLagDeadzoneDeg = 0.35f;

    public static void EvaluateLeadLag(
        float over,
        float under,
        float netSignal,
        float netDeadzone,
        out int direction,
        out float intensity)
    {
        direction = 0;
        intensity = 0f;

        float gap = under - over;
        float gapIntensity = 0f;
        if (Mathf.Abs(gap) >= RateGapTrigger)
            gapIntensity = Mathf.Clamp01(Mathf.InverseLerp(RateGapTrigger, 0.35f, Mathf.Abs(gap)));

        float netIntensity = 0f;
        if (Mathf.Abs(netSignal) >= netDeadzone)
            netIntensity = Mathf.Clamp01(Mathf.InverseLerp(netDeadzone, netDeadzone * 4f, Mathf.Abs(netSignal)));

        if (gapIntensity < 0.01f && netIntensity < 0.01f)
            return;

        if (Mathf.Abs(gap) >= RateGapTrigger)
            direction = gap > 0f ? 1 : -1;
        else if (Mathf.Abs(netSignal) >= netDeadzone)
            direction = netSignal > 0f ? 1 : -1;
        else
            return;

        float alignedBoost = 1f;
        if (netIntensity > 0.01f)
        {
            int netDir = netSignal > 0f ? 1 : -1;
            alignedBoost = netDir == direction ? 1f + 0.35f * netIntensity : Mathf.Lerp(1f, 0.45f, netIntensity);
        }

        intensity = Mathf.Clamp01(Mathf.Max(gapIntensity, netIntensity * 0.85f) * alignedBoost);
        float dominant = direction > 0 ? under : over;
        intensity = Mathf.Clamp01(intensity * Mathf.Lerp(0.75f, 1.15f, Mathf.InverseLerp(0.2f, 0.55f, dominant)));
    }

    static float StepForIntensity(float intensity, int adaptationRound)
    {
        float baseStep = Mathf.Lerp(0.10f, 0.18f, Mathf.Clamp01(intensity));
        float roundScale = adaptationRound <= 0 ? 1f : adaptationRound == 1 ? 0.9f : 0.75f;
        return baseStep * roundScale;
    }

    static SensitivitySuggestion Build(
        int direction,
        float intensity,
        float over,
        float under,
        string contextPrefix,
        int adaptationRound)
    {
        if (direction == 0 || intensity < 0.01f)
            return default;

        float step = StepForIntensity(intensity, adaptationRound);
        int roundDisplay = adaptationRound + 1;

        if (direction > 0)
        {
            return new SensitivitySuggestion
            {
                horizontalMul = 1f + step,
                verticalMul = 1f + step * 0.9f,
                adsMul = 1f + step * 0.45f,
                accelDelta = Mathf.Lerp(0.02f, 0.05f, intensity),
                summary = $"提高灵敏度（{contextPrefix}欠冲）· 第{roundDisplay}批",
                detail = $"近{TargetsPerTune}靶：过冲 {over:P0} / 欠冲 {under:P0} → 水平 +{step:P0}"
            };
        }

        return new SensitivitySuggestion
        {
            horizontalMul = 1f - step,
            verticalMul = 1f - step * 0.9f,
            adsMul = 1f - step * 0.45f,
            accelDelta = Mathf.Lerp(-0.02f, -0.05f, intensity),
            summary = $"降低灵敏度（{contextPrefix}过冲）· 第{roundDisplay}批",
            detail = $"近{TargetsPerTune}靶：过冲 {over:P0} / 欠冲 {under:P0} → 水平 -{step:P0}"
        };
    }

    /// <summary>
    /// 每满 TargetsPerTune 个目标调用一次。
    /// 3 靶小样本要求至少 2 票同向才调，避免 2 欠 1 过就一路加灵敏度。
    /// </summary>
    public static SensitivitySuggestion FromFlickBatch(FlickMetrics m, int adaptationRound = 0)
    {
        int overN = m.overshootEvents;
        int underN = m.undershootEvents;
        int settleN = m.settleEvents;
        int n = Mathf.Max(1, overN + underN + settleN);
        float over = overN / (float)n;
        float under = underN / (float)n;

        // 至少 2/3 同向才动灵敏度
        if (underN >= 2 && underN > overN)
        {
            float intensity = Mathf.Clamp01(0.35f + 0.25f * (underN - overN));
            return Build(1, intensity, over, under, "拉枪", adaptationRound);
        }

        if (overN >= 2 && overN > underN)
        {
            float intensity = Mathf.Clamp01(0.35f + 0.25f * (overN - underN));
            return Build(-1, intensity, over, under, "拉枪", adaptationRound);
        }

        return SensitivitySuggestion.Identity(
            $"满{TargetsPerTune}靶：信号不强，本批不改",
            $"过冲{overN} / 欠冲{underN} / 干净{settleN}（需≥2票同向才调参）。");
    }

    public static SensitivitySuggestion FromTrack(TrackMetrics m, int adaptationRound = 0)
    {
        if (m.totalSeconds < 5f || m.lagSamples < 30)
            return SensitivitySuggestion.Identity("样本不足", "再多跟几秒移动靶。");

        float over = m.OvershootRate;
        float under = m.UndershootRate;
        float meanLag = m.MeanLagDegrees;

        EvaluateLeadLag(over, under, meanLag, TrackLagDeadzoneDeg, out int dir, out float intensity);
        var sug = Build(dir, intensity, over, under, "跟枪", adaptationRound);
        if (!string.IsNullOrEmpty(sug.summary))
            return sug;

        return SensitivitySuggestion.Identity(
            "当前参数较均衡",
            $"过冲 {over:P0} / 欠冲 {under:P0}，平均滞后 {meanLag:+0.00;-0.00}°。");
    }
}
