/**
 * @file Rotator.cs
 * @author Unity MCP Assistant
 * @date 2026-02-28
 * @last_update 2026-02-28
 * @description Bir nesnenin kendi eksenleri etrafında sürekli dönmesini sağlayan yardımcı sınıftır.
 */

using UnityEngine;

/// <summary>
/// Bir nesnenin kendi ekseni etrafında sürekli dönmesini sağlar (Örn: Yardım kolileri, paralar).
/// </summary>
public class Rotator : MonoBehaviour
{
    /// <summary> Her bir eksen (X, Y, Z) için saniyelik dönüş hızı (derece). </summary>
    [Tooltip("Dönüş hızı (X, Y, Z eksenleri için).")]
    public Vector3 rotationSpeed = new(0, 100, 0);

    private void Update()
    {
        // Nesneyi belirlenen eksen ve hızda her karede döndür.
        // Space.Self (Varsayılan): Nesnenin kendi yerel eksenlerine göre döndürür.
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}