using UnityEngine;
using UnityEngine.UI;

namespace Gazze.PowerUps
{
    /// <summary>
    /// Bootstrapper that ensures PowerUpManager has data for all power-up types
    /// and PowerUpEffects singleton exists at runtime. Also creates the TimeWarp overlay.
    /// </summary>
    public class PowerUpBootstrapper : MonoBehaviour
    {
        [Header("Icon Sprites (assigned in Inspector or loaded from Resources)")]
        public Sprite timeWarpIcon;
        public Sprite shockWaveIcon;
        public Sprite juggernautIcon;

        private void Awake()
        {
            // 1. Ensure PowerUpEffects exists
            if (PowerUpEffects.Instance == null)
            {
                GameObject effectsGO = new GameObject("PowerUpEffects");
                effectsGO.AddComponent<PowerUpEffects>();
                DontDestroyOnLoad(effectsGO);
            }

            // 2. Ensure PowerUpManager has data for new types
            StartCoroutine(ConfigureAfterFrame());
        }

        private System.Collections.IEnumerator ConfigureAfterFrame()
        {
            // Wait one frame for Awake chain to complete
            yield return null;

            var mgr = PowerUpManager.Instance;
            if (mgr == null) yield break;

            // Load icons from Assets if not assigned
            if (timeWarpIcon == null) timeWarpIcon = LoadIcon("zaman bükücü");
            if (shockWaveIcon == null) shockWaveIcon = LoadIcon("Şok Dalgası");
            if (juggernautIcon == null) juggernautIcon = LoadIcon("dev modu");

            // Check existing power-up data and add missing entries
            var existing = mgr.availablePowerUps != null 
                ? new System.Collections.Generic.List<PowerUpData>(mgr.availablePowerUps) 
                : new System.Collections.Generic.List<PowerUpData>();

            bool hasTimeWarp = false, hasShockWave = false, hasJuggernaut = false;
            foreach (var d in existing)
            {
                if (d.type == PowerUpType.TimeWarp) hasTimeWarp = true;
                if (d.type == PowerUpType.ShockWave) hasShockWave = true;
                if (d.type == PowerUpType.Juggernaut) hasJuggernaut = true;
            }

            if (!hasTimeWarp)
            {
                existing.Add(new PowerUpData
                {
                    type = PowerUpType.TimeWarp,
                    duration = 5f,
                    icon = timeWarpIcon,
                    themeColor = new Color(0.4f, 0.2f, 0.9f, 1f), // Mor/Mavi
                    displayName = "ZAMAN BÜKÜCÜ"
                });
            }

            if (!hasShockWave)
            {
                existing.Add(new PowerUpData
                {
                    type = PowerUpType.ShockWave,
                    duration = 0.1f, // Anlık efekt
                    icon = shockWaveIcon,
                    themeColor = new Color(0.3f, 0.7f, 1f, 1f), // Açık Mavi
                    displayName = "ŞOK DALGASI"
                });
            }

            if (!hasJuggernaut)
            {
                existing.Add(new PowerUpData
                {
                    type = PowerUpType.Juggernaut,
                    duration = 6f,
                    icon = juggernautIcon,
                    themeColor = new Color(1f, 0.5f, 0f, 1f), // Turuncu
                    displayName = "DEV MODU"
                });
            }

            mgr.availablePowerUps = existing.ToArray();

            // 3. Create TimeWarp overlay in the Canvas
            SetupTimeWarpOverlay();
        }

        private void SetupTimeWarpOverlay()
        {
            if (PowerUpEffects.Instance != null && PowerUpEffects.Instance.timeWarpOverlay != null) return;

            // Find the main Canvas
            Canvas mainCanvas = null;
            var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var c in canvases)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay || c.name == "Canvas")
                {
                    mainCanvas = c;
                    break;
                }
            }

            if (mainCanvas == null) return;

            // Create a full-screen overlay Image
            GameObject overlayGO = new GameObject("TimeWarpOverlay");
            overlayGO.transform.SetParent(mainCanvas.transform, false);

            RectTransform rt = overlayGO.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = overlayGO.AddComponent<Image>();
            img.color = new Color(0.3f, 0.15f, 0.8f, 0f); // Mavi-Mor, başlangıçta transparan
            img.raycastTarget = false; // Tıklamayı engellemez

            overlayGO.SetActive(false);

            // Assign to PowerUpEffects
            if (PowerUpEffects.Instance != null)
            {
                PowerUpEffects.Instance.timeWarpOverlay = img;
            }
        }

        private Sprite LoadIcon(string name)
        {
            // Try loading from Resources first
            Sprite sprite = Resources.Load<Sprite>(name);
            if (sprite != null) return sprite;

            // Try loading as Texture2D and converting
            Texture2D tex = Resources.Load<Texture2D>(name);
            if (tex != null)
            {
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }

            return null;
        }
    }
}
