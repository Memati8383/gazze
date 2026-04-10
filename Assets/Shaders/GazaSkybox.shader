Shader "Custom/GazaSkybox_URP"
{
    Properties
    {
        [Header(Sky Colors)]
        _SkyColor ("Sky Top Color", Color) = (0.2, 0.22, 0.25, 1)
        _HorizonColor ("Horizon Color (Haze)", Color) = (0.5, 0.4, 0.3, 1)
        _GroundColor ("Bottom Color", Color) = (0.1, 0.08, 0.06, 1)
        
        [Header(Smoke and Ash Settings)]
        _SmokeIntensity ("Smoke Intensity", Range(0, 1)) = 0.4
        _SmokeScale ("Smoke Scale", Float) = 3.0
        _SmokeColor ("Smoke Tint", Color) = (0.3, 0.25, 0.2, 1)
        _AshIntensity ("Ash Particle Intensity", Range(0, 1)) = 0.2
        
        [Header(Atmospheric Glow)]
        _GlowIntensity ("Horizon Glow", Range(0, 5)) = 1.5
        _GlowColor ("Glow Color", Color) = (1, 0.4, 0.1, 1)
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 viewDir    : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _SkyColor;
                half4 _HorizonColor;
                half4 _GroundColor;
                half  _SmokeIntensity;
                float _SmokeScale;
                half4 _SmokeColor;
                half  _AshIntensity;
                half  _GlowIntensity;
                half4 _GlowColor;
            CBUFFER_END

            // Simplex-style noise for smoke
            inline float hash(float3 p) {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            inline float noise(float3 p) {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = hash(i);
                float b = hash(i + float3(1.0, 0.0, 0.0));
                float c = hash(i + float3(0.0, 1.0, 0.0));
                float d = hash(i + float3(1.0, 1.0, 0.0));
                float e = hash(i + float3(0.0, 0.0, 1.0));
                float f1 = hash(i + float3(1.0, 0.0, 1.0));
                float g = hash(i + float3(0.0, 1.0, 1.0));
                float h = hash(i + float3(1.0, 1.0, 1.0));

                return lerp(lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y),
                            lerp(lerp(e, f1, f.x), lerp(g, h, f.x), f.y), f.z);
            }

            inline float fbm(float3 p) {
                float v = 0.0;
                float a = 0.5;
                for (int i = 0; i < 4; i++) {
                    v += a * noise(p);
                    p = p * 2.1;
                    a = a * 0.5;
                }
                return v;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.viewDir = input.positionOS.xyz;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 v = normalize(input.viewDir);
                float y = v.y;

                // 1. Basic Three-Way Gradient
                half3 sky = lerp(_HorizonColor.rgb, _SkyColor.rgb, saturate(y * 4.0));
                sky = lerp(sky, _GroundColor.rgb, saturate(-y * 2.0));

                // 2. Horizon Glow (Warm ember/sunset look though dust)
                float glow = pow(saturate(1.0 - abs(y - 0.05) * 5.0), 4.0);
                sky += _GlowColor.rgb * glow * _GlowIntensity;

                // 3. Dynamic Smoke Layer
                // Rotate noise based on time for subtle movement
                float3 smokePos = v * _SmokeScale;
                smokePos.z += _Time.y * 0.05; 
                float s = fbm(smokePos);
                float smokeMask = saturate((s - 0.4) * _SmokeIntensity * 5.0);
                // Lower smoke visibility at the top (keep Zenith cleaner)
                smokeMask *= saturate(1.0 - y);
                sky = lerp(sky, _SmokeColor.rgb, smokeMask);

                // 4. Ash Specks (Flickering tiny dots)
                float ashNoise = hash(v * 500.0);
                float ash = step(0.998 - (_AshIntensity * 0.01), ashNoise);
                float ashFlicker = sin(_Time.y * 5.0 + ashNoise * 10.0) * 0.5 + 0.5;
                sky += half3(1, 1, 1) * ash * ashFlicker * _AshIntensity;

                return half4(sky, 1.0);
            }
            ENDHLSL
        }
    }
}
