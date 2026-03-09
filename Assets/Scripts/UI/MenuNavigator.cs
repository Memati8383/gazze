using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sahneler arası geçişi sağlayan yardımcı sınıf.
/// </summary>
public class MenuNavigator : MonoBehaviour
{
    public void LoadMainMenu()
    {
        // AudioManager varsa tıklama sesini çal
        if (Settings.AudioManager.Instance != null)
        {
            Settings.AudioManager.Instance.PlayClickSound();
        }

        // Zaman akışını normale döndür (eğer oyun duraklatıldıysa)
        Time.timeScale = 1f;

        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
