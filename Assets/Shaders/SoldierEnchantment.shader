Shader "Custom/SpriteOutlineMapRenderer"
{
    Properties
    {
        _MainTex ("Sprite Atlas", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "black" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _SpriteRect ("Sprite Rect XYWH", Vector) = (0,0,1,1)
        _AlphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.001
        _ShowEnchant ("Show Enchant", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
                fixed4 color    : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float4 _SpriteRect;
            float _AlphaCutoff;
            float _ShowEnchant;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 atlasUV = i.uv;

                float2 localUV;
                localUV.x = (atlasUV.x - _SpriteRect.x) / _SpriteRect.z;
                localUV.y = (atlasUV.y - _SpriteRect.y) / _SpriteRect.w;

                if (localUV.x < 0 || localUV.x > 1 || localUV.y < 0 || localUV.y > 1)
                    discard;

                fixed4 spriteCol = tex2D(_MainTex, atlasUV) * i.color;
                fixed4 maskCol = tex2D(_MaskTex, localUV);

                bool isVisiblePixel = maskCol.r > 0.5;
                bool isOutlinePixel = (maskCol.g > 0.5) && (_ShowEnchant > 0.5);

                if (!isVisiblePixel && !isOutlinePixel)
                    discard;

                if (isVisiblePixel)
                {
                    if (spriteCol.a <= _AlphaCutoff)
                        discard;

                    return spriteCol;
                }

                return _OutlineColor;
            }
            ENDCG
        }
    }
}