using System;
using UnityEngine;

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
/// 按过冲/欠冲数据调参。单步幅度按「约 3 轮完成适配」设计。
/// </summary>
public static class SensitivityAdvisor
{
    public const int TargetAdaptationRounds = 3;

    const float RateGapTrigger = 0.08f;
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

        float primary = gapIntensity;
        float secondary = netIntensity;

        if (primary < 0.01f && secondary < 0.01f)
            return;

        if (Mathf.Abs(gap) >= RateGapTrigger)
            direction = gap > 0f ? 1 : -1;
        else if (Mathf.Abs(netSignal) >= netDeadzone)
            direction = netSignal > 0f ? 1 : -1;
        else
            return;

        float alignedBoost = 1f;
        if (secondary > 0.01f)
        {
            int netDir = netSignal > 0f ? 1 : -1;
            alignedBoost = netDir == direction ? 1f + 0.35f * secondary : Mathf.Lerp(1f, 0.45f, secondary);
        }

        intensity = Mathf.Clamp01(Mathf.Max(primary, secondary * 0.85f) * alignedBoost);

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
        int roundDisplay = Mathf.Clamp(adaptationRound + 1, 1, TargetAdaptationRounds);
        int left = Mathf.Max(0, TargetAdaptationRounds - roundDisplay);

        if (direction > 0)
        {
            return new SensitivitySuggestion
            {
                horizontalMul = 1f + step,
                verticalMul = 1f + step * 0.9f,
                adsMul = 1f + step * 0.45f,
                accelDelta = Mathf.Lerp(0.02f, 0.05f, intensity),
                summary = $"建议提高灵敏度（{contextPrefix}欠冲）· 第{roundDisplay}/{TargetAdaptationRounds}轮",
                detail = $"过冲 {over:P0} / 欠冲 {under:P0} → 水平 +{step:P0}。按约三轮适配设计" +
                         (left > 0 ? $"，建议再测 {left} 轮。" : "，本轮后应接近完成。")
            };
        }

        return new SensitivitySuggestion
        {
            horizontalMul = 1f - step,
            verticalMul = 1f - step * 0.9f,
            adsMul = 1f - step * 0.45f,
            accelDelta = Mathf.Lerp(-0.02f, -0.05f, intensity),
            summary = $"建议降低灵敏度（{contextPrefix}过冲）· 第{roundDisplay}/{TargetAdaptationRounds}轮",
            detail = $"过冲 {over:P0} / 欠冲 {under:P0} → 水平 -{step:P0}。按约三轮适配设计" +
                     (left > 0 ? $"，建议再测 {left} 轮。" : "，本轮后应接近完成。")
        };
    }

    public static SensitivitySuggestion FromTrack(TrackMetrics m, int adaptationRound = 0)
    {
        if (m.totalSeconds < 5f || m.lagSamples < 30)
            return SensitivitySuggestion.Identity("样本不足", "再多跟几秒移动靶，数据会更可靠。");

        float over = m.OvershootRate;
        float under = m.UndershootRate;
        float meanLag = m.MeanLagDegrees;
        float stay = m.StayRatio;

        EvaluateLeadLag(over, under, meanLag, TrackLagDeadzoneDeg, out int dir, out float intensity);
        var sug = Build(dir, intensity, over, under, "跟枪", adaptationRound);
        if (!string.IsNullOrEmpty(sug.summary))
            return sug;

        if (stay < 0.3f && adaptationRound < TargetAdaptationRounds)
        {
            float step = StepForIntensity(0.35f, adaptationRound);
            return new SensitivitySuggestion
            {
                horizontalMul = 1f + step,
                verticalMul = 1f + step * 0.9f,
                adsMul = 1f + step * 0.4f,
                accelDelta = 0.02f,
                summary = $"建议提高灵敏度（贴靶偏少）· 第{adaptationRound + 1}/{TargetAdaptationRounds}轮",
                detail = $"停留 {stay:P0}，过冲 {over:P0} / 欠冲 {under:P0}。过冲欠冲接近，但跟不上目标，按欠冲方向上调。"
            };
        }

        return SensitivitySuggestion.Identity(
            adaptationRound >= TargetAdaptationRounds - 1 ? "适配完成" : "当前参数较均衡",
            $"过冲 {over:P0} / 欠冲 {under:P0}，平均滞后 {meanLag:+0.00;-0.00}°，停留 {stay:P0}。无需再调。");
    }
}
