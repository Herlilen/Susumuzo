Shader "KASA/PostProcessing/ASCII"
{
    Properties
    {
        [PerRendererData] _MainTex ("Source", 2D) = "white" {}
        _CellSize ("Cell Size", Range(5, 20)) = 8
        _Contrast ("Contrast", Range(0.5, 2.5)) = 1.25
        _BlockStrength ("Color Block Strength", Range(0, 1)) = 0.58
        _ColorSteps ("Color Steps", Range(2, 12)) = 6
        _Background ("Background", Color) = (0.015, 0.025, 0.02, 1)
        _Tint ("Glyph Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _CellSize;
            float _Contrast;
            float _BlockStrength;
            float _ColorSteps;
            float4 _Background;
            float4 _Tint;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            float letterRow(float letter, float row)
            {
                // Five-bit rows for eight 5x7 capital letters: A C E M N R X Z.
                if (letter < 0.5) // A
                {
                    if (row < 0.5) return 14; if (row < 1.5) return 17;
                    if (row < 2.5) return 17; if (row < 3.5) return 31;
                    return 17;
                }
                if (letter < 1.5) // C
                {
                    if (row < 0.5) return 14; if (row < 1.5) return 17;
                    if (row < 5.5) return 16; return 15;
                }
                if (letter < 2.5) // E
                {
                    if (row < 0.5 || row > 5.5) return 31;
                    if (row > 2.5 && row < 3.5) return 30;
                    return 16;
                }
                if (letter < 3.5) // M
                {
                    if (row < 0.5) return 17; if (row < 1.5) return 27;
                    if (row < 2.5) return 21; return 17;
                }
                if (letter < 4.5) // N
                {
                    if (row < 0.5) return 17; if (row < 1.5) return 25;
                    if (row < 3.5) return 21; if (row < 4.5) return 19;
                    return 17;
                }
                if (letter < 5.5) // R
                {
                    if (row < 0.5 || (row > 2.5 && row < 3.5)) return 30;
                    if (row < 2.5) return 17; if (row < 4.5) return 20;
                    if (row < 5.5) return 18; return 17;
                }
                if (letter < 6.5) // X
                {
                    if (row < 0.5 || row > 5.5) return 17;
                    if (row < 2.5 || row > 3.5) return 10;
                    return 4;
                }
                // Z
                if (row < 0.5 || row > 5.5) return 31;
                if (row < 1.5) return 1; if (row < 2.5) return 2;
                if (row < 3.5) return 4; if (row < 4.5) return 8;
                return 16;
            }

            float letterGlyph(float2 cellPosition, float letter)
            {
                // Leave a slim gap between cells, then rasterize a real 5x7 glyph.
                if (cellPosition.x < 0.08 || cellPosition.x > 0.92 ||
                    cellPosition.y < 0.08 || cellPosition.y > 0.92)
                    return 0.0;

                float x = floor(saturate((cellPosition.x - 0.08) / 0.84) * 5.0);
                float y = floor((1.0 - saturate((cellPosition.y - 0.08) / 0.84)) * 7.0);
                x = min(x, 4.0);
                y = min(y, 6.0);
                float rowBits = letterRow(letter, y);
                float bitValue = floor(rowBits / exp2(4.0 - x));
                return fmod(bitValue, 2.0);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 pixel = i.uv / _MainTex_TexelSize.xy;
                float2 cell = floor(pixel / _CellSize);
                float2 cellUv = (cell + 0.5) * _CellSize * _MainTex_TexelSize.xy;
                float3 source = tex2D(_MainTex, cellUv).rgb;
                float luminance = saturate((dot(source, float3(0.2126, 0.7152, 0.0722)) - 0.5) * _Contrast + 0.5);
                float2 local = frac(pixel / _CellSize);
                float randomValue = frac(sin(dot(cell, float2(12.9898, 78.233))) * 43758.5453);
                float letter = floor(frac(randomValue + luminance * 0.73) * 8.0);
                float presence = step(0.08 + randomValue * 0.42, luminance);
                float mask = letterGlyph(local, letter) * presence;

                // Retain a simplified block of the source image underneath each
                // glyph. Quantized colour makes silhouettes readable without
                // losing the deliberately low-resolution ASCII character grid.
                float3 blockColor = floor(source * _ColorSteps + 0.5) / _ColorSteps;
                float blockPresence = smoothstep(0.025, 0.14, luminance);
                float3 color = lerp(_Background.rgb, blockColor, blockPresence * _BlockStrength);
                float3 glyphColor = lerp(_Tint.rgb, source * _Tint.rgb * 1.55, 0.78);
                color = lerp(color, glyphColor, mask);
                return fixed4(color, 1.0) * i.color;
            }
            ENDCG
        }
    }
}
