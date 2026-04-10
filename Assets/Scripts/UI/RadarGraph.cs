using UnityEngine;
using UnityEngine.UI;

namespace Gazze.UI
{
    /// <summary>
    /// Araç özelliklerini radar grafiği (beşgen) şeklinde gösteren UI bileşeni.
    /// </summary>
    public class RadarGraph : MaskableGraphic
    {
        [Tooltip("Radar grafiğinin yarıçapı.")]
        public float radius = 100f;

        [Header("Özellik Değerleri (0-1 aralığında)")]
        [Tooltip("Hiz degerinin normalize edilmis karsiligi.")]
        public float speed = 0.5f;
        [Tooltip("Ivme degerinin normalize edilmis karsiligi.")]
        public float acceleration = 0.5f;
        [Tooltip("Yol tutus/manevra degerinin normalize edilmis karsiligi.")]
        public float handling = 0.5f;
        [Tooltip("Dayaniklilik degerinin normalize edilmis karsiligi.")]
        public float durability = 0.5f;
        [Tooltip("Maliyet degerinin normalize edilmis karsiligi.")]
        public float cost = 0.5f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            // Merkez noktası
            vh.AddVert(Vector2.zero, color, Vector2.zero);

            // 5 köşeyi hesapla (72 derece arayla)
            float[] values = { speed, acceleration, handling, durability, cost };
            for (int i = 0; i < 5; i++)
            {
                float angle = (90f + i * 72f) * Mathf.Deg2Rad; // 90 derece ile üstten başla
                float dist = Mathf.Clamp01(values[i]) * radius;
                Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
                vh.AddVert(pos, color, Vector2.zero);
            }

            // Üçgenleri oluştur (merkezden dışa)
            for (int i = 1; i <= 5; i++)
            {
                int next = (i == 5) ? 1 : i + 1;
                vh.AddTriangle(0, i, next);
            }
        }

        /// <summary>
        /// Grafiği yeni değerlerle günceller.
        /// </summary>
        public void UpdateGraph(float speed, float accel, float handling, float durability, float cost)
        {
            this.speed = speed;
            this.acceleration = accel;
            this.handling = handling;
            this.durability = durability;
            this.cost = cost;
            SetVerticesDirty();
        }
    }
}
