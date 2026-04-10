using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Gazze.UI
{
    /// <summary>
    /// Ana menüye derinlik katan, mouse hareketine duyarlı parallax sistemi.
    /// V2: Serialization ve Editor-safe geliştirmeleri.
    /// </summary>
    public class MenuParallaxEffect : MonoBehaviour
    {
        [Header("3D Camera Parallax")]
        public Transform targetCamera;
        public float cameraMoveAmount = 0.3f;
        public float cameraRotationAmount = 0.8f;

        [Header("UI Layer Parallax")]
        public RectTransform[] uiLayers;
        public float[] layerStrengths;

        [Header("Background Parallax")]
        public RectTransform backgroundLayer;
        public float backgroundStrength = 15f;

        [Header("Settings")]
        public float smoothTime = 0.25f;
        public bool useUnscaledTime = true;
        [Range(0, 2)]
        public float globalMultiplier = 1.0f;

        // Gizli ama seri hale getirilen veri (Domain reload'dan kurtulur)
        [SerializeField, HideInInspector] private Vector3 initialCameraPos;
        [SerializeField, HideInInspector] private Quaternion initialCameraRot;
        [SerializeField, HideInInspector] private Vector2[] initialUiPositions;
        [SerializeField, HideInInspector] private Vector2 initialBgPos;
        [SerializeField, HideInInspector] private bool isCaptured = false;
        
        private Vector2 smoothMousePos;
        private Vector2 currentVelocity;

        private void Start()
        {
            if (!isCaptured) CaptureInitialState();
        }

        [ContextMenu("Capture Initial State")]
        public void CaptureInitialState()
        {
            if (targetCamera)
            {
                initialCameraPos = targetCamera.localPosition;
                initialCameraRot = targetCamera.localRotation;
            }

            if (backgroundLayer)
            {
                initialBgPos = backgroundLayer.anchoredPosition;
            }

            if (uiLayers != null && uiLayers.Length > 0)
            {
                initialUiPositions = new Vector2[uiLayers.Length];
                for (int i = 0; i < uiLayers.Length; i++)
                {
                    if (uiLayers[i]) initialUiPositions[i] = uiLayers[i].anchoredPosition;
                }
            }
            isCaptured = true;
        }

        [ContextMenu("Reset to Initial State")]
        public void ResetToInitial()
        {
            if (!isCaptured) return;

            if (targetCamera)
            {
                targetCamera.localPosition = initialCameraPos;
                targetCamera.localRotation = initialCameraRot;
            }

            if (backgroundLayer)
            {
                backgroundLayer.anchoredPosition = initialBgPos;
            }

            if (uiLayers != null && initialUiPositions != null)
            {
                for (int i = 0; i < uiLayers.Length; i++)
                {
                    if (uiLayers[i] && i < initialUiPositions.Length)
                    {
                        uiLayers[i].anchoredPosition = initialUiPositions[i];
                    }
                }
            }
        }

        private void Update()
        {
            if (!Application.isPlaying || !isCaptured) return;

            Vector2 inputPos = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (Application.isMobilePlatform)
            {
                // Mobilde telefonun eğimine (Accelerometer) göre parallax
                if (Accelerometer.current != null)
                {
                    if (!Accelerometer.current.enabled) InputSystem.EnableDevice(Accelerometer.current);
                    
                    Vector3 accel = Accelerometer.current.acceleration.ReadValue();
                    // Cihazı sağ/sol yatırma (X) ve ön/arka yatırma (Y)
                    // Y değeri genellikle tutuş açısına göre -0.5f civarındadır, bu yüzden ofsetliyoruz.
                    inputPos.x = accel.x * 2.5f; 
                    inputPos.y = (accel.z + 0.6f) * 2.5f; 
                }
                // İvmeölçer yoksa dokunmatik konumu kullan
                else if (Pointer.current != null)
                {
                    Vector2 pPos = Pointer.current.position.ReadValue();
                    inputPos.x = (pPos.x / Screen.width) * 2f - 1f;
                    inputPos.y = (pPos.y / Screen.height) * 2f - 1f;
                }
            }
            else
            {
                // Masaüstünde mouse konumu
                Vector2 rawPos = Vector2.zero;
                if (Mouse.current != null) rawPos = Mouse.current.position.ReadValue();
                else rawPos = Input.mousePosition;

                inputPos.x = (rawPos.x / Screen.width) * 2f - 1f;
                inputPos.y = (rawPos.y / Screen.height) * 2f - 1f;
            }
#else
            // Legacy fallback
            Vector2 rawPos = Input.mousePosition;
            inputPos.x = (rawPos.x / Screen.width) * 2f - 1f;
            inputPos.y = (rawPos.y / Screen.height) * 2f - 1f;
#endif

            // Sınırla
            inputPos.x = Mathf.Clamp(inputPos.x, -1f, 1f);
            inputPos.y = Mathf.Clamp(inputPos.y, -1f, 1f);

            float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            smoothMousePos = Vector2.SmoothDamp(smoothMousePos, inputPos, ref currentVelocity, smoothTime, Mathf.Infinity, delta);

            ApplyParallax();
        }

        private void ApplyParallax()
        {
            float m = globalMultiplier;

            // 1. Kamera
            if (targetCamera)
            {
                targetCamera.localPosition = initialCameraPos + new Vector3(smoothMousePos.x * cameraMoveAmount * m, smoothMousePos.y * cameraMoveAmount * m, 0);
                targetCamera.localRotation = initialCameraRot * Quaternion.Euler(-smoothMousePos.y * cameraRotationAmount * m, smoothMousePos.x * cameraRotationAmount * m, 0);
            }

            // 2. Arka Plan
            if (backgroundLayer)
            {
                backgroundLayer.anchoredPosition = initialBgPos - new Vector2(smoothMousePos.x * backgroundStrength * m, smoothMousePos.y * backgroundStrength * m);
            }

            // 3. UI Katmanları
            if (uiLayers != null && initialUiPositions != null)
            {
                for (int i = 0; i < uiLayers.Length; i++)
                {
                    if (uiLayers[i] && i < initialUiPositions.Length && i < layerStrengths.Length)
                    {
                        uiLayers[i].anchoredPosition = initialUiPositions[i] + new Vector2(smoothMousePos.x * layerStrengths[i] * m, smoothMousePos.y * layerStrengths[i] * m);
                    }
                }
            }
        }
    }
}
