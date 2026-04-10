#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Gazze.UI;

namespace Settings.Editor
{
    /// <summary>
    /// Dayaniklilik slider'ini kalp tabanli can gostergesine donusturen editor aracidir.
    /// </summary>
    public class HeartHealthBuilder
    {
        [MenuItem("Tools/Gazze/Upgrade Health UI to Hearts")]
        public static void Build()
        {
            GameObject canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null)
            {
                Debug.LogError("Canvas bulunamadı!");
                return;
            }

            GameObject playerGo = GameObject.Find("Player");
            if (playerGo == null)
            {
                Debug.LogError("Player bulunamadı!");
                return;
            }

            PlayerController pc = playerGo.GetComponent<PlayerController>();

            // 1. Assets
            Sprite fullHeart = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Violet Theme Ui/Colored Icons/Heart.png");
            Sprite emptyHeart = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Violet Theme Ui/Colored Icons/Heart Dark.png");

            // 2. Clear Old Heart Container if exists
            GameObject old = GameObject.Find("HeartContainer");
            if (old) Object.DestroyImmediate(old);

            // 3. Create Container
            GameObject container = new GameObject("HeartContainer", typeof(RectTransform));
            container.transform.SetParent(canvasGo.transform, false);
            
            var rect = container.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1);
            rect.anchorMax = new Vector2(0.5f, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, -90); // Home butonunun biraz altında
            rect.sizeDelta = new Vector2(250, 60);

            // Add Layout
            var hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            var display = container.AddComponent<HealthHeartDisplay>();
            display.fullHeartSprite = fullHeart;
            display.emptyHeartSprite = emptyHeart;

            // 4. Setup Dynamic Hearts (Initial 100 health = 4 hearts)
            display.SetupHearts(100);

            // 5. Connect to PlayerController
            pc.heartDisplay = display;
            EditorUtility.SetDirty(pc);

            Selection.activeGameObject = container;
            Debug.Log("<color=red>Gazze:</color> Health UI upgraded to Hearts!");
        }
    }
}
#endif
