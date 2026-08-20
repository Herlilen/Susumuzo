using System;
using UnityEngine;

/// <summary>
/// 灵敏度参数 + 面向玩家的直观说明（痛点3）。
/// </summary>
[Serializable]
public class SensitivityProfile
{
    [Range(0.05f, 5f)] public float horizontal = 1.2f;
    [Range(0.05f, 5f)] public float vertical = 1.2f;
    [Range(0.2f, 1.5f)] public float adsMultiplier = 0.75f;
    [Range(0f, 1f)] public float acceleration = 0.15f;

    public const string HorizontalHelp =
        "水平灵敏度：控制左右甩枪速度。过高容易过冲（拉过头），过低容易欠冲（拉不到）。";
    public const string VerticalHelp =
        "垂直灵敏度：控制上下抬枪/压枪速度。建议与水平接近，避免瞄准轨迹“椭圆变形”。";
    public const string AdsHelp =
        "开镜倍率：开镜时相对腰射的灵敏度比例。数值越低，开镜越稳，适合跟枪。";
    public const string AccelHelp =
        "鼠标加速：大幅甩动时额外提速。利于快速拉枪，但跟枪时更容易过头。";

    public SensitivityProfile Clone()
    {
        return new SensitivityProfile
        {
            horizontal = horizontal,
            vertical = vertical,
            adsMultiplier = adsMultiplier,
            acceleration = acceleration
        };
    }

    public void CopyFrom(SensitivityProfile other)
    {
        horizontal = other.horizontal;
        vertical = other.vertical;
        adsMultiplier = other.adsMultiplier;
        acceleration = other.acceleration;
    }

    public void ApplySuggestion(SensitivitySuggestion suggestion)
    {
        horizontal = Mathf.Clamp(horizontal * suggestion.horizontalMul, 0.05f, 5f);
        vertical = Mathf.Clamp(vertical * suggestion.verticalMul, 0.05f, 5f);
        adsMultiplier = Mathf.Clamp(adsMultiplier * suggestion.adsMul, 0.2f, 1.5f);
        acceleration = Mathf.Clamp01(acceleration + suggestion.accelDelta);
    }
}

[Serializable]
public struct SensitivitySuggestion
{
    public float horizontalMul;
    public float verticalMul;
    public float adsMul;
    public float accelDelta;
    public string summary;
    public string detail;

    public static SensitivitySuggestion Identity(string summary, string detail)
    {
        return new SensitivitySuggestion
        {
            horizontalMul = 1f,
            verticalMul = 1f,
            adsMul = 1f,
            accelDelta = 0f,
            summary = summary,
            detail = detail
        };
    }
}
