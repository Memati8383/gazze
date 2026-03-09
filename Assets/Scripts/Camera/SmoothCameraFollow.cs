using UnityEngine;

namespace Gazze.CameraSystem
{
    /// <summary>
    /// Oyuncu aracını yumuşak bir şekilde takip eden kamera kontrolcüsü.
    /// Hıza duyarlı sarsıntı ve damping ayarları içerir.
    /// </summary>
    public class SmoothCameraFollow : MonoBehaviour
    {
        [Header("Takip Ayarları")]
        [Tooltip("Takip edilecek hedef (Player).")]
        public Transform target;
        [Tooltip("Kameranın hedefe olan uzaklık ofseti.")]
        public Vector3 offset = new Vector3(0, 3, -6);
        
        [Header("Damping (Yumuşama) Ayarları")]
        [Tooltip("Pozisyon takip hızı (Düşük değer = daha fazla gecikme).")]
        public float positionSmoothTime = 0.15f;
        [Tooltip("Rotasyon takip hızı.")]
        public float rotationSmoothTime = 0.1f;

        [Header("Hız Dinamiği")]
        [Tooltip("Maksimum hızda kameranın ne kadar uzaklaşacağı.")]
        public float maxSpeedOffsetZ = -2f;
        [Tooltip("Hız arttıkça damping'in ne kadar değişeceği.")]
        public float speedDampingMultiplier = 0.5f;

        [Header("FOV Ayarları")]
        [Tooltip("Varsayılan Field of View.")]
        public float defaultFOV = 60f;
        [Tooltip("Hız arttıkça FOV'un ne kadar artacağı (hıza orantılı).")]
        public float speedFOVAmount = 8f;
        [Tooltip("Boost sırasında FOV'a eklenen ekstra miktar.")]
        public float boostFOVAmount = 10f;
        [Tooltip("FOV değişim hızı.")]
        public float fovSmoothSpeed = 5f;

        [Header("Sarsıntı – Boost (hafif, kısa)")]
        [Tooltip("Boost başladığındaki sarsıntı şiddeti.")]
        public float boostShakeIntensity = 0.15f;
        [Tooltip("Boost sarsıntı süresi.")]
        public float boostShakeDuration = 0.2f;
        [Tooltip("Boost sarsıntı rotasyon çarpanı.")]
        public float boostShakeRotMul = 0.5f;

        [Header("Sarsıntı – Kaza (orta şiddet)")]
        [Tooltip("Kaza anında sarsıntı şiddeti.")]
        public float crashShakeIntensity = 0.3f;
        [Tooltip("Kaza sarsıntı süresi.")]
        public float crashShakeDuration = 0.3f;
        [Tooltip("Kaza sarsıntı rotasyon çarpanı.")]
        public float crashShakeRotMul = 1.0f;

        [Header("Sarsıntı – Ölüm (dramatik)")]
        [Tooltip("Ölüm anında sarsıntı şiddeti.")]
        public float deathShakeIntensity = 0.55f;
        [Tooltip("Ölüm sarsıntı süresi.")]
        public float deathShakeDuration = 0.5f;
        [Tooltip("Ölüm sarsıntı rotasyon çarpanı.")]
        public float deathShakeRotMul = 1.5f;

        private Vector3 currentVelocity = Vector3.zero;
        private Quaternion currentRotationVelocity;
        private float initialZOffset;
        private Camera mainCam;
        private float currentShakeTime = 0f;
        private float currentShakeDuration = 0f;
        private float currentShakeIntensity = 0f;
        private float currentShakeRotMul = 1f;
        private Vector3 shakeOffset = Vector3.zero;
        private Quaternion shakeRotation = Quaternion.identity;
        private Quaternion baseRotation;

        public static SmoothCameraFollow Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            
            mainCam = GetComponent<Camera>();
        }

        private void Start()
        {
            if (target == null && PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
            }
            
            initialZOffset = offset.z;
            if (mainCam != null) defaultFOV = mainCam.fieldOfView;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            HandlePosition();
            HandleRotation();
            HandleFOV();
            HandleShake();
        }

        private void HandlePosition()
        {
            float currentSpeed = PlayerController.Instance != null ? PlayerController.Instance.currentWorldSpeed : 0f;
            float maxSpeed = PlayerController.Instance != null ? PlayerController.Instance.maxSpeed : 100f;
            
            // Hıza göre dinamik offset ayarı (Hızlandıkça kamera biraz uzaklaşır)
            float speedFactor = currentSpeed / maxSpeed;
            Vector3 dynamicOffset = offset;
            dynamicOffset.z = initialZOffset + (speedFactor * maxSpeedOffsetZ);

            // Hedef pozisyonu hesapla (shake hariç)
            Vector3 targetPosition = target.position + dynamicOffset;

            // SmoothDamp ile yumuşak geçiş
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, positionSmoothTime);
            
            // Shake offset'i doğrudan uygula (timeScale=0 iken de çalışır)
            transform.position += shakeOffset;
        }

        private void HandleFOV()
        {
            if (mainCam == null || PlayerController.Instance == null) return;

            float currentSpeed = PlayerController.Instance.currentWorldSpeed;
            float maxSpeed = PlayerController.Instance.maxSpeed;
            
            // Hıza orantılı FOV artışı (cruise'dan max'a doğru kademeli)
            float speedRatio = Mathf.Clamp01(currentSpeed / maxSpeed);
            float speedFOV = defaultFOV + (speedRatio * speedFOVAmount);
            
            // Boost aktifken ekstra FOV ekle
            bool isBoosting = currentSpeed > maxSpeed * 0.9f;
            float targetFOV = isBoosting ? speedFOV + boostFOVAmount : speedFOV;
            
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, targetFOV, fovSmoothSpeed * Time.deltaTime);
        }

        private void HandleShake()
        {
            if (currentShakeTime > 0)
            {
                float normalizedTime = currentShakeTime / currentShakeDuration;
                float intensity = currentShakeIntensity * normalizedTime;
                
                // Pozisyon sarsıntısı
                shakeOffset = new Vector3(
                    Random.Range(-1f, 1f) * intensity,
                    Random.Range(-1f, 1f) * intensity * 0.6f,
                    Random.Range(-1f, 1f) * intensity * 0.3f
                );
                
                // Rotasyon sarsıntısı (çarpana göre ölçeklenir)
                float rotI = intensity * currentShakeRotMul;
                shakeRotation = Quaternion.Euler(
                    Random.Range(-rotI, rotI),
                    Random.Range(-rotI, rotI) * 0.5f,
                    Random.Range(-rotI, rotI) * 0.8f
                );
                
                currentShakeTime -= Time.unscaledDeltaTime;
            }
            else
            {
                shakeOffset = Vector3.zero;
                shakeRotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// Genel sarsıntı tetikleyici (varsayılan: kaza değerleri).
        /// </summary>
        public void TriggerShake(float intensity = -1f, float duration = -1f)
        {
            currentShakeDuration = duration > 0 ? duration : crashShakeDuration;
            currentShakeTime = currentShakeDuration;
            currentShakeIntensity = intensity > 0 ? intensity : crashShakeIntensity;
            currentShakeRotMul = crashShakeRotMul;
        }

        /// <summary>
        /// Tüm sarsıntıyı anında durdurur.
        /// </summary>
        public void StopShake()
        {
            currentShakeTime = 0f;
            currentShakeIntensity = 0f;
            shakeOffset = Vector3.zero;
            shakeRotation = Quaternion.identity;
        }

        /// <summary>
        /// Boost – hafif titreşim, kısa süre.
        /// </summary>
        public void TriggerBoostShake()
        {
            currentShakeDuration = boostShakeDuration;
            currentShakeTime = boostShakeDuration;
            currentShakeIntensity = boostShakeIntensity;
            currentShakeRotMul = boostShakeRotMul;
        }

        /// <summary>
        /// Kaza – orta şiddet, belirgin darbe hissi.
        /// </summary>
        public void TriggerCrashShake()
        {
            currentShakeDuration = crashShakeDuration;
            currentShakeTime = crashShakeDuration;
            currentShakeIntensity = crashShakeIntensity;
            currentShakeRotMul = crashShakeRotMul;
        }

        /// <summary>
        /// Ölüm – dramatik, uzun ve güçlü sarsıntı.
        /// </summary>
        public void TriggerDeathShake()
        {
            currentShakeDuration = deathShakeDuration;
            currentShakeTime = deathShakeDuration;
            currentShakeIntensity = deathShakeIntensity;
            currentShakeRotMul = deathShakeRotMul;
        }

        private void HandleRotation()
        {
            Quaternion targetRotation = Quaternion.LookRotation(target.position + Vector3.up - transform.position);
            baseRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothTime * Time.deltaTime * 10f);
            // Shake rotasyonunu üzerine ekle
            transform.rotation = baseRotation * shakeRotation;
        }

        /// <summary>
        /// Kamerayı anlık olarak hedefe kilitler (Sahne başlangıcında veya ışınlanmalarda kullanılır).
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null) return;
            transform.position = target.position + offset;
            transform.LookAt(target.position + Vector3.up);
        }
    }
}
