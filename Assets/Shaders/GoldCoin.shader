Shader "Gazze/GoldCoin"
{
    Properties
    {
        _BaseColor    ("Base Color",    Color)      = (1, 0.82, 0.1, 1)
        _EmissionColor("Emission Color",Color)      = (1, 0.7, 0, 1)
        _EmissionIntensity("Emission Intensity", Float) = 1.5
        _Metallic     ("Metallic",      Range(0,1)) = 0.95
        _Smoothness   ("Smoothness",    Range(0,1)) = 0.88
        _FresnelPower ("Fresnel Power", Range(0.1, 8)) = 2.0
        _FresnelColor ("Fresnel Color", Color)      = (1, 1, 0.6, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 viewDirWS   : TEXCOORD2;
                float  fogCoord    : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _EmissionColor;
                float  _EmissionIntensity;
                float  _Metallic;
                float  _Smoothness;
                float  _FresnelPower;
                float4 _FresnelColor;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   vni = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = vpi.positionCS;
                OUT.positionWS = vpi.positionWS;
                OUT.normalWS   = vni.normalWS;
                OUT.viewDirWS  = GetWorldSpaceViewDir(vpi.positionWS);
                OUT.fogCoord   = ComputeFogFactor(vpi.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(IN.viewDirWS);

                // Fresnel rim
                float fresnel = pow(1.0 - saturate(dot(N, V)), _FresnelPower);
                float3 rimColor = _FresnelColor.rgb * fresnel;

                // Simple PBR-ish shading
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(N, mainLight.direction));
                float3 H    = normalize(V + mainLight.direction);
                float NdotH = saturate(dot(N, H));

                // Specular (Blinn-Phong approximation for gold glint)
                float spec = pow(NdotH, _Smoothness * 128.0) * _Metallic;

                float3 albedo   = _BaseColor.rgb;
                float3 diffuse  = albedo * mainLight.color * NdotL;
                float3 specular = spec * mainLight.color * albedo;
                float3 emission = _EmissionColor.rgb * _EmissionIntensity;

                float3 color = diffuse + specular + emission + rimColor;
                color = MixFog(color, IN.fogCoord);

                return half4(color, 1.0);
            }
            ENDHLSL
        }

        // Shadow caster
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex   ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
