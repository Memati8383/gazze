using UnityEngine;
using TMPro;

namespace Gazze.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string localizationKey;

        private TextMeshProUGUI textComponent;

        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            if (LocalizationManager.Instance != null)
            {
                // OnEnable'da manager hazır değilse burada abone ol ve güncelle
                LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
                LocalizationManager.Instance.OnLanguageChanged += UpdateText;
                UpdateText();
            }
        }

        private void OnEnable()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += UpdateText;
                UpdateText();
            }
        }

        private void OnDisable()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
            }
        }

        public void UpdateText()
        {
            if (string.IsNullOrEmpty(localizationKey) || localizationKey == "ENTER_KEY_HERE") return;

            if (textComponent == null) textComponent = GetComponent<TextMeshProUGUI>();

            if (textComponent != null && LocalizationManager.Instance != null)
            {
                textComponent.text = LocalizationManager.Instance.GetTranslation(localizationKey);
            }
        }

        // Key'i dinamik olarak değiştirmek gerekirse
        public void SetKey(string key)
        {
            localizationKey = key;
            UpdateText();
        }
    }
}
