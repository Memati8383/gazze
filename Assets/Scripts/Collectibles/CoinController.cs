using UnityEngine;
using System.Collections;

namespace Gazze.Collectibles
{
    /// <summary>
    /// Coin objesinin görsel, hover (sallanma) ve toplanma (collect) efektlerini
    /// tamamen dinamik ve güvenli bir şekilde yönetir. Statik Texture sızıntıları giderildi.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CoinController : MonoBehaviour
    {
        [Header("Görsel Ayarlar")]
        public Sprite icon;
        public Color glowColor = new Color(1f, 0.82f, 0.1f, 1f); // Altın sarısı
        public float glowIntensity = 10.0f;
        public float pulseSpeed = 4f;
        public float pulseAmount = 0.2f;
        public float iconScale = 0.35f;
        public float glowScale = 0.65f;

        [Header("Hover & Hareket")]
        public float hoverAmplitude = 0.15f;
        public float hoverFrequency = 1.4f;

        [Header("Curved World")]
        public float curvature = 0.002f;
        public float curvatureH = -0.0015f;
        public float horizonOffset = 10f;

        [Header("Toplama")]
        public float collectScaleMultiplier = 1.8f;
        public float collectDuration = 0.22f;

        private Vector3 basePosition;
        private Vector3 baseScale;
        private float timeOffset;
        public bool IsCollected { get; private set; }

        private Collider coinCollider;
        private SpriteRenderer iconRenderer;
        private SpriteRenderer outerGlowRenderer;
        private SpriteRenderer innerGlowRenderer;
        private Light pointLight;
        private ParticleSystem particles;
        private bool visualsInitialized;

        private void Awake()
        {
            coinCollider = GetComponent<Collider>();
            if (coinCollider != null)
                coinCollider.isTrigger = true;
                
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            rb.useGravity = false;
            
            transform.localScale = Vector3.one;
            baseScale = Vector3.one;
            SetupVisuals();
        }

        private void SetupVisuals()
        {
            // Eski mesh eklentileri (varsa) temizle
            var mf = GetComponent<MeshFilter>();
            var mr = GetComponent<MeshRenderer>();
            if (mf != null) Destroy(mf);
            if (mr != null) Destroy(mr);

            foreach (Transform child in transform)
            {
                if (child.name == "CoinVisualTemp") child.gameObject.SetActive(false);
            }

            Sprite orbSprite = CreateOrbSprite(); 

            // Dış Parlama (Outer Glow)
            GameObject outerGo = new GameObject("OuterGlow");
            outerGo.transform.SetParent(transform, false);
            outerGo.transform.localScale = Vector3.one * glowScale;
            outerGlowRenderer = outerGo.AddComponent<SpriteRenderer>();
            outerGlowRenderer.sprite = orbSprite;
            outerGlowRenderer.sortingOrder = -1;

            // İç Parlama (Inner Glow)
            GameObject innerGo = new GameObject("InnerGlow");
            innerGo.transform.SetParent(transform, false);
            innerGo.transform.localScale = Vector3.one * (glowScale * 0.45f);
            innerGlowRenderer = innerGo.AddComponent<SpriteRenderer>();
            innerGlowRenderer.sprite = orbSprite;
            innerGlowRenderer.sortingOrder = 0;

            // Merkez İkon (Icon)
            GameObject iconGo = new GameObject("IconSprite");
            iconGo.transform.SetParent(transform, false);
            iconGo.transform.localScale = Vector3.one * iconScale;
            iconRenderer = iconGo.AddComponent<SpriteRenderer>();
            iconRenderer.sprite = icon != null ? icon : orbSprite;
            iconRenderer.color = Color.white;
            iconRenderer.sortingOrder = 1;

            // URP Lit shaders might cause NaN (black boxes) with coplanar Point Lights. Force unlit!
            Shader additiveShader = Shader.Find("Legacy Shaders/Particles/Additive") 
                                 ?? Shader.Find("Universal Render Pipeline/Particles/Unlit")
                                 ?? Shader.Find("Mobile/Particles/Additive")
                                 ?? Shader.Find("Sprites/Default");
            
            Material glowMat = new Material(additiveShader);
            outerGlowRenderer.material = glowMat;
            innerGlowRenderer.material = glowMat;
            
            Shader spriteUnlit = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
            if (iconRenderer) iconRenderer.material = new Material(spriteUnlit);

            // Fix sprite stretching issues by making sure localScale is perfectly uniform on awake
            outerGo.transform.localScale = new Vector3(glowScale, glowScale, glowScale);
            innerGo.transform.localScale = new Vector3(glowScale * 0.45f, glowScale * 0.45f, glowScale * 0.45f);
            iconGo.transform.localScale = new Vector3(iconScale, iconScale, iconScale);

            // -- Işık (Light) --
            GameObject lightGo = new GameObject("PointLight");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = new Vector3(0, 0, -0.5f); // Offset to prevent coplanar lighting bugs
            pointLight = lightGo.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = glowColor;
            pointLight.intensity = glowIntensity;
            pointLight.range = 4f;
            pointLight.renderMode = LightRenderMode.ForcePixel;

            // -- Parçacıklar (Particles) --
            GameObject particleGo = new GameObject("Sparkles");
            particleGo.transform.SetParent(transform, false);
            particles = particleGo.AddComponent<ParticleSystem>();
            SetupParticles(particles);

            visualsInitialized = true;
        }

        private void OnEnable()
        {
            IsCollected = false;
            transform.localScale = baseScale;
            if (coinCollider != null) coinCollider.enabled = true;
            timeOffset = Random.Range(0f, Mathf.PI * 2f);

            // Spawner bu objeyi pool'dan alıp pozisyonunu atattıktan sonra OnEnable tetiklenir
            basePosition = transform.position;

            if (iconRenderer) iconRenderer.color = Color.white;
            if (outerGlowRenderer) outerGlowRenderer.gameObject.SetActive(true);
            if (innerGlowRenderer) innerGlowRenderer.gameObject.SetActive(true);
            
            if (particles) particles.Play();
        }

        private void Update()
        {
            if (IsCollected || !visualsInitialized) return;

            // Curvature & Y-Bob (Hover) hesaplaması
            float hover = Mathf.Sin((Time.time + timeOffset) * hoverFrequency * Mathf.PI * 2f) * hoverAmplitude;
            float curveY = 0f, curveX = 0f;

            if (Camera.main != null)
            {
                float distZ = Mathf.Max(0f, transform.position.z - Camera.main.transform.position.z - horizonOffset);
                curveY = -(distZ * distZ * curvature);
                curveX = distZ * distZ * curvatureH;
            }

            transform.position = new Vector3(basePosition.x + curveX, basePosition.y + hover + curveY, transform.position.z);

            // Görsel Efektler (Billboard & Pulse)
            if (Camera.main != null)
            {
                // Always face camera directly
                Quaternion look = Quaternion.LookRotation(Camera.main.transform.forward);
                
                if (outerGlowRenderer) outerGlowRenderer.transform.rotation = look;
                if (innerGlowRenderer) innerGlowRenderer.transform.rotation = look;
                if (iconRenderer)
                {
                    float sway = Mathf.Sin(Time.time * pulseSpeed * 0.6f) * 15f;
                    iconRenderer.transform.rotation = look * Quaternion.Euler(0, 0, sway);
                }
            }

            if (iconRenderer)
            {
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
                iconRenderer.transform.localScale = Vector3.one * (iconScale * pulse);
            }
            if (outerGlowRenderer && outerGlowRenderer.gameObject.activeInHierarchy)
            {
                float alpha = 0.5f + Mathf.Sin(Time.time * pulseSpeed * 0.8f) * 0.3f;
                outerGlowRenderer.color = new Color(glowColor.r, glowColor.g, glowColor.b, alpha);
            }
            if (innerGlowRenderer && innerGlowRenderer.gameObject.activeInHierarchy)
            {
                float innerAlpha = 0.7f + Mathf.Sin(Time.time * pulseSpeed * 1.5f) * 0.3f;
                Color baseC = Color.Lerp(glowColor, Color.white, 0.5f);
                innerGlowRenderer.color = new Color(baseC.r, baseC.g, baseC.b, innerAlpha);
            }
            if (pointLight)
            {
                pointLight.intensity = glowIntensity + Mathf.Sin(Time.time * 6f) * 1.5f;
            }

            // Mıknatıs (Magnet) Efekti
            if (Gazze.PowerUps.PowerUpManager.Instance != null && 
                Gazze.PowerUps.PowerUpManager.Instance.IsPowerUpActive(Gazze.PowerUps.PowerUpType.Magnet))
            {
                if (PlayerController.Instance != null)
                {
                    float dist = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
                    if (dist < 15f)
                    {
                        Vector3 currentRealPos = new Vector3(basePosition.x, basePosition.y, transform.position.z);
                        Vector3 newRealPos = Vector3.MoveTowards(currentRealPos, PlayerController.Instance.transform.position, 40f * Time.deltaTime);
                        
                        basePosition.x = newRealPos.x;
                        basePosition.y = newRealPos.y;
                        transform.position = new Vector3(transform.position.x, transform.position.y, newRealPos.z);
                    }
                }
            }

            // High-speed / fallback manual collision detection (matches PowerUp implementation)
            if (PlayerController.Instance != null && !IsCollected)
            {
                // Measure against projected flat horizontal distance (Ignore Y axis difference since coin is floating, player is grounded)
                float dx = Mathf.Abs(transform.position.x - PlayerController.Instance.transform.position.x);
                float dz = Mathf.Abs(transform.position.z - PlayerController.Instance.transform.position.z);
                
                // Expand the Z tolerance based on vehicle velocity over frame to catch frame-skips completely
                float zTol = 1.75f + (PlayerController.Instance.currentWorldSpeed * Time.deltaTime * 1.5f);

                if (dx < 1.75f && dz < zTol)
                {
                    PlayerController.Instance.ProcessCoinTrigger(gameObject);
                }
            }
        }

        public void Collect()
        {
            if (IsCollected) return;
            IsCollected = true;
            if (coinCollider) coinCollider.enabled = false;

            StartCoroutine(CollectAnimation());
        }

        private IEnumerator CollectAnimation()
        {
            float t = 0;
            Vector3 startScale = baseScale;
            Vector3 targetScale = baseScale * collectScaleMultiplier;

            if (pointLight) pointLight.intensity = glowIntensity * 2f;
            
            // Clean visual: disable glows during collect to prevent artifacts
            if (outerGlowRenderer) outerGlowRenderer.gameObject.SetActive(false);
            if (innerGlowRenderer) innerGlowRenderer.gameObject.SetActive(false);
            
            Color startIconColor = iconRenderer ? iconRenderer.color : Color.white;

            while (t < collectDuration)
            {
                t += Time.deltaTime;
                float pct = t / collectDuration;
                transform.localScale = Vector3.Lerp(startScale, targetScale, pct);
                
                if (iconRenderer && iconRenderer.gameObject.activeInHierarchy) 
                { 
                    Color c = startIconColor; 
                    c.a = Mathf.Lerp(startIconColor.a, 0f, pct); 
                    iconRenderer.color = c; 
                }

                yield return null;
            }

            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            transform.localScale = baseScale;
        }

        private void SetupParticles(ParticleSystem ps)
        {
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 1.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startColor = new Color(glowColor.r, glowColor.g, glowColor.b, 1f);
            main.maxParticles = 30;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = true;

            var emission = ps.emission;
            emission.rateOverTime = 15f;
            
            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            velocity.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.orbitalY = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.orbitalZ = new ParticleSystem.MinMaxCurve(-1f, 1f);

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
            Shader particleShader = Shader.Find("Particles/Standard Unlit") 
                                 ?? Shader.Find("Universal Render Pipeline/Particles/Unlit")
                                 ?? Shader.Find("Sprites/Default");
            
            if (particleShader != null) renderer.material = new Material(particleShader);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        // Statik variable KULLANMADAN her obje kendi sprite'ını oluştursun (veya ortak havuz yöneticisi ayarlasın)
        // Memory leak yaşamamak için 128x128 soft circle sprite.
        private Sprite CreateOrbSprite()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = size / 2f, radius = size / 2f;

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
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 25.6f);
        }
    }
}

