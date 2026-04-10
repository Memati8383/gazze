using UnityEngine;
using System.Collections;

namespace Gazze.UI
{
    public class MenuAnimationController : MonoBehaviour
    {
        public RectTransform titleGroup;
        public RectTransform buttonGroup;
        public RectTransform[] hudElements;

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                ResetToFinalState();
                return;
            }

            StopAllCoroutines();
            StartCoroutine(EntryRoutine());
        }

        public void ResetToFinalState()
        {
            if (titleGroup) titleGroup.anchoredPosition = new Vector2(0, titleGroup.anchoredPosition.y);
            if (buttonGroup) buttonGroup.anchoredPosition = new Vector2(0, buttonGroup.anchoredPosition.y);

            if (hudElements != null)
            {
                foreach (var hud in hudElements)
                {
                    if (hud)
                    {
                        hud.localScale = Vector3.one;
                        var cg = hud.GetComponent<CanvasGroup>();
                        if (cg) cg.alpha = 1f;
                    }
                }
            }

            var panelCg = GetComponent<CanvasGroup>();
            if (panelCg) panelCg.alpha = 1f;
        }

        private IEnumerator EntryRoutine()
        {
            if (titleGroup) titleGroup.anchoredPosition = new Vector2(-600, titleGroup.anchoredPosition.y);
            if (buttonGroup) buttonGroup.anchoredPosition = new Vector2(600, buttonGroup.anchoredPosition.y);

            if (hudElements != null)
                foreach (var hud in hudElements)
                    if (hud) hud.localScale = Vector3.zero;

            yield return new WaitForSecondsRealtime(0.05f);

            float duration = 0.65f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = 1f - Mathf.Pow(1f - t, 4f); // Quartic Out

                if (titleGroup)
                    titleGroup.anchoredPosition = new Vector2(Mathf.Lerp(-600, 0, easedT), titleGroup.anchoredPosition.y);
                if (buttonGroup)
                    buttonGroup.anchoredPosition = new Vector2(Mathf.Lerp(600, 0, easedT), buttonGroup.anchoredPosition.y);

                if (hudElements != null)
                    foreach (var hud in hudElements)
                        if (hud) hud.localScale = Vector3.one * Mathf.Lerp(0, 1, Mathf.Clamp01(t * 1.5f));

                yield return null;
            }

            ResetToFinalState();
        }
    }
}
