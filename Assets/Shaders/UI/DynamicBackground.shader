Shader "Gazze/UI/DynamicBackground"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _WaveSpeed ("Wave Speed", Float) = 0.3
        _WaveStrength ("Wave Strength", Float) = 0.012
        _GodRaySpeed ("God Ray Speed", Float) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

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
                float2 texcoord  : TEXCOORD0;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float _WaveSpeed;
            float _WaveStrength;
            float _GodRaySpeed;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                
                // Subtle world space distortion for "dust movement"
                float2 uv = IN.texcoord;
                uv.x += sin(_Time.y * _WaveSpeed + uv.y * 5.0) * _WaveStrength;
                uv.y += cos(_Time.y * _WaveSpeed * 0.8 + uv.x * 3.0) * _WaveStrength;
                
                OUT.texcoord = uv;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // Add a very subtle moving "light beam" overlay (simulated God Ray)
                float ray = sin(IN.texcoord.x * 2.0 - IN.texcoord.y * 1.5 + _Time.y * _GodRaySpeed) * 0.5 + 0.5;
                ray = pow(ray, 10.0); // Make it sharp
                color.rgb += ray * 0.05; // Very subtle addition
                
                color.rgb *= color.a;
                return color;
            }
        ENDCG
        }
    }
}
