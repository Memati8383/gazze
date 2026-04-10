using UnityEngine;

namespace Gazze.PowerUps
{
    [RequireComponent(typeof(PowerUpItem))]
    public class PowerUpVisual : MonoBehaviour
    {
        [Header("Fallback (auto-fetched from PowerUpManager if available)")]
        public Sprite icon;
        public Color glowColor = Color.cyan;

        [Header("Settings")]
        public float glowIntensity = 12.0f;
        public float pulseSpeed = 5f;
        public float pulseAmount = 0.35f;
        public float iconScale = 1.1f;
        public float glowScale = 2.8f;

        private SpriteRenderer iconRenderer;
        private SpriteRenderer outerGlowRenderer;
        private SpriteRenderer innerGlowRenderer;
        private Light pointLight;
        private ParticleSystem particles;
        private Vector3 baseScale;
        private bool initialized;

        private void Start()
        {
            // Auto-fetch icon & color from PowerUpManager data
            var item = GetComponent<PowerUpItem>();
            if (item != null && PowerUpManager.Instance != null)
            {
                var data = PowerUpManager.Instance.GetData(item.powerUpType);
                if (data != null)
                {
                    if (data.icon != null) icon = data.icon;
                    glowColor = data.themeColor;
                }
            }

            SetupVisuals();
            initialized = true;
        }

        private void SetupVisuals()
        {
            // Remove old mesh visuals at runtime
            var mf = GetComponent<MeshFilter>();
            var mr = GetComponent<MeshRenderer>();
            if (mf != null) Destroy(mf);
            if (mr != null) Destroy(mr);

            // ── Outer Glow ──
            GameObject outerGlowGo = new GameObject("OuterGlow");
            outerGlowGo.transform.SetParent(transform, false);
            outerGlowGo.transform.localPosition = Vector3.zero;
            outerGlowGo.transform.localScale = Vector3.one * glowScale;
            outerGlowRenderer = outerGlowGo.AddComponent<SpriteRenderer>();
            outerGlowRenderer.sprite = CreateCircleSprite();
            outerGlowRenderer.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.8f);
            outerGlowRenderer.sortingOrder = -1;

            // Use Additive material for better glow
            Shader glowShader = Shader.Find("Legacy Shaders/Particles/Additive");
            if (glowShader == null) glowShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (glowShader == null) glowShader = Shader.Find("Mobile/Particles/Additive");
            
            if (glowShader != null)
            {
                outerGlowRenderer.material = new Material(glowShader);
            }

            // ── Inner Core Glow ──
            GameObject innerGlowGo = new GameObject("InnerGlow");
            innerGlowGo.transform.SetParent(transform, false);
            innerGlowGo.transform.localPosition = Vector3.zero;
            innerGlowGo.transform.localScale = Vector3.one * (glowScale * 0.45f);
            innerGlowRenderer = innerGlowGo.AddComponent<SpriteRenderer>();
            innerGlowRenderer.sprite = CreateCircleSprite();
            Color innerGlowC = Color.Lerp(glowColor, Color.white, 0.7f);
            innerGlowRenderer.color = new Color(innerGlowC.r, innerGlowC.g, innerGlowC.b, 1.0f);
            innerGlowRenderer.sortingOrder = 0;
            
            if (glowShader != null)
            {
                innerGlowRenderer.material = new Material(glowShader);
            }

            // ── Icon sprite ──
            GameObject iconGo = new GameObject("IconSprite");
            iconGo.transform.SetParent(transform, false);
            iconGo.transform.localPosition = Vector3.zero;
            iconGo.transform.localScale = Vector3.one * iconScale;
            iconRenderer = iconGo.AddComponent<SpriteRenderer>();
            iconRenderer.sprite = icon;
            iconRenderer.color = Color.white;
            iconRenderer.sortingOrder = 1;

            // ── Point light ──
            GameObject lightGo = new GameObject("PointLight");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = Vector3.zero;
            pointLight = lightGo.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = glowColor;
            pointLight.intensity = glowIntensity;
            pointLight.range = 5.0f;
            pointLight.renderMode = LightRenderMode.ForcePixel;

            // ── Particle sparkles ──
            GameObject particleGo = new GameObject("Sparkles");
            particleGo.transform.SetParent(transform, false);
            particleGo.transform.localPosition = Vector3.zero;
            particles = particleGo.AddComponent<ParticleSystem>();
            SetupParticles(particles);

            baseScale = iconGo.transform.localScale;
        }

        private void Update()
        {
            if (!initialized) return;

            // Billboard — always face the camera
            if (Camera.main != null)
            {
                Quaternion look = Quaternion.LookRotation(Camera.main.transform.forward);
                if (outerGlowRenderer != null) outerGlowRenderer.transform.rotation = look;
                if (innerGlowRenderer != null) innerGlowRenderer.transform.rotation = look;
                
                // Icon also sways slightly back and forth
                if (iconRenderer != null)
                {
                    float sway = Mathf.Sin(Time.time * pulseSpeed * 0.6f) * 15f;
                    iconRenderer.transform.rotation = look * Quaternion.Euler(0, 0, sway);
                }
            }

            // Pulse scale
            if (iconRenderer != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
                iconRenderer.transform.localScale = baseScale * pulse;
            }

            // Glow pulse alpha and inner pulse
            if (outerGlowRenderer != null)
            {
                float alpha = 0.5f + Mathf.Sin(Time.time * pulseSpeed * 0.8f) * 0.3f;
                outerGlowRenderer.color = new Color(glowColor.r, glowColor.g, glowColor.b, alpha);
                outerGlowRenderer.transform.localScale = Vector3.one * (glowScale + Mathf.Sin(Time.time * pulseSpeed * 0.5f) * 0.2f);
            }
            if (innerGlowRenderer != null)
            {
                Color innerBase = Color.Lerp(glowColor, Color.white, 0.7f);
                float innerAlpha = 0.8f + Mathf.Sin(Time.time * pulseSpeed * 1.5f) * 0.2f;
                innerGlowRenderer.color = new Color(innerBase.r, innerBase.g, innerBase.b, innerAlpha);
            }

            // Light flicker
            if (pointLight != null)
            {
                pointLight.intensity = glowIntensity + Mathf.Sin(Time.time * 6f) * 2.0f;
            }
        }

        private void SetupParticles(ParticleSystem ps)
        {
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 1.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startColor = new Color(glowColor.r, glowColor.g, glowColor.b, 1f);
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = true;

            var emission = ps.emission;
            emission.rateOverTime = 40f;
            
            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            // Linear velocity: all axes must be the same mode (TwoConstants)
            velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.5f, 1.5f); // Rise up
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            
            // Orbital velocity: all axes must be the same mode (TwoConstants)
            velocity.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.orbitalY = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.orbitalZ = new ParticleSystem.MinMaxCurve(-1f, 1f); // Swirl

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.4f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(glowColor, 0.5f), new GradientColorKey(glowColor, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0, 1, 1, 0));

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            Shader particleShader = Shader.Find("Particles/Standard Unlit");
            if (particleShader == null) particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (particleShader == null) particleShader = Shader.Find("Legacy Shaders/Particles/Additive");
            if (particleShader == null) particleShader = Shader.Find("Sprites/Default");
            if (particleShader == null) particleShader = Shader.Find("UI/Default");
            
            if (particleShader != null) 
            {
                renderer.material = new Material(particleShader);
            }
            else
            {
                Debug.LogError("PowerUpVisual: Could not find any suitable particle shader.");
            }
            
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private Sprite CreateCircleSprite()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = size / 2f;
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float t = Mathf.Clamp01(dist / radius);
                    float alpha = Mathf.Pow(1f - t, 2f);
                    tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
