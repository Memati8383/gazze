using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Settings
{
    /// <summary>
    /// Staggered fade+slide entrance animation for settings sections.
    /// Only animates at runtime; in editor, sections remain fully visible.
    /// </summary>
    public class SettingsSectionEntrance : MonoBehaviour
    {
        public float delay = 0f;

        private CanvasGroup _cg;
        private RectTransform _rt;
        private Vector2 _targetPos;

        private void OnEnable()
        {
            _cg = GetComponent<CanvasGroup>();
            _rt = GetComponent<RectTransform>();

            if (_cg == null) return;

            // Only animate at runtime
            if (!Application.isPlaying)
            {
                _cg.alpha = 1f;
                return;
            }

            _cg.alpha = 0f;
            StartCoroutine(AnimateIn());
        }

        private IEnumerator AnimateIn()
        {
            if (delay > 0f)
                yield return new WaitForSecondsRealtime(delay);

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Ease out cubic
                float ease = 1f - Mathf.Pow(1f - t, 3f);

                _cg.alpha = ease;
                yield return null;
            }

            _cg.alpha = 1f;
        }
    }
}
