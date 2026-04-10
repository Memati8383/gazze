using UnityEngine;
using UnityEngine.UI;

namespace Settings
{
    /// <summary>Kart kenarında nefes alan gold↔copper gradient animasyonu</summary>
    public class SettingsGradientBorderPulse : MonoBehaviour
    {
        public Outline outline;

        static readonly Color BorderCyan   = new Color32(255, 191, 36, 100);  // Gold
        static readonly Color BorderViolet = new Color32(205, 127, 50, 100);  // Copper

        void Update()
        {
            if (!outline) return;
            float t = Time.unscaledTime;

            // Slow color ping-pong between cyan and violet
            float lerp = Mathf.Sin(t * 1.2f) * 0.5f + 0.5f;
            Color pulseColor = Color.Lerp(BorderCyan, BorderViolet, lerp);

            // Breathing alpha
            float alpha = 0.18f + 0.12f * Mathf.Sin(t * 2.0f);
            pulseColor.a = alpha;

            outline.effectColor = pulseColor;

            // Subtle distance oscillation
            float dist = 1.0f + 0.25f * Mathf.Cos(t * 0.9f);
            outline.effectDistance = new Vector2(dist, -dist);
        }
    }
}
