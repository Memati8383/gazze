using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Settings
{
    public class LanguageSwitcherAdapter : MonoBehaviour
    {
        public Button trButton;
        public Button enButton;
        public SettingsView view;

        public Color activeColor = new Color32(100, 185, 255, 255);
        public Color inactiveColor = new Color32(25, 28, 55, 180);

        private void Start()
        {
            if (trButton != null) trButton.onClick.AddListener(() => SetLanguage(0));
            if (enButton != null) enButton.onClick.AddListener(() => SetLanguage(1));
            
            // Initial state
            if (PlayerPrefs.HasKey("Language"))
            {
                UpdateVisuals(PlayerPrefs.GetInt("Language", 0));
            }
        }

        private void SetLanguage(int index)
        {
            if (view != null) view.TriggerLanguageChange(index);
            UpdateVisuals(index);
        }

        public void UpdateVisuals(int activeIdx)
        {
            if (trButton != null) trButton.GetComponent<Image>().color = activeIdx == 0 ? activeColor : inactiveColor;
            if (enButton != null) enButton.GetComponent<Image>().color = activeIdx == 1 ? activeColor : inactiveColor;
            
            var trTxt = trButton?.GetComponentInChildren<TextMeshProUGUI>();
            var enTxt = enButton?.GetComponentInChildren<TextMeshProUGUI>();
            
            if (trTxt != null) trTxt.color = activeIdx == 0 ? Color.black : Color.white;
            if (enTxt != null) enTxt.color = activeIdx == 1 ? Color.black : Color.white;
        }
    }
}
