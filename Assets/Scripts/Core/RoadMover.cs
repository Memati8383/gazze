using UnityEngine;

/// <summary>
/// Nesnelerin sadece basit bir hızla geriye doğru gitmesini sağlayan yardımcı sınıf.
/// </summary>
public class RoadMover : MonoBehaviour
{
    /// <summary> Nesnenin geriye doğru kayma hızı. </summary>
    [Tooltip("Nesnenin geriye doğru kayma hızı.")]
    public float speed = 10f;

    private void Update()
    {
        // Nesneyi belirlenen hızda Z ekseninde geriye taşı
        transform.Translate(Vector3.back * speed * Time.deltaTime);

        // Belirli bir sınırın dışına çıktığında öne ışınla (Basit bir döngü mekanizması örneği)
        // Not: Bu değerler sahne ölçeğine göre ayarlanmalıdır.
        if (transform.position.z <= -10f)
        {
            transform.position += new Vector3(0, 0, 20f);
        }
    }
}