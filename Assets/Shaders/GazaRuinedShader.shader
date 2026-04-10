// ═══════════════════════════════════════════════════════════════════════
//  GazaRuinedShader_URP — War-torn building shader for the Gaza game
//
//  Designed for procedural & prefab buildings lining the infinite road.
//  Features:
//    • Multi-layer procedural weathering (dust, grime, cracks, soot)
//    • Curved-world vertex deformation (shared CurvedWorldCore include)
//    • Full URP pass set: ForwardLit, ShadowCaster, DepthOnly, DepthNormals
//    • Additional lights support
//    • Distance fade for LOD pop-in masking
//    • Normal map, emission, alpha clip support
//    • GPU instancing compatible
// ═══════════════════════════════════════════════════════════════════════
Shader "Custom/GazaRuinedShader_URP"
{
    Properties
    {
        [Header(Base Surface)]
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" {}
        [MainColor]   _BaseColor ("Base Color", Color) = (1, 1, 1, 1)

        [Header(Weathering)]
        _DirtColor       ("Dust Color",        Color)       = (0.72, 0.65, 0.50, 1)
        _DirtScale       ("Dust Noise Scale",  Float)       = 2.0
        _DirtIntensity   ("Dust Intensity",    Range(0, 3)) = 0.8
        _GrimeIntensity  ("Grime Intensity",   Range(0, 1)) = 0.55
        _WearAmount      ("Edge Wear",         Range(0, 1)) = 0.35

        [Header(Structural Damage)]
        _TopDustAmount   ("Top Dust Deposit",  Range(0, 2)) = 0.9
        _CrackIntensity  ("Crack Darkness",    Range(0, 2)) = 0.5
        _CrackScale      ("Crack Scale",       Float)       = 8.0
        _SootAmount      ("Soot / Burn Marks", Range(0, 1)) = 0.25
        _SootColor       ("Soot Color",        Color)       = (0.06, 0.05, 0.04, 1)

        [Header(Surface)]
        _Smoothness ("Smoothness",     Range(0, 1)) = 0.05
        _SpecColor  ("Specular Color", Color)       = (0.04, 0.04, 0.04, 1)
        _RimPower   ("Ashy Rim Power", Range(0.5, 8)) = 3.5

        [Header(Normal Map)]
        [Toggle(_NORMALMAP)] _UseNormalMap ("Enable Normal Map", Float) = 0
        [NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Range(0, 2)) = 1.0

        [Header(Emission)]
        [Toggle(_EMISSION)] _UseEmission ("Enable Emission", Float) = 0
        _EmissionMap       ("Emission Map",       2D)          = "white" {}
        [HDR] _EmissionColor ("Emission Color",    Color)       = (0, 0, 0, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 20)) = 1.0

        [Header(Curved World)]
        _Curvature     ("Curvature V",     Float) = 0.002
        _CurvatureH    ("Curvature H",     Float) = -0.0015
        _HorizonOffset ("Horizon Offset",  Float) = 10.0

        [Header(Distance Fade)]
        [Toggle(_DISTANCE_FADE)] _UseDistanceFade ("Enable Distance Fade", Float) = 0
        _FadeStart ("Fade Start", Float) = 200.0
        _FadeEnd   ("Fade End",   Float) = 400.0

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

        // ════════════════════════════════════════════════════════════
        //  Shared HLSL (accessible by all passes)
        // ════════════════════════════════════════════════════════════
        HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Shaders/Include/ProceduralNoise.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                half   _BumpScale;
                half   _Smoothness;
                half4  _SpecColor;
                half   _Cutoff;

                // Weathering
                float  _DirtScale;
                half4  _DirtColor;
                half   _DirtIntensity;
                half   _GrimeIntensity;
                half   _WearAmount;

                // Damage
                half   _TopDustAmount;
                half   _CrackIntensity;
                float  _CrackScale;
                half   _SootAmount;
                half4  _SootColor;
                half   _RimPower;

                // Emission
                half4  _EmissionColor;
                half   _EmissionIntensity;

                // Curved World (per-material override)
                float _Curvature;
                float _CurvatureH;
                float _HorizonOffset;

                // Distance Fade
                float _FadeStart;
                float _FadeEnd;
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

            // URP multi-compiles
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            // Material features
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _EMISSION
            #pragma shader_feature_local _DISTANCE_FADE
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalOS   : NORMAL;
                #ifdef _NORMALMAP
                float4 tangentOS  : TANGENT;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float3 positionWS   : TEXCOORD2;
                float  fogFactor    : TEXCOORD3;
                #ifdef _NORMALMAP
                float3 tangentWS    : TEXCOORD4;
                float3 bitangentWS  : TEXCOORD5;
                #endif
                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                float4 shadowCoord  : TEXCOORD6;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap);     SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);     SAMPLER(sampler_BumpMap);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);

            Varyings Vert(Attributes i)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_TRANSFER_INSTANCE_ID(i, o);

                float3 posWS = TransformObjectToWorld(i.positionOS.xyz);
                posWS = CurveWorldPos(posWS);

                o.positionCS  = TransformWorldToHClip(posWS);
                o.positionWS  = posWS;

                float3 nrmWS  = TransformObjectToWorldNormal(i.normalOS);
                o.normalWS    = CurveWorldNrm(nrmWS, posWS);

                #ifdef _NORMALMAP
                float3 tanWS  = TransformObjectToWorldDir(i.tangentOS.xyz);
                o.tangentWS   = tanWS;
                o.bitangentWS = cross(o.normalWS, tanWS) * i.tangentOS.w;
                #endif

                o.uv        = TRANSFORM_TEX(i.uv, _BaseMap);
                o.fogFactor = ComputeFogFactor(o.positionCS.z);

                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                o.shadowCoord = TransformWorldToShadowCoord(posWS);
                #endif

                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                // ── Noise coordinates (world-space for seamless tiling) ──
                float2 noiseUV = i.positionWS.xz * 0.5 + i.positionWS.y * 0.3;

                // ── Base albedo ──
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;

                // ════════════════════════════════════════════════════════
                //  PROCEDURAL WEATHERING STACK
                // ════════════════════════════════════════════════════════

                // 1. CRACKS — Voronoi-edge dark lines
                float crackDist = Voronoi2D(noiseUV * _CrackScale);
                float crackMask = saturate(1.0 - pow(crackDist, 0.12) * 2.2) * _CrackIntensity;
                albedo.rgb = lerp(albedo.rgb, albedo.rgb * 0.15, crackMask);

                // 2. GRIME — Deep stains in recesses
                float grimeNoise = FBM2D(noiseUV * _DirtScale);
                float grime = saturate(1.3 - grimeNoise * _GrimeIntensity * 2.0);
                albedo.rgb *= grime;

                // 3. TOP DUST — Sandy accumulation on upward-facing surfaces
                float3 nrmNormalized = normalize(i.normalWS);
                float topFacing  = saturate(nrmNormalized.y);
                float dustSpread = saturate((FBM2D(noiseUV * 10.0) - 0.2) * 2.0);
                float topDust    = topFacing * dustSpread * _TopDustAmount;
                albedo.rgb = lerp(albedo.rgb, _DirtColor.rgb * 1.15, topDust);

                // 4. GENERAL DIRT — Dusty clusters
                float dirtMask = saturate((FBM2D(noiseUV * _DirtScale * 0.5) - 0.35) * _DirtIntensity);
                albedo.rgb = lerp(albedo.rgb, albedo.rgb * _DirtColor.rgb * 1.3, dirtMask);

                // 5. SOOT — Dark burn patches (unique to this game's war theme)
                float sootNoise = FBM2D(noiseUV * 3.0 + 100.0); // offset to decorrelate
                float sootMask  = saturate((sootNoise - 0.5) * _SootAmount * 3.0);
                albedo.rgb = lerp(albedo.rgb, _SootColor.rgb, sootMask);

                // 6. RIM WEAR — Ashy/bleached edges
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.positionWS);
                float rim = 1.0 - saturate(dot(nrmNormalized, viewDir));
                float edgeWear = pow(rim, _RimPower) * _WearAmount;
                albedo.rgb = lerp(albedo.rgb, half3(0.45, 0.43, 0.40), edgeWear);

                // Alpha clip
                #ifdef _ALPHATEST_ON
                clip(albedo.a - _Cutoff);
                #endif

                // ════════════════════════════════════════════════════════
                //  LIGHTING
                // ════════════════════════════════════════════════════════
                float3 normalWS = nrmNormalized;
                #ifdef _NORMALMAP
                float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uv), _BumpScale);
                float3x3 TBN = float3x3(normalize(i.tangentWS), normalize(i.bitangentWS), normalWS);
                normalWS = normalize(mul(normalTS, TBN));
                #endif

                // Main light
                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                Light mainLight = GetMainLight(i.shadowCoord);
                #else
                Light mainLight = GetMainLight();
                #endif

                float NdotL = saturate(dot(normalWS, mainLight.direction));

                // Wrap-diffuse for softer shadow transitions on rubble
                float wrapNdotL = saturate((NdotL + 0.15) / 1.15);
                half3 ambient = SampleSH(normalWS);
                half3 diffuse = mainLight.color * wrapNdotL * mainLight.shadowAttenuation;

                // Specular (rough surfaces — low power)
                float3 halfDir  = normalize(mainLight.direction + viewDir);
                float NdotH     = saturate(dot(normalWS, halfDir));
                float specPower = exp2(10.0 * _Smoothness + 1.0);
                half3 specular  = _SpecColor.rgb * pow(NdotH, specPower)
                                * mainLight.color * mainLight.shadowAttenuation * _Smoothness;

                // Additional lights
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

                half3 finalRGB = albedo.rgb * (diffuse + addDiffuse + ambient) + specular;

                // Emission
                #ifdef _EMISSION
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, i.uv).rgb
                               * _EmissionColor.rgb * _EmissionIntensity;
                finalRGB += emission;
                #endif

                // Distance fade
                #ifdef _DISTANCE_FADE
                float camDist = distance(i.positionWS, _WorldSpaceCameraPos);
                float fadeFactor = 1.0 - saturate((camDist - _FadeStart) / max(_FadeEnd - _FadeStart, 0.001));
                albedo.a *= fadeFactor;
                #endif

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
        //  Pass 4 — Depth Normals (URP 2021+)
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
