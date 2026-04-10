using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Settings
{
    /// <summary>
    /// Catch SetValueWithoutNotify and keep slider percentage text in sync.
    /// Used by SettingsVisualOverhaul for dynamic UI generation.
    /// </summary>
    public class SliderValueTextSync : MonoBehaviour
    {
        [Tooltip("The text component that displays the percentage.")]
        public TextMeshProUGUI targetText;
        
        private Slider _slider;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        private void Update() 
        { 
            if (_slider != null && targetText != null) 
            {
                string v = Mathf.RoundToInt(_slider.value * 100) + "%";
                if (targetText.text != v)
                {
                    targetText.text = v;
                }
            }
        }
    }
}
