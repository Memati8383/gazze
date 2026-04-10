using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Üzerindeki butonu duraklatma menüsüne bağlayan yardımcı bileşen.
/// Btn_ReturnMenu butonuna eklenir — artık direkt ana menüye gitmek yerine
/// duraklatma panelini açar/kapatır.
/// </summary>
public class MenuButtonLinker : MonoBehaviour
{
    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnClicked);
        }
    }

    void OnClicked()
    {
        if (Settings.AudioManager.Instance != null)
            Settings.AudioManager.Instance.PlayClickSound();

        Gazze.UI.PauseMenuBuilder.Toggle();
    }
}
