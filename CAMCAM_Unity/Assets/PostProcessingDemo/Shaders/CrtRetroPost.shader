Shader "KASA/PostProcessing/CRT Retro"
{
    Properties
    {
        [PerRendererData] _MainTex ("Source", 2D) = "white" {}
        _Curvature ("Curvature", Range(0, 0.25)) = 0.08
        _ScanlineStrength ("Scanline Strength", Range(0, 1)) = 0.32
        _ChromaticAberration ("Chromatic Aberration", Range(0, 4)) = 1.25
        _NoiseStrength ("Noise Strength", Range(0, 0.25)) = 0.035
        _Vignette ("Vignette", Range(0, 2)) = 0.85
        _Brightness ("Brightness", Range(0.5, 2)) = 1.15
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
            float _Curvature;
            float _ScanlineStrength;
            float _ChromaticAberration;
            float _NoiseStrength;
            float _Vignette;
            float _Brightness;

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

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 centered = i.uv * 2.0 - 1.0;
                float2 warped = centered * (1.0 + dot(centered, centered) * _Curvature);
                float2 uv = warped * 0.5 + 0.5;

                float outside = step(uv.x, 0.0) + step(1.0, uv.x) + step(uv.y, 0.0) + step(1.0, uv.y);
                float2 offset = float2(_MainTex_TexelSize.x * _ChromaticAberration, 0.0);
                float3 color;
                color.r = tex2D(_MainTex, uv + offset).r;
                color.g = tex2D(_MainTex, uv).g;
                color.b = tex2D(_MainTex, uv - offset).b;

                float scanline = 1.0 - _ScanlineStrength * (0.5 + 0.5 * sin(uv.y * _MainTex_TexelSize.w * 3.14159265));
                float grille = 0.94 + 0.06 * sin(uv.x * _MainTex_TexelSize.z * 2.094);
                float noise = (hash(floor(uv * _MainTex_TexelSize.zw) + floor(_Time.y * 30.0)) - 0.5) * _NoiseStrength;
                float vignette = saturate(1.0 - dot(centered * centered, centered * centered) * _Vignette);

                color = (color * scanline * grille + noise) * vignette * _Brightness;
                color *= 1.0 - saturate(outside);
                return fixed4(color, 1.0) * i.color;
            }
            ENDCG
        }
    }
}
