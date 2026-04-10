using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Settings
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UISliderValueBinder : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private Slider _slider;
        public string format = "{0}%";
        public float multiplier = 100f;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            // En yakın ebeveynde veya komşuda slider ara
            _slider = GetComponentInParent<Slider>();
            if (_slider == null)
            {
                // Alternatif olarak container içinde ara (HorizontalLayoutGroup için)
                _slider = transform.parent?.parent?.GetComponentInChildren<Slider>();
            }
        }

        private void OnEnable()
        {
            if (_slider != null)
            {
                _slider.onValueChanged.AddListener(OnUpdateValue);
                OnUpdateValue(_slider.value);
            }
        }

        private void OnDisable()
        {
            if (_slider != null)
            {
                _slider.onValueChanged.RemoveListener(OnUpdateValue);
            }
        }

        private void OnUpdateValue(float val)
        {
            if (_text != null)
            {
                _text.text = string.Format(format, Mathf.RoundToInt(val * multiplier));
            }
        }
    }
}
