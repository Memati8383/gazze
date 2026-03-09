using UnityEngine;
using UnityEngine.UI;

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
        MenuNavigator nav = Object.FindFirstObjectByType<MenuNavigator>();
        if (nav != null)
        {
            nav.LoadMainMenu();
        }
        else
        {
            // Eğer sahnedeki navigatör bulunamazsa geçici bir tane oluştur veya direkt yükle
            if (LoadingManager.Instance != null)
                LoadingManager.Instance.LoadScene("MainMenu");
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
