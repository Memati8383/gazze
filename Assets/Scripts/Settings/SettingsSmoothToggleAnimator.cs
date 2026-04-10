using UnityEngine;
using UnityEngine.UI;

namespace Settings
{
    /// <summary>iOS tarzı pill toggle animatörü — handle kayma + renk geçişi</summary>
    public class SettingsSmoothToggleAnimator : MonoBehaviour
    {
        public RectTransform handle;
        public Image bgImage;
        public Color onColor, offColor;
        public float onX = 14f, offX = -14f;
        public bool IsOn;

        private Toggle _toggle;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            if (_toggle != null)
            {
                IsOn = _toggle.isOn;
                _toggle.onValueChanged.AddListener(val => SetState(val));
            }
        }

        public void SetState(bool isOn, bool instant = false)
        {
            IsOn = isOn;
            if (instant && handle && bgImage)
            {
                handle.anchoredPosition = new Vector2(IsOn ? onX : offX, 0);
                bgImage.color = IsOn ? onColor : offColor;
            }
        }

        void Update()
        {
            if (!handle || !bgImage) return;
            float dt = Time.unscaledDeltaTime * 14f; // Slower but smoother
            
            // Handle position lerp
            var targetX = IsOn ? onX : offX;
            var pos = handle.anchoredPosition;
            if (Mathf.Abs(pos.x - targetX) > 0.01f)
            {
                pos.x = Mathf.Lerp(pos.x, targetX, dt);
                handle.anchoredPosition = pos;
            }

            // Bg color lerp
            var targetColor = IsOn ? onColor : offColor;
            if (bgImage.color != targetColor)
            {
                bgImage.color = Color.Lerp(bgImage.color, targetColor, dt);
            }
        }
    }
}
