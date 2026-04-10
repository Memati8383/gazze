using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Gazze.UI;

namespace Gazze.PowerUps
{
    public class PowerUpEffects : MonoBehaviour
    {
        public static PowerUpEffects Instance { get; private set; }

        [Header("Zaman Bükücü (TimeWarp) Ayarları")]
        [Tooltip("Dünya yavaşlama oranı (0.5 = %50 yavaş, 1.0 = Normal hız).")]
        public float timeWarpScale = 0.5f;
        [Tooltip("Aktif olduğunda ekranda görünen mavi-mor saydam Image bileşeni.")]
        public Image timeWarpOverlay;

        [Header("Şok Dalgası (ShockWave) Ayarları")]
        [Tooltip("Şok dalgasının (patlamanın) etki dairesi yarıçapı.")]
        public float shockWaveRadius = 25f;
        [Tooltip("Trafik araçlarına uygulanan yatay fırlatma şiddeti.")]
        public float shockWaveForce = 40f;
        [Tooltip("Trafik araçlarına uygulanan dikey (yukarı doğru) fırlatma şiddeti.")]
        public float shockWaveUpForce = 8f;

        [Header("Dev Modu (Juggernaut) Ayarları")]
        [Tooltip("Arabanın büyüme oranı (1.2 = %20 büyüme).")]
        public float juggernautScale = 1.2f;
        [Tooltip("Çarpılan araçların ne kadar sert fırlatılacağı.")]
        public float juggernautFlingForce = 30f;
        [Tooltip("Dev modunda yok edilen her araç için kazanılacak ekstra puan.")]
        public int juggernautBonusScore = 500;

        [Header("Görsel Efektler")]
        [Tooltip("Şok dalgası halka efekti (Particle) prefabı.")]
        public GameObject shockWaveRingPrefab;
        [Tooltip("Dev modu aktifken aracın etrafında görünen kalkan efekti prefabı.")]
        public GameObject juggernautShieldPrefab;

        // --- AKTİF DURUM BİLGİLERİ (Runtime State) ---
        /// <summary> Zaman bükme modu şu an devrede mi? </summary>
        private bool isTimeWarpActive = false;
        /// <summary> Dev modu (invincible) şu an devrede mi? </summary>
        private bool isJuggernautActive = false;
        /// <summary> Sahnedeki aktif kalkan objesi referansı. </summary>
        private GameObject activeJuggernautShield;
        /// <summary> Arabanın orijinal boyutlarını geri yüklemek için tutulan değer. </summary>
        private Vector3 originalPlayerScale;
        /// <summary> Tek seferlik dev modu kullanımında kaç aracın ezildiği. </summary>
        private int juggernautFlingCount = 0;
        /// <summary> Aynı araca birden fazla kez vurup puan almayı engelleyen ID listesi. </summary>
        private HashSet<int> juggernautFlingedCars = new HashSet<int>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void OnEnable()
        {
            StartCoroutine(WaitAndSubscribe());
        }

        private IEnumerator WaitAndSubscribe()
        {
            // Wait until PowerUpManager is ready
            while (PowerUpManager.Instance == null) yield return null;

            PowerUpManager.Instance.OnPowerUpActivated += OnPowerUpActivated;
            PowerUpManager.Instance.OnPowerUpDeactivated += OnPowerUpDeactivated;
        }

        private void OnDisable()
        {
            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.OnPowerUpActivated -= OnPowerUpActivated;
                PowerUpManager.Instance.OnPowerUpDeactivated -= OnPowerUpDeactivated;
            }
        }

        private void OnPowerUpActivated(PowerUpType type, float duration, float totalDuration)
        {
            switch (type)
            {
                case PowerUpType.TimeWarp:
                    ActivateTimeWarp();
                    break;
                case PowerUpType.ShockWave:
                    ActivateShockWave();
                    break;
                case PowerUpType.Juggernaut:
                    ActivateJuggernaut();
                    break;
            }
        }

        private void OnPowerUpDeactivated(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.TimeWarp:
                    DeactivateTimeWarp();
                    break;
                case PowerUpType.Juggernaut:
                    DeactivateJuggernaut();
                    break;
                // ShockWave is instant, no deactivation needed
            }
        }

        // ═══════════════════════════════════════════════════════
        // ██ TIME WARP (Zaman Bükücü)
        // ═══════════════════════════════════════════════════════

        private void ActivateTimeWarp()
        {
            if (isTimeWarpActive) return;
            isTimeWarpActive = true;

            // Dünyayı yavaşlat ama fixedDeltaTime'ı da oranla (fizik tutarlılığı için)
            Time.timeScale = timeWarpScale;
            Time.fixedDeltaTime = 0.02f * timeWarpScale;

            // Ses perdesini düşür (AudioMixer üzerinden veya fallback olarak)
            if (Settings.AudioManager.Instance != null)
            {
                Settings.AudioManager.Instance.SetGlobalPitch(timeWarpScale + 0.1f);
            }

            // Mavi-mor overlay'ı göster
            if (timeWarpOverlay != null)
            {
                timeWarpOverlay.gameObject.SetActive(true);
                timeWarpOverlay.color = new Color(0.1f, 0.05f, 0.35f, 0.2f); // Biraz daha koyu bir baz
            }
            
            // Haptic feedback
            Settings.HapticManager.Heavy();
        }

        private void DeactivateTimeWarp()
        {
            if (!isTimeWarpActive) return;
            isTimeWarpActive = false;

            // Zamanı normallere döndür
            if (!Gazze.UI.PauseMenuBuilder.IsPaused && PlayerController.Instance != null)
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f;
                
                if (Settings.AudioManager.Instance != null)
                {
                    Settings.AudioManager.Instance.ResetGlobalPitch();
                }
            }

            // Overlay'ı kapat
            if (timeWarpOverlay != null)
            {
                StartCoroutine(FadeOutOverlay(timeWarpOverlay, 0.4f));
            }

            Debug.Log("[PowerUpEffects] TimeWarp DEACTIVATED — World speed and audio restored");
        }

        // ═══════════════════════════════════════════════════════
        // ██ SHOCK WAVE (Şok Dalgası)
        // ═══════════════════════════════════════════════════════

        private void ActivateShockWave()
        {
            if (PlayerController.Instance == null) return;

            Vector3 playerPos = PlayerController.Instance.transform.position;

            // Yakındaki tüm trafik araçlarını bul ve fırlat
            Collider[] hits = Physics.OverlapSphere(playerPos, shockWaveRadius);
            int flingedCount = 0;
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("TrafficCar"))
                {
                    FlingTrafficCar(hit.gameObject, playerPos, shockWaveForce, shockWaveUpForce);
                    flingedCount++;
                }
            }

            // Halka dalga efekti
            if (shockWaveRingPrefab != null)
            {
                GameObject ring = Instantiate(shockWaveRingPrefab, playerPos, Quaternion.identity);
                Destroy(ring, 2f);
            }
            else
            {
                // Fallback: Programatik halka efekti
                StartCoroutine(CreateShockWaveRing(playerPos));
            }

            // Ekran sarsıntısı
            if (Gazze.CameraSystem.SmoothCameraFollow.Instance != null)
            {
                Gazze.CameraSystem.SmoothCameraFollow.Instance.TriggerCrashShake();
            }

            // Ekstra: Şok dalgası sırasında ekranı bir anlığına patlama beyazı yap
            if (timeWarpOverlay != null)
            {
                timeWarpOverlay.gameObject.SetActive(true);
                timeWarpOverlay.color = new Color(1, 1, 1, 0.4f);
                Invoke(nameof(ResetOverlayAfterShock), 0.1f);
            }

            // Haptic
            Settings.HapticManager.Heavy();
        }

        private void ResetOverlayAfterShock()
        {
            if (timeWarpOverlay != null && !isTimeWarpActive) 
                timeWarpOverlay.color = new Color(0, 0, 0, 0);
            else if (timeWarpOverlay != null && isTimeWarpActive)
                timeWarpOverlay.color = new Color(0.1f, 0.05f, 0.35f, 0.2f); // Restore TimeWarp color
            else if (timeWarpOverlay != null)
                timeWarpOverlay.gameObject.SetActive(false); // Turn off if not TimeWarp
        }

        // ═══════════════════════════════════════════════════════
        // ██ JUGGERNAUT (Dev Modu)
        // ═══════════════════════════════════════════════════════

        private void ActivateJuggernaut()
        {
            if (isJuggernautActive || PlayerController.Instance == null) return;
            isJuggernautActive = true;
            juggernautFlingCount = 0;
            juggernautFlingedCars.Clear();

            Transform playerTransform = PlayerController.Instance.transform;

            // Orijinal ölçeği kaydet ve büyüt
            originalPlayerScale = playerTransform.localScale;
            StartCoroutine(ScalePlayer(playerTransform, originalPlayerScale * juggernautScale, 0.5f));

            // Enerji kalkanı efekti
            if (juggernautShieldPrefab != null)
            {
                activeJuggernautShield = Instantiate(juggernautShieldPrefab, playerTransform);
                activeJuggernautShield.transform.localPosition = Vector3.zero;
            }
            else
            {
                // Fallback: Programatik turuncu enerji kalkanı
                StartCoroutine(CreateJuggernautShield(playerTransform));
            }

            // Koruma süresini başlat (Juggernaut süresince hasar almaz)
            PlayerController.Instance.StartInvulnerability(99f);

            Settings.HapticManager.Heavy();
            Debug.Log("[PowerUpEffects] Juggernaut ACTIVATED — Car scaled up, invincible mode ON");
        }

        private void DeactivateJuggernaut()
        {
            if (!isJuggernautActive || PlayerController.Instance == null) return;
            isJuggernautActive = false;

            Transform playerTransform = PlayerController.Instance.transform;

            // Ölçeği geri döndür
            StartCoroutine(ScalePlayer(playerTransform, originalPlayerScale, 0.5f));

            // Kalkanı kaldır
            if (activeJuggernautShield != null)
            {
                Destroy(activeJuggernautShield);
                activeJuggernautShield = null;
            }

            // Koruma süresini normal sınırına indir
            PlayerController.Instance.StartInvulnerability(PlayerController.Instance.invulnerabilityDuration);

            juggernautFlingedCars.Clear();
            Debug.Log($"[PowerUpEffects] Juggernaut DEACTIVATED — Flinged {juggernautFlingCount} cars total");
        }

        // ═══════════════════════════════════════════════════════
        // ██ UPDATE LOOP (Visual ticking effects)
        // ═══════════════════════════════════════════════════════

        private void Update()
        {
            // TimeWarp overlay pulsing
            if (isTimeWarpActive && timeWarpOverlay != null)
            {
                float pulse = 0.08f + Mathf.Sin(Time.unscaledTime * 2f) * 0.04f;
                timeWarpOverlay.color = new Color(0.3f, 0.15f, 0.8f, pulse);
            }

            // Juggernaut: Çarpışma kontrolü (her frame)
            if (isJuggernautActive && PlayerController.Instance != null)
            {
                CheckJuggernautCollisions();
            }
        }

        private void CheckJuggernautCollisions()
        {
            BoxCollider bc = PlayerController.Instance.GetComponent<BoxCollider>();
            if (bc == null) return;

            Vector3 center = PlayerController.Instance.transform.TransformPoint(bc.center);
            Vector3 halfExtents = Vector3.Scale(bc.size, PlayerController.Instance.transform.lossyScale) * 0.55f;

            // Sweep alanını biraz genişlet (fırlatma için)
            float speed = PlayerController.Instance.currentWorldSpeed;
            halfExtents.z += speed * Time.deltaTime * 0.5f;
            center += PlayerController.Instance.transform.forward * (speed * Time.deltaTime * 0.5f);

            Collider[] hits = Physics.OverlapBox(center, halfExtents, PlayerController.Instance.transform.rotation);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("TrafficCar"))
                {
                    int carId = hit.gameObject.GetInstanceID();
                    if (!juggernautFlingedCars.Contains(carId))
                    {
                        juggernautFlingedCars.Add(carId);
                        juggernautFlingCount++;

                        // Aracı fırlat
                        FlingTrafficCar(hit.gameObject, PlayerController.Instance.transform.position, juggernautFlingForce, 6f);

                        // Skor ekle
                        PlayerController.Instance.AddScore(juggernautBonusScore);
                        
                        // Floating Text (Görsel Skor Geri Bildirimi)
                        string crushedMsg = Gazze.UI.LocalizationManager.Get("PowerUp_Crushed", "EZİLDİ!");
                        PlayerController.Instance.ShowFloatingText($"{crushedMsg}\n<color=orange>+{juggernautBonusScore}</color>", Color.yellow, 10f, 0f);

                        // Sarsıntı ve Haptik
                        var cf = Gazze.CameraSystem.SmoothCameraFollow.Instance;
                        if (cf != null) cf.TriggerShake(0.35f, 0.35f); // Juggernaut darbe şiddeti
                        Settings.HapticManager.Heavy();
                    }
                }
            }
        }

        // ═══════════════════════════════════════════════════════
        // ██ UTILITY METHODS
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Bir trafik aracını belirli bir noktadan itilmiş gibi fırlatır.
        /// </summary>
        private void FlingTrafficCar(GameObject car, Vector3 sourcePos, float force, float upForce)
        {
            if (car == null) return;

            // Rigidbody'yi kinematic'ten çıkar (geçici olarak)
            Rigidbody rb = car.GetComponent<Rigidbody>();
            if (rb == null) rb = car.AddComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.useGravity = true;

            // Fırlatma yönü: Oyuncudan uzağa + yukarı
            Vector3 direction = (car.transform.position - sourcePos).normalized;
            if (direction.magnitude < 0.01f) direction = Vector3.right; // Tam üstünde ise sağa at
            direction.y = 0; // Yatay düzleme çıkar
            direction = direction.normalized;

            // Yana doğru kuvvet + yukarı doğru kuvvet
            rb.AddForce(direction * force + Vector3.up * upForce, ForceMode.Impulse);
            // Döndürme torku (dramatik görünüm)
            rb.AddTorque(Random.insideUnitSphere * force * 0.5f, ForceMode.Impulse);

            // Collider'ı trigger olmaktan çıkar ki fizik etkileşsin
            BoxCollider bc = car.GetComponent<BoxCollider>();
            if (bc != null) bc.isTrigger = false;

            // 3 saniye sonra aracı deaktif et
            StartCoroutine(DespawnAfterFling(car, rb, bc, 3f));
        }

        private IEnumerator DespawnAfterFling(GameObject car, Rigidbody rb, BoxCollider bc, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (car != null)
            {
                // Tekrar kinematic yap ve deaktif et
                if (rb != null)
                {
                    if (!rb.isKinematic)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
                if (bc != null) bc.isTrigger = true;
                car.SetActive(false);
            }
        }

        private IEnumerator ScalePlayer(Transform player, Vector3 targetScale, float duration)
        {
            Vector3 startScale = player.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (player == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                // Overshoot curve for juicy feel
                t = 1f - Mathf.Pow(1f - t, 3f);
                player.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            if (player != null) player.localScale = targetScale;
        }

        private IEnumerator FadeOutOverlay(Image overlay, float duration)
        {
            float elapsed = 0f;
            Color startColor = overlay.color;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / duration);
                overlay.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
            overlay.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
            overlay.gameObject.SetActive(false);
        }

        /// <summary>
        /// Programatik şok dalgası halka efekti oluşturur.
        /// </summary>
        private IEnumerator CreateShockWaveRing(Vector3 position)
        {
            // Basit bir halka efekti (Quad + transparan materyal)
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "ShockWaveRing";
            ring.transform.position = position + Vector3.up * 0.5f;
            ring.transform.localScale = new Vector3(0.5f, 0.01f, 0.5f);

            // Collider'ı kaldır
            Collider col = ring.GetComponent<Collider>();
            if (col != null) Destroy(col);

            // Transparan materyal
            Renderer rend = ring.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                if (mat.shader == null) mat = new Material(Shader.Find("Standard"));
                mat.SetFloat("_Surface", 1); // Transparent
                mat.SetFloat("_Blend", 0);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
                mat.color = new Color(0.4f, 0.7f, 1f, 0.6f);
                rend.material = mat;
            }

            // Genişleyerek solma animasyonu
            float elapsed = 0f;
            float duration = 0.8f;
            while (elapsed < duration)
            {
                if (ring == null) yield break;
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                float scale = Mathf.Lerp(0.5f, shockWaveRadius * 2f, t);
                ring.transform.localScale = new Vector3(scale, 0.01f, scale);

                float alpha = Mathf.Lerp(0.6f, 0f, t);
                if (rend != null) rend.material.color = new Color(0.4f, 0.7f, 1f, alpha);

                yield return null;
            }

            if (ring != null) Destroy(ring);
        }

        /// <summary>
        /// Programatik Juggernaut enerji kalkanı efekti.
        /// </summary>
        private IEnumerator CreateJuggernautShield(Transform parent)
        {
            GameObject shield = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shield.name = "JuggernautShield";
            shield.transform.SetParent(parent, false);
            shield.transform.localPosition = Vector3.up * 0.5f;
            shield.transform.localScale = Vector3.one * 3f;
            activeJuggernautShield = shield;

            // Collider'ı kaldır
            Collider col = shield.GetComponent<Collider>();
            if (col != null) Destroy(col);

            // Turuncu transparan materyal
            Renderer rend = shield.GetComponent<Renderer>();
            Material mat = null;
            if (rend != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                mat = new Material(shader);
                mat.SetFloat("_Surface", 1);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
                mat.color = new Color(1f, 0.5f, 0f, 0.2f); // Turuncu
                rend.material = mat;
            }

            // Pulsing efekti
            while (shield != null && isJuggernautActive)
            {
                float pulse = 0.15f + Mathf.Sin(Time.time * 3f) * 0.08f;
                if (rend != null && mat != null)
                {
                    mat.color = new Color(1f, 0.5f, 0f, pulse);
                }
                float scalePulse = 3f + Mathf.Sin(Time.time * 2f) * 0.2f;
                shield.transform.localScale = Vector3.one * scalePulse;
                yield return null;
            }
        }

        private void ShowJuggernautHitText(Vector3 position)
        {
            // Floating text
            GameObject textGO = new GameObject("JuggernautHitText");
            textGO.transform.position = position + Vector3.up * 2.5f;

            var tmp = textGO.AddComponent<TMPro.TextMeshPro>();
            string msg = Gazze.UI.LocalizationManager.Get("PowerUp_Crushed", "EZİLDİ!");
            tmp.text = $"{msg}\n+{juggernautBonusScore}";
            tmp.fontSize = 8f;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.fontStyle = TMPro.FontStyles.Bold;

            // Turuncu gradient
            tmp.color = Color.white;
            tmp.enableVertexGradient = true;
            Color topColor = new Color(1f, 0.6f, 0.1f);
            Color botColor = new Color(0.8f, 0.2f, 0f);
            tmp.colorGradient = new TMPro.VertexGradient(topColor, topColor, botColor, botColor);
            tmp.outlineWidth = 0.25f;
            tmp.outlineColor = new Color32(20, 10, 0, 255);

            if (Camera.main != null)
            {
                textGO.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
            }

            var ft = textGO.AddComponent<Gazze.UI.FloatingText>();
            ft.duration = 1.2f;
            ft.upwardSpeed = 4f;

            AnimationCurve popCurve = new AnimationCurve();
            popCurve.AddKey(new Keyframe(0f, 0f, 0f, 7f));
            popCurve.AddKey(new Keyframe(0.2f, 1.3f, 0f, 0f));
            popCurve.AddKey(new Keyframe(1f, 0.85f, -0.5f, 0f));
            ft.scaleCurve = popCurve;
        }

        // ═══════════════════════════════════════════════════════
        // ██ PUBLIC API
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Dışarıdan sorgulanabilir: Juggernaut aktif mi?
        /// </summary>
        public bool IsJuggernautActive => isJuggernautActive;

        /// <summary>
        /// Dışarıdan sorgulanabilir: TimeWarp aktif mi?
        /// </summary>
        public bool IsTimeWarpActive => isTimeWarpActive;
    }
}
