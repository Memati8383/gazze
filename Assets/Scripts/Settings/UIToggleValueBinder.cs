using UnityEngine;
using UnityEngine.UI;

namespace Settings
{
    /// <summary>
    /// Toggle value binder with smooth handle slide + color transitions.
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class UIToggleValueBinder : MonoBehaviour
    {
        public RectTransform handleRect;
        public Image backgroundImage;
        public Image accentBarImage;

        public Color activeColor = new Color(0.18f, 0.8f, 0.44f, 1f);
        public Color inactiveColor = new Color(0.24f, 0.24f, 0.35f, 1f);
        public Color activeBarColor = new Color(0.18f, 0.8f, 0.44f, 1f);
        public Color inactiveBarColor = new Color(0.58f, 0.64f, 0.72f, 1f);

        public Vector2 activePos = new Vector2(37, 0);
        public Vector2 inactivePos = new Vector2(13, 0);

        private Toggle _toggle;
        private bool _targetState;
        private bool _animating;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
        }

        private void OnEnable()
        {
            if (_toggle != null)
            {
                _toggle.onValueChanged.AddListener(OnUpdateValue);
                // Apply initial state immediately
                ApplyImmediate(_toggle.isOn);
            }
        }

        private void OnDisable()
        {
            if (_toggle != null)
            {
                _toggle.onValueChanged.RemoveListener(OnUpdateValue);
            }
        }

        public void OnUpdateValue(bool isOn)
        {
            _targetState = isOn;
            _animating = true;
        }

        private void Update()
        {
            if (!_animating) return;
            float speed = Time.unscaledDeltaTime * 12f;

            Vector2 targetPos = _targetState ? activePos : inactivePos;
            Color targetBg = _targetState ? activeColor : inactiveColor;
            Color targetBar = _targetState ? activeBarColor : inactiveBarColor;

            bool done = true;

            if (handleRect != null)
            {
                handleRect.anchoredPosition = Vector2.Lerp(handleRect.anchoredPosition, targetPos, speed);
                if (Vector2.Distance(handleRect.anchoredPosition, targetPos) > 0.1f) done = false;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = Color.Lerp(backgroundImage.color, targetBg, speed);
                if (!ColorClose(backgroundImage.color, targetBg)) done = false;
            }

            if (accentBarImage != null)
            {
                accentBarImage.color = Color.Lerp(accentBarImage.color, targetBar, speed);
                if (!ColorClose(accentBarImage.color, targetBar)) done = false;
            }

            if (done)
            {
                ApplyImmediate(_targetState);
                _animating = false;
            }
        }

        private void ApplyImmediate(bool isOn)
        {
            _targetState = isOn;
            if (handleRect != null) handleRect.anchoredPosition = isOn ? activePos : inactivePos;
            if (backgroundImage != null) backgroundImage.color = isOn ? activeColor : inactiveColor;
            if (accentBarImage != null) accentBarImage.color = isOn ? activeBarColor : inactiveBarColor;
        }

        private static bool ColorClose(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.01f &&
                   Mathf.Abs(a.g - b.g) < 0.01f &&
                   Mathf.Abs(a.b - b.b) < 0.01f &&
                   Mathf.Abs(a.a - b.a) < 0.01f;
        }
    }
}
