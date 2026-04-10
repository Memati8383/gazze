// ═══════════════════════════════════════════════════════════════════════
//  PremiumRoadShader — Procedural asphalt with lane markings
//
//  Features:
//    • World-space texture mapping (no Z-stretching)
//    • Anti-aliased procedural lane lines (center dashed + solid edges)
//    • Curved-world vertex deformation
//    • ShadowCaster pass for proper shadow casting
//    • Fog support
// ═══════════════════════════════════════════════════════════════════════
Shader "Custom/PremiumRoadShader"
{
    Properties
    {
        _MainTex   ("Asphalt Texture", 2D)      = "white" {}
        _BaseColor ("Asphalt Color",   Color)    = (0.12, 0.12, 0.12, 1)
        _LineColor ("Line Color",      Color)    = (0.9, 0.9, 0.8, 1)
        _TilingY   ("Texture Tiling",  Float)    = 40.0
        _DashLength ("Dash Length",    Float)    = 4.0
        _LineWidth ("Line Width",      Range(0, 0.1)) = 0.02

        [Header(Curved World)]
        _Curvature     ("Curvature V",    Float) = 0.002
        _CurvatureH    ("Curvature H",    Float) = -0.0015
        _HorizonOffset ("Horizon Offset", Float) = 10.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry-1"
        }
        LOD 100

        HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor;
                float4 _LineColor;
                float  _TilingY;
                float  _DashLength;
                float  _LineWidth;
                float  _Curvature;
                float  _CurvatureH;
                float  _HorizonOffset;
            CBUFFER_END

            inline float3 CurveWorldPos(float3 posWS)
            {
                float distZ = max(0.0, posWS.z - _WorldSpaceCameraPos.z - _HorizonOffset);
                float d2 = distZ * distZ;
                posWS.y -= d2 * _Curvature;
                posWS.x += d2 * _CurvatureH;
                return posWS;
            }
        ENDHLSL

        // ════════════════════════════════════════════════════════════
        //  Pass 1 — Forward Lit
        // ════════════════════════════════════════════════════════════
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float  fogFactor  : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Anti-aliased line drawing
            float DrawLine(float u, float center, float width)
            {
                float d = abs(u - center);
                return 1.0 - smoothstep(width * 0.45, width * 0.55, d);
            }

            Varyings Vert(Attributes i)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_TRANSFER_INSTANCE_ID(i, o);

                float3 posWS = TransformObjectToWorld(i.positionOS.xyz);
                posWS = CurveWorldPos(posWS);

                o.positionWS = posWS;
                o.positionCS = TransformWorldToHClip(posWS);
                o.uv = i.uv;
                o.fogFactor = ComputeFogFactor(o.positionCS.z);

                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                // World-space UV for asphalt tiling
                float2 uvAsphalt = float2(i.uv.x, i.positionWS.z / _TilingY);
                half4 asphalt = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvAsphalt) * _BaseColor;

                float u = i.uv.x;
                float lines = 0;

                // Center dashed line
                float dash = step(0.5, frac(i.positionWS.z / _DashLength));
                lines += DrawLine(u, 0.5, _LineWidth * 1.5) * dash;

                // Solid edge lines
                lines += DrawLine(u, 0.08, _LineWidth);
                lines += DrawLine(u, 0.92, _LineWidth);

                half4 col = lerp(asphalt, _LineColor, saturate(lines));

                col.rgb = MixFog(col.rgb, i.fogFactor);
                return col;
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
            ZWrite On  ZTest LEqual  ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_instancing

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            ShadowVaryings ShadowVert(ShadowAttributes v)
            {
                ShadowVaryings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
                posWS = CurveWorldPos(posWS);
                o.positionCS = TransformWorldToHClip(posWS);
                return o;
            }

            half4 ShadowFrag(ShadowVaryings i) : SV_Target { return 0; }
            ENDHLSL
        }

        // ════════════════════════════════════════════════════════════
        //  Pass 3 — Depth Only
        // ════════════════════════════════════════════════════════════
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On  ColorMask R

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
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
                return o;
            }

            half4 DepthFrag(DepthVaryings i) : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
