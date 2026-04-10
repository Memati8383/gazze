using UnityEngine;
using UnityEngine.UI;

namespace Settings
{
    /// <summary>
    /// Premium Aurora arkaplan efekti. 
    /// Arkaplanda yavaşça süzülen renkli 'blob'lar sayesinde 'Frosted Midnight' estetiğini güçlendirir.
    /// </summary>
    public class SettingsAuroraBackground : MonoBehaviour
    {
        public RectTransform blob1, blob2, blob3;
        
        void Update()
        {
            float t = Time.unscaledTime * 0.35f;
            
            if (blob1) Move(blob1, t, 1.0f, 150f, 120f, 0.7f);
            if (blob2) Move(blob2, t * 0.8f, 1.1f, 180f, 150f, 1.3f);
            if (blob3) Move(blob3, t * 1.2f, 0.6f, 220f, 100f, 0.4f);
        }

        void Move(RectTransform rt, float t, float speedX, float ampX, float ampY, float offset)
        {
            rt.anchoredPosition = new Vector2(
                Mathf.Sin(t * speedX + offset) * ampX,
                Mathf.Cos(t * 0.85f * speedX) * ampY
            );
            rt.Rotate(0, 0, Time.unscaledDeltaTime * 15f);
        }
    }
}
