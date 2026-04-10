using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Settings
{
    /// <summary>
    /// Premium button micro-animation with scale, color, shadow depth, and glowing outline transitions.
    /// </summary>
    public class SettingsFluidButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Image targetGraphic;
        public Color normalColor, hoverColor, pressColor;

        public Shadow targetShadow;
        public Vector2 normalShadow = new Vector2(0, -4);
        public Vector2 hoverShadow = new Vector2(0, -8);
        public Vector2 pressShadow = new Vector2(0, -1);

        public Outline targetOutline;
        public Color normalOutline = new Color(1f, 1f, 1f, 0.08f);
        public Color hoverOutline = new Color(1f, 1f, 1f, 0.4f);
        public Color pressOutline = new Color(1f, 1f, 1f, 0.02f);

        Vector3 _targetScale = Vector3.one;
        Color _targetColor;
        Vector2 _targetShadowDepth;
        Color _targetOutlineColor;

        void Start()
        {
            _targetColor = normalColor;
            _targetShadowDepth = normalShadow;
            _targetOutlineColor = normalOutline;

            if (targetShadow) targetShadow.effectDistance = normalShadow;
            if (targetOutline) targetOutline.effectColor = normalOutline;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, dt * 16f);
            
            if (targetGraphic)
                targetGraphic.color = Color.Lerp(targetGraphic.color, _targetColor, dt * 14f);

            if (targetShadow)
                targetShadow.effectDistance = Vector2.Lerp(targetShadow.effectDistance, _targetShadowDepth, dt * 18f);

            if (targetOutline)
                targetOutline.effectColor = Color.Lerp(targetOutline.effectColor, _targetOutlineColor, dt * 12f);
        }

        public void OnPointerEnter(PointerEventData e)
        {
            _targetScale = Vector3.one * 1.04f;
            _targetColor = hoverColor;
            _targetShadowDepth = hoverShadow;
            _targetOutlineColor = hoverOutline;
        }

        public void OnPointerExit(PointerEventData e)
        {
            _targetScale = Vector3.one;
            _targetColor = normalColor;
            _targetShadowDepth = normalShadow;
            _targetOutlineColor = normalOutline;
        }

        public void OnPointerDown(PointerEventData e)
        {
            _targetScale = Vector3.one * 0.94f;
            _targetColor = pressColor;
            _targetShadowDepth = pressShadow;
            _targetOutlineColor = pressOutline;
        }

        public void OnPointerUp(PointerEventData e)
        {
            _targetScale = Vector3.one * 1.04f;
            _targetColor = hoverColor;
            _targetShadowDepth = hoverShadow;
            _targetOutlineColor = hoverOutline;
        }
    }
}
