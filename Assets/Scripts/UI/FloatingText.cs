using UnityEngine;
using TMPro;

namespace Gazze.UI
{
    /// <summary>
    /// Kisa omurlu, yukariya hareket eden ve fade olan dunya uzayi metnini yonetir.
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        [Header("Animasyon")]
        [Tooltip("Metnin sahnede kalma suresi (sn).")]
        public float duration = 1.0f;
        [Tooltip("Metnin yukari hareket hizi.")]
        public float upwardSpeed = 1.5f;
        [Tooltip("Alpha degeri icin fade egirisi.")]
        public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [Tooltip("Olcek degisimi icin animasyon egirisi.")]
        public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1.0f, 1, 1.5f);

        private TextMeshPro textMesh;
        private Color startColor;
        private float timer;

        private void Awake()
        {
            textMesh = GetComponent<TextMeshPro>();
            if (textMesh != null) startColor = textMesh.color;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            if (t >= 1.0f)
            {
                Destroy(gameObject);
                return;
            }

            // Move up
            transform.position += Vector3.up * upwardSpeed * Time.deltaTime;

            // Fade
            if (textMesh != null)
            {
                Color c = startColor;
                c.a *= fadeCurve.Evaluate(t);
                textMesh.color = c;
            }

            // Scale
            transform.localScale = Vector3.one * scaleCurve.Evaluate(t);
        }

        public void SetText(string text)
        {
            if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
            if (textMesh != null) textMesh.text = text;
        }
    }
}
