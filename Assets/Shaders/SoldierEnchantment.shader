Shader "Custom/SoldierEnchantment"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Base Tint", Color) = (1,1,1,1)

        _EnchantColor ("Enchant Color", Color) = (0.4, 0.8, 1, 0.35)
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveFrequency ("Wave Frequency", Float) = 8.0
        _WaveWidth ("Wave Width", Range(0.01, 1.0)) = 0.2
        _WaveStrength ("Wave Strength", Range(0, 1)) = 0.5
        _DiagonalAmount ("Diagonal Amount", Float) = 1.0
        _EnchantEnabled ("Enchant Enabled", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;

            fixed4 _EnchantColor;
            float _WaveSpeed;
            float _WaveFrequency;
            float _WaveWidth;
            float _WaveStrength;
            float _DiagonalAmount;
            float _EnchantEnabled;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, IN.uv) * IN.color;

                // Sprite alpha is the mask
                float mask = baseCol.a;

                // Diagonal coordinate
                float diag = IN.uv.x + (IN.uv.y * _DiagonalAmount);

                // Moving wave
                float wave = frac(diag * _WaveFrequency - _Time.y * _WaveSpeed);

                // Turn wave into a soft band
                float band = smoothstep(0.0, _WaveWidth, wave) *
                             (1.0 - smoothstep(_WaveWidth, _WaveWidth * 2.0, wave));

                // Only show effect inside sprite shape
                float effect = band * _WaveStrength * mask * _EnchantEnabled;

                fixed3 finalRgb = lerp(baseCol.rgb, baseCol.rgb + _EnchantColor.rgb, effect);
                float finalA = baseCol.a;

                return fixed4(finalRgb, finalA);
            }
            ENDCG
        }
    }
}