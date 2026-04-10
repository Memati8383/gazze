using UnityEngine;

namespace Gazze.PowerUps
{
    /// <summary>
    /// Sahneye yerleştirilen veya spawn edilen güçlendirici (Power-Up) nesnesidir.
    /// Oyuncu ile temas ettiğinde ilgili güçlendiriciyi aktif eder.
    /// </summary>
    public class PowerUpItem : MonoBehaviour
    {
        [Header("Güçlendirici Ayarları")]
        [Tooltip("Bu nesnenin oyuncuya vereceği güçlendirici türü.")]
        public PowerUpType powerUpType;
        [Tooltip("Nesnenin kendi ekseni etrafında dönme hızı.")]
        public float rotationSpeed = 100f;
        [Tooltip("Nesnenin havada süzülme (bobbing) yüksekliği.")]
        public float floatHeight = 0.2f;

        [Header("Görsel ve İşitsel Efektler")]
        [Tooltip("Nesne toplandığında oluşacak parçacık efekti prefabı.")]
        public GameObject pickUpEffectPrefab;
        [Tooltip("Nesne toplandığında çalınacak ses efekti klibi.")]
        public AudioClip pickUpSound;
        
        [Header("Dünya Eğriliği (Yol shader'ı ile eşleşmelidir)")]
        [Tooltip("Dikey eksendeki eğrilik şiddeti.")]
        public float curvature = 0.002f;
        [Tooltip("Yatay eksendeki eğrilik şiddeti.")]
        public float curvatureH = -0.0015f;
        [Tooltip("Eğriliğin başlayacağı uzaklık farkı (Z).")]
        public float horizonOffset = 10f;

        private float baseX;
        private float baseY;

        private void Start()
        {
            // Orijinal pozisyonu kaydet
            baseX = transform.position.x;
            baseY = transform.position.y;
        }

        private void Update()
        {
            // Kendi ekseninde döndür
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // Havada süzülme (bobbing) ofseti hesapla
            float yBob = Mathf.Sin(Time.time * 2f) * floatHeight;

            // CPU tarafında dünya eğriliği hesaplaması — CurvedWorld_URP vertex shader ile aynı formül.
            // Bu, objelerin yolun eğriliğine göre doğru yerde görünmesini sağlar.
            float curveY = 0f;
            float curveX = 0f;
            if (Camera.main != null)
            {
                float distZ = Mathf.Max(0f, transform.position.z - Camera.main.transform.position.z - horizonOffset);
                curveY = -(distZ * distZ * curvature);
                curveX = distZ * distZ * curvatureH;
            }

            // Pozisyonu güncelle (Yatay/Dikey eğrilik + Süzülme)
            transform.position = new Vector3(
                baseX + curveX,
                baseY + yBob + curveY,
                transform.position.z
            );

            // Mesafe bazlı toplama kontrolü (Collider bazen ıskalayabildiği için ek güvenlik)
            if (PlayerController.Instance != null)
            {
                float dist = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
                // X ve Z koordinatları oyuncuya yeterince yakınsa topla
                if (dist < 1.5f && Mathf.Abs(transform.position.z - PlayerController.Instance.transform.position.z) < 1.5f)
                {
                    CollectPowerUp();
                }
            }
        }

        /// <summary>
        /// Güçlendiriciyi oyuncuya verir ve görsel/işitsel geri bildirim sağlar.
        /// </summary>
        private void CollectPowerUp()
        {
            if (PowerUpManager.Instance != null)
            {
                // Güçlendiriciyi yöneticiden aktif et
                PowerUpManager.Instance.ActivatePowerUp(powerUpType);
                
                // Geri bildirim efektleri
                if (pickUpEffectPrefab != null) Instantiate(pickUpEffectPrefab, transform.position, Quaternion.identity);
                if (pickUpSound != null && Settings.AudioManager.Instance != null) Settings.AudioManager.Instance.PlaySFX(pickUpSound);
                
                // Titreşim (Haptic)
                Settings.HapticManager.Heavy();
            }
            
            // Obje kullanıldı, sahneden çıkar
            gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Oyuncu ile çarpışma kontrolü (Tag bazlı)
            if (other.CompareTag("Player") || (other.transform.parent != null && other.transform.parent.CompareTag("Player")))
            {
                CollectPowerUp();
            }
        }
    }
}
