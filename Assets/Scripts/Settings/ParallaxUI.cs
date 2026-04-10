using UnityEngine;
using UnityEngine.InputSystem;

namespace Settings
{
    /// <summary>
    /// UI Elemanları için Parallax Efekti (New Input System Uyumlu)
    /// Fare hareketine göre arka planı veya diğer katmanları hafifçe kaydırır
    /// </summary>
    public class ParallaxUI : MonoBehaviour
    {
        [Header("Settings")]
        public float intensity = 25.0f; // Kayma miktarı (pixel)
        public float smoothTime = 0.15f; // Yumuşatma süresi

        private RectTransform _rect;
        private Vector2 _initialPos;
        private Vector2 _velocity;

        void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _initialPos = _rect.anchoredPosition;
            
            // Re-scale the object slightly to prevent gaps at edges during movement
            // Adding a larger margin based on intensity
            float scaleFactor = 1.2f; 
            _rect.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        }

        void Update()
        {
            Vector2 inputPos = Vector2.zero;
            bool inputDetected = false;

            if (Application.isMobilePlatform)
            {
                // Mobilde telefonun eğimine (Accelerometer) göre parallax
                if (Accelerometer.current != null)
                {
                    if (!Accelerometer.current.enabled) InputSystem.EnableDevice(Accelerometer.current);
                    
                    Vector3 accel = Accelerometer.current.acceleration.ReadValue();
                    // Cihazı sağ/sol yatırma (X) ve ön/arka yatırma (Y/Z)
                    // Y değeri genellikle tutuş açısına göre -0.5f civarındadır, bu yüzden ofsetliyoruz.
                    inputPos.x = accel.x * 2.5f; 
                    inputPos.y = (accel.z + 0.6f) * 2.5f;
                    inputDetected = true;
                }
                // İvmeölçer yoksa veya çalışmıyorsa dokunmatik konumu kullan
                if (!inputDetected && Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
                {
                    Vector2 tPos = Touchscreen.current.touches[0].position.ReadValue();
                    inputPos.x = (tPos.x / Screen.width) * 2f - 1f;
                    inputPos.y = (tPos.y / Screen.height) * 2f - 1f;
                    inputDetected = true;
                }
            }
            else
            {
                // Masaüstünde mouse konumu
                Vector2 mouseRaw = Vector2.zero;
                bool hasMouse = false;

                if (Mouse.current != null)
                {
                    mouseRaw = Mouse.current.position.ReadValue();
                    hasMouse = true;
                }
                else if (Pointer.current != null)
                {
                    mouseRaw = Pointer.current.position.ReadValue();
                    hasMouse = true;
                }

                if (hasMouse)
                {
                    inputPos.x = (mouseRaw.x / Screen.width) * 2f - 1f;
                    inputPos.y = (mouseRaw.y / Screen.height) * 2f - 1f;
                    inputDetected = true;
                }
            }

            if (!inputDetected) return;

            // Sınırla ve yumuşat
            float normX = Mathf.Clamp(inputPos.x, -1f, 1f);
            float normY = Mathf.Clamp(inputPos.y, -1f, 1f);

            // Target position calculation
            Vector2 targetOffset = new Vector2(normX * intensity, normY * intensity);
            Vector2 targetPos = _initialPos + targetOffset;

            // Yumuşak geçiş
            _rect.anchoredPosition = Vector2.SmoothDamp(_rect.anchoredPosition, targetPos, ref _velocity, smoothTime);
        }
    }
}
