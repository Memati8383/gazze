// ═══════════════════════════════════════════════════════════════════════
//  VehicleShader_URP — Traffic vehicle & player car shader
//
//  Features:
//    • Procedural dirt/grime + edge wear
//    • Matcap reflections (corrected)
//    • Rim light, occlusion map, emission
//    • Curved-world vertex deformation
//    • Full URP pass set: ForwardLit, ShadowCaster, DepthOnly, DepthNormals
//    • GPU instancing compatible
// ═══════════════════════════════════════════════════════════════════════
Shader "Custom/VehicleShader_URP"
{
    Properties
    {
        [Header(Base Surface)]
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" {}
        [MainColor]   _BaseColor ("Base Color", Color) = (1, 1, 1, 1)

        [Header(Normal Map)]
        [Toggle(_NORMALMAP)] _UseNormalMap ("Enable Normal Map", Float) = 0
        [NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Range(0, 2)) = 1.0

        [Header(Occlusion)]
        [Toggle(_OCCLUSIONMAP)] _UseOcclusionMap ("Enable Occlusion", Float) = 0
        [NoScaleOffset] _OcclusionMap ("Occlusion Map", 2D) = "white" {}
        _OcclusionStrength ("Occlusion Strength", Range(0, 1)) = 1.0

        [Header(Matcap Reflection)]
        [Toggle(_MATCAP)] _UseMatcap ("Enable Matcap", Float) = 0
        _MatcapTex ("Matcap Texture", 2D) = "black" {}
        _MatcapColor ("Matcap Color", Color) = (1, 1, 1, 1)
        _MatcapIntensity ("Matcap Intensity", Range(0, 2)) = 1.0

        [Header(Rim Light)]
        [Toggle(_RIMLIGHT)] _UseRimLight ("Enable Rim Light", Float) = 0
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(0.1, 8)) = 3.0
        _RimIntensity ("Rim Intensity", Range(0, 5)) = 1.0

        [Header(Dirt and Rust)]
        _DirtColor ("Dirt Color", Color) = (0.2, 0.1, 0.05, 1)
        _DirtAmount ("Dirt Amount", Range(0, 1)) = 0.3
        _DirtScale ("Dirt Scale", Float) = 5.0
        _DirtPower ("Dirt Sharpness", Range(0, 5)) = 1.0

        [Header(Edge Wear)]
        _WearColor ("Wear Color", Color) = (0.8, 0.8, 0.8, 1)
        _WearStrength ("Wear Strength", Range(0, 5)) = 0.5
        _WearPower ("Wear Falloff", Range(0.1, 10)) = 2.0

        [Header(Surface)]
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0

        [Header(Emission)]
        [Toggle(_EMISSION)] _UseEmission ("Enable Emission", Float) = 0
        [NoScaleOffset] _EmissionMap ("Emission Map", 2D) = "black" {}
        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 0)

        [Header(Curved World)]
        _Curvature     ("Curvature V",    Float) = 0.002
        _CurvatureH    ("Curvature H",    Float) = -0.0015
        _HorizonOffset ("Horizon Offset", Float) = 10.0

        [Header(Rendering)]
        [Toggle(_ALPHATEST_ON)] _AlphaClip ("Alpha Clipping", Float) = 0
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 200

        HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Shaders/Include/ProceduralNoise.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                half   _BumpScale;
                half   _OcclusionStrength;
                half4  _MatcapColor;
                half   _MatcapIntensity;
                half4  _RimColor;
                half   _RimPower;
                half   _RimIntensity;
                half4  _DirtColor;
                half   _DirtAmount;
                half   _DirtScale;
                half   _DirtPower;
                half4  _WearColor;
                half   _WearStrength;
                half   _WearPower;
                half4  _EmissionColor;
                half   _Smoothness;
                half   _Metallic;
                float  _Curvature;
                float  _CurvatureH;
                float  _HorizonOffset;
                half   _Cutoff;
            CBUFFER_END

            // ── Curved World ──────────────────────────────────────
            inline float3 CurveWorldPos(float3 posWS)
            {
                float distZ = max(0.0, posWS.z - _WorldSpaceCameraPos.z - _HorizonOffset);
                float d2 = distZ * distZ;
                posWS.y -= d2 * _Curvature;
                posWS.x += d2 * _CurvatureH;
                return posWS;
            }

            inline float3 CurveWorldNrm(float3 nrmWS, float3 posWS)
            {
                float distZ = max(0.0, posWS.z - _WorldSpaceCameraPos.z - _HorizonOffset);
                float dydz = -2.0 * distZ * _Curvature;
                float dxdz =  2.0 * distZ * _CurvatureH;
                float3 c = nrmWS;
                c.z -= dydz * nrmWS.y;
                c.z -= dxdz * nrmWS.x;
                return normalize(c);
            }
        ENDHLSL

        // ════════════════════════════════════════════════════════════
        //  Pass 1 — Forward Lit
        // ════════════════════════════════════════════════════════════
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _OCCLUSIONMAP
            #pragma shader_feature_local _MATCAP
            #pragma shader_feature_local _RIMLIGHT
            #pragma shader_feature_local _EMISSION
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                float3 objectPos   : TEXCOORD3;
                float  fogFactor   : TEXCOORD4;
                float4 tangentWS   : TEXCOORD5;
                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                float4 shadowCoord : TEXCOORD6;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap);      SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);      SAMPLER(sampler_BumpMap);
            TEXTURE2D(_OcclusionMap); SAMPLER(sampler_OcclusionMap);
            TEXTURE2D(_MatcapTex);    SAMPLER(sampler_MatcapTex);
            TEXTURE2D(_EmissionMap);  SAMPLER(sampler_EmissionMap);

            Varyings Vert(Attributes i)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_TRANSFER_INSTANCE_ID(i, o);

                float3 posWS = TransformObjectToWorld(i.positionOS.xyz);
                o.objectPos = i.positionOS.xyz;

                posWS = CurveWorldPos(posWS);
                o.positionCS = TransformWorldToHClip(posWS);
                o.positionWS = posWS;

                float3 nrmWS = TransformObjectToWorldNormal(i.normalOS);
                o.normalWS = CurveWorldNrm(nrmWS, posWS);

                o.tangentWS = float4(TransformObjectToWorldDir(i.tangentOS.xyz), i.tangentOS.w);
                o.uv = TRANSFORM_TEX(i.uv, _BaseMap);
                o.fogFactor = ComputeFogFactor(o.positionCS.z);

                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                o.shadowCoord = TransformWorldToShadowCoord(posWS);
                #endif

                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;

                #ifdef _ALPHATEST_ON
                clip(albedo.a - _Cutoff);
                #endif

                float3 viewDir = normalize(_WorldSpaceCameraPos - i.positionWS);

                // Normal mapping
                float3 normalWS = normalize(i.normalWS);
                #ifdef _NORMALMAP
                float3 tangentWS = normalize(i.tangentWS.xyz);
                float3 bitangentWS = cross(normalWS, tangentWS) * i.tangentWS.w;
                float3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uv));
                normalTS.xy *= _BumpScale;
                normalWS = normalize(tangentWS * normalTS.x + bitangentWS * normalTS.y + normalWS * normalTS.z);
                #endif

                // Occlusion
                half occlusion = 1.0;
                #ifdef _OCCLUSIONMAP
                occlusion = lerp(1.0, SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, i.uv).r, _OcclusionStrength);
                #endif

                // ── Procedural Dirt ──
                float noiseVal = FBM3D(i.objectPos * _DirtScale);
                float dirtMask = saturate(pow(max(0.0, noiseVal * _DirtAmount * 2.5), _DirtPower));
                float vertDirt = saturate((1.2 - i.objectPos.y) * _DirtAmount);
                dirtMask = saturate(dirtMask + vertDirt * 0.4);
                albedo.rgb = lerp(albedo.rgb, _DirtColor.rgb, dirtMask);

                // ── Edge Wear ──
                float fresnel = 1.0 - saturate(dot(normalWS, viewDir));
                float wearMask = pow(max(0.0, fresnel), _WearPower) * _WearStrength;
                wearMask *= SmoothThreshold(noiseVal, 0.4, 0.1);
                albedo.rgb = lerp(albedo.rgb, _WearColor.rgb, wearMask);

                // ── Main Light ──
                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                Light mainLight = GetMainLight(i.shadowCoord);
                #else
                Light mainLight = GetMainLight();
                #endif

                float NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 ambient = SampleSH(normalWS) * occlusion;
                half3 diffuse = mainLight.color * NdotL * mainLight.shadowAttenuation;

                // Specular (roughness reduced by dirt)
                float smoothness = _Smoothness * (1.0 - dirtMask * 0.8);
                float3 halfDir = normalize(mainLight.direction + viewDir);
                float NdotH = saturate(dot(normalWS, halfDir));
                float specPower = exp2(10.0 * smoothness + 1.0);

                half3 specColor = lerp(half3(0.04, 0.04, 0.04), albedo.rgb, _Metallic);
                half3 specular = specColor * pow(max(0.0, NdotH), specPower)
                               * mainLight.color * mainLight.shadowAttenuation * smoothness;

                // ── Matcap Reflections ──
                #ifdef _MATCAP
                // FIXED: was `float33` — now correct `float3x3`
                float3 viewNormal = mul((float3x3)GetWorldToViewMatrix(), normalWS);
                float2 matcapUV = viewNormal.xy * 0.5 + 0.5;
                half3 matcap = SAMPLE_TEXTURE2D(_MatcapTex, sampler_MatcapTex, matcapUV).rgb * _MatcapColor.rgb;
                specular += matcap * _MatcapIntensity * smoothness * (1.0 - dirtMask);
                #endif

                // ── Rim Light ──
                half3 rim = half3(0, 0, 0);
                #ifdef _RIMLIGHT
                rim = pow(max(0.0, fresnel), _RimPower) * _RimColor.rgb * _RimIntensity * (1.0 - dirtMask);
                #endif

                // ── Additional Lights ──
                half3 addDiffuse = half3(0, 0, 0);
                #ifdef _ADDITIONAL_LIGHTS
                uint lightCount = GetAdditionalLightsCount();
                for (uint li = 0; li < lightCount; li++)
                {
                    Light addLight = GetAdditionalLight(li, i.positionWS);
                    float addNdotL = saturate(dot(normalWS, addLight.direction));
                    addDiffuse += addLight.color * addNdotL
                                * addLight.distanceAttenuation * addLight.shadowAttenuation;
                }
                #endif

                // Emission
                half3 emission = half3(0, 0, 0);
                #ifdef _EMISSION
                emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, i.uv).rgb * _EmissionColor.rgb;
                #endif

                half3 finalRGB = albedo.rgb * (diffuse + addDiffuse + ambient) + specular + emission + rim;
                finalRGB = MixFog(finalRGB, i.fogFactor);

                return half4(finalRGB, albedo.a);
            }
            ENDHLSL
        }

        // ════════════════════════════════════════════════════════════
        //  Pass 2 — Shadow Caster
        // ════════════════════════════════════════════════════════════
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On  ZTest LEqual  ColorMask 0  Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                #ifdef _ALPHATEST_ON
                float2 uv : TEXCOORD0;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float3 _LightDirection;

            ShadowVaryings ShadowVert(ShadowAttributes v)
            {
                ShadowVaryings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
                posWS = CurveWorldPos(posWS);

                float3 nrmWS = TransformObjectToWorldNormal(v.normalOS);
                nrmWS = CurveWorldNrm(nrmWS, posWS);

                o.positionCS = TransformWorldToHClip(ApplyShadowBias(posWS, nrmWS, _LightDirection));

                #if UNITY_REVERSED_Z
                o.positionCS.z = min(o.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                o.positionCS.z = max(o.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                #ifdef _ALPHATEST_ON
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                #endif

                return o;
            }

            half4 ShadowFrag(ShadowVaryings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                #ifdef _ALPHATEST_ON
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                clip(col.a - _Cutoff);
                #endif
                return 0;
            }
            ENDHLSL
        }

        // ════════════════════════════════════════════════════════════
        //  Pass 3 — Depth Only
        // ════════════════════════════════════════════════════════════
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On  ColorMask R  Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile_instancing

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
                #ifdef _ALPHATEST_ON
                float2 uv : TEXCOORD0;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            DepthVaryings DepthVert(DepthAttributes v)
            {
                DepthVaryings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
                posWS = CurveWorldPos(posWS);
                o.positionCS = TransformWorldToHClip(posWS);

                #ifdef _ALPHATEST_ON
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                #endif
                return o;
            }

            half4 DepthFrag(DepthVaryings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                #ifdef _ALPHATEST_ON
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                clip(col.a - _Cutoff);
                #endif
                return 0;
            }
            ENDHLSL
        }

        // ════════════════════════════════════════════════════════════
        //  Pass 4 — Depth Normals
        // ════════════════════════════════════════════════════════════
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On  Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex DNVert
            #pragma fragment DNFrag
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile_instancing

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);

            struct DNAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct DNVaryings
            {
                float4 positionCS  : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float2 uv          : TEXCOORD1;
                #ifdef _NORMALMAP
                float3 tangentWS   : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            DNVaryings DNVert(DNAttributes v)
            {
                DNVaryings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
                posWS = CurveWorldPos(posWS);
                o.positionCS = TransformWorldToHClip(posWS);

                float3 nrmWS = TransformObjectToWorldNormal(v.normalOS);
                o.normalWS = CurveWorldNrm(nrmWS, posWS);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);

                #ifdef _NORMALMAP
                float3 tanWS = TransformObjectToWorldDir(v.tangentOS.xyz);
                o.tangentWS = tanWS;
                o.bitangentWS = cross(o.normalWS, tanWS) * v.tangentOS.w;
                #endif

                return o;
            }

            half4 DNFrag(DNVaryings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                #ifdef _ALPHATEST_ON
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                clip(col.a - _Cutoff);
                #endif

                float3 normalWS = normalize(i.normalWS);
                #ifdef _NORMALMAP
                float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uv), _BumpScale);
                float3x3 TBN = float3x3(normalize(i.tangentWS), normalize(i.bitangentWS), normalWS);
                normalWS = normalize(mul(normalTS, TBN));
                #endif

                return half4(normalWS, 0.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
