// ═══════════════════════════════════════════════════════════════════
//  CurvedWorldCore.hlsl — Shared curved–world vertex deformation
//  Included by ALL environment shaders for perfectly unified bending.
//
//  Parameters are read from GLOBAL shader keywords set by C# via
//  Shader.SetGlobalFloat(), so every object bends identically without
//  per-material or per-PropertyBlock duplication.
//
//  Fallback: If a material has a local _Curvature property it will
//  shadow the global one (standard Unity behaviour).
// ═══════════════════════════════════════════════════════════════════
#ifndef CURVED_WORLD_CORE_INCLUDED
#define CURVED_WORLD_CORE_INCLUDED

// Global curvature params — set from C# CurvatureManager
float _Global_Curvature;
float _Global_CurvatureH;
float _Global_HorizonOffset;

// Per-material fallbacks (inside CBUFFER of each shader).
// If the material has its own values, Unity will use those; 
// otherwise the globals above are used.

// ─── Position Deformation ────────────────────────────────────────
inline float3 CurveWorldPosition(float3 positionWS, float curvature, float curvatureH, float horizonOffset)
{
    float distZ = max(0.0, positionWS.z - _WorldSpaceCameraPos.z - horizonOffset);
    float distSq = distZ * distZ;
    positionWS.y -= distSq * curvature;
    positionWS.x += distSq * curvatureH;
    return positionWS;
}

// Convenience overload that uses the global values
inline float3 CurveWorldPosition(float3 positionWS)
{
    return CurveWorldPosition(positionWS, _Global_Curvature, _Global_CurvatureH, _Global_HorizonOffset);
}

// ─── Normal Correction ───────────────────────────────────────────
inline float3 CurveWorldNormal(float3 normalWS, float3 positionWS, float curvature, float curvatureH, float horizonOffset)
{
    float distZ = max(0.0, positionWS.z - _WorldSpaceCameraPos.z - horizonOffset);
    float dydz = -2.0 * distZ * curvature;
    float dxdz =  2.0 * distZ * curvatureH;
    float3 corrected = normalWS;
    corrected.z -= dydz * normalWS.y;
    corrected.z -= dxdz * normalWS.x;
    return normalize(corrected);
}

inline float3 CurveWorldNormal(float3 normalWS, float3 positionWS)
{
    return CurveWorldNormal(normalWS, positionWS, _Global_Curvature, _Global_CurvatureH, _Global_HorizonOffset);
}

#endif // CURVED_WORLD_CORE_INCLUDED
