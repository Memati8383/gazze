// ═══════════════════════════════════════════════════════════════════
//  ProceduralNoise.hlsl — Shared noise functions for environment FX
//  Used by GazaRuinedShader, BarrierShader, VehicleShader, etc.
// ═══════════════════════════════════════════════════════════════════
#ifndef PROCEDURAL_NOISE_INCLUDED
#define PROCEDURAL_NOISE_INCLUDED

// ─── 2D Hash ─────────────────────────────────────────────────────
inline float Hash2D(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
}

// ─── 3D Hash ─────────────────────────────────────────────────────
inline float Hash3D(float n)
{
    return frac(sin(n) * 43758.5453123);
}

// ─── 2D Value Noise ──────────────────────────────────────────────
inline float Noise2D(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f); // Smoothstep
    float a = Hash2D(i);
    float b = Hash2D(i + float2(1.0, 0.0));
    float c = Hash2D(i + float2(0.0, 1.0));
    float d = Hash2D(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

// ─── 3D Value Noise ──────────────────────────────────────────────
inline float Noise3D(float3 x)
{
    float3 p = floor(x);
    float3 f = frac(x);
    f = f * f * (3.0 - 2.0 * f);
    float n = p.x + p.y * 57.0 + 113.0 * p.z;
    return lerp(
        lerp(lerp(Hash3D(n), Hash3D(n + 1.0), f.x),
             lerp(Hash3D(n + 57.0), Hash3D(n + 58.0), f.x), f.y),
        lerp(lerp(Hash3D(n + 113.0), Hash3D(n + 114.0), f.x),
             lerp(Hash3D(n + 170.0), Hash3D(n + 171.0), f.x), f.y),
        f.z);
}

// ─── 2D FBM (4 octaves) ─────────────────────────────────────────
inline float FBM2D(float2 p)
{
    float v = 0.0;
    float a = 0.5;
    [unroll] for (int i = 0; i < 4; i++)
    {
        v += a * Noise2D(p);
        p *= 2.02;
        a *= 0.5;
    }
    return v;
}

// ─── 3D FBM (3 octaves — cheaper for vehicles) ──────────────────
inline float FBM3D(float3 p)
{
    float f = 0.5000 * Noise3D(p); p *= 2.02;
    f     += 0.2500 * Noise3D(p); p *= 2.03;
    f     += 0.1250 * Noise3D(p);
    return f;
}

// ─── Voronoi (crack simulation) ──────────────────────────────────
inline float Voronoi2D(float2 p)
{
    float2 n = floor(p);
    float2 f = frac(p);
    float md = 8.0;
    [unroll] for (int j = -1; j <= 1; j++)
    {
        [unroll] for (int i = -1; i <= 1; i++)
        {
            float2 g = float2(float(i), float(j));
            float2 o = Hash2D(n + g);
            float2 r = g + o - f;
            float d = dot(r, r);
            if (d < md) md = d;
        }
    }
    return sqrt(md);
}

// ─── Smooth Threshold ────────────────────────────────────────────
inline float SmoothThreshold(float val, float t, float w)
{
    return smoothstep(t - w, t + w, val);
}

#endif // PROCEDURAL_NOISE_INCLUDED
