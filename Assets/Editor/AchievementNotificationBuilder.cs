using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

namespace Gazze.Editor
{
    /// <summary>
    /// Başarım bildirim UI'ını otomatik oluşturan editor script
    /// </summary>
    public static class AchievementNotificationBuilder
    {
        [MenuItem("Gazze/UI/Create Achievement Notification Prefab")]
        public static void CreateNotificationPrefab()
        {
            // Ana container - daha büyük
            GameObject notifObj = new GameObject("AchievementNotification", typeof(RectTransform));
            RectTransform mainRT = notifObj.GetComponent<RectTransform>();
            mainRT.sizeDelta = new Vector2(420f, 120f);
            mainRT.anchorMin = new Vector2(1f, 1f);
            mainRT.anchorMax = new Vector2(1f, 1f);
            mainRT.pivot = new Vector2(1f, 1f);
            
            // CanvasGroup ekle
            notifObj.AddComponent<CanvasGroup>();
            
            // Outer shadow (dış gölge)
            GameObject outerShadow = new GameObject("OuterShadow", typeof(RectTransform));
            outerShadow.transform.SetParent(notifObj.transform, false);
            RectTransform outerShadowRT = outerShadow.GetComponent<RectTransform>();
            outerShadowRT.anchorMin = Vector2.zero;
            outerShadowRT.anchorMax = Vector2.one;
            outerShadowRT.sizeDelta = new Vector2(10f, 10f);
            outerShadowRT.anchoredPosition = new Vector2(3f, -3f);
            
            Image outerShadowImage = outerShadow.AddComponent<Image>();
            outerShadowImage.color = new Color(0, 0, 0, 0.5f);
            outerShadowImage.raycastTarget = false;
            
            // Background - gradient effect için 2 katman
            GameObject bgObj = new GameObject("Background", typeof(RectTransform));
            bgObj.transform.SetParent(notifObj.transform, false);
            RectTransform bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);
            bgImage.raycastTarget = false;
            
            // Gradient overlay (üstten alta hafif gradient)
            GameObject gradientObj = new GameObject("GradientOverlay", typeof(RectTransform));
            gradientObj.transform.SetParent(bgObj.transform, false);
            RectTransform gradientRT = gradientObj.GetComponent<RectTransform>();
            gradientRT.anchorMin = Vector2.zero;
            gradientRT.anchorMax = Vector2.one;
            gradientRT.sizeDelta = Vector2.zero;
            
            Image gradientImage = gradientObj.AddComponent<Image>();
            gradientImage.color = new Color(1f, 1f, 1f, 0.05f);
            gradientImage.raycastTarget = false;
            
            // İç gölge (derinlik hissi)
            GameObject innerShadow = new GameObject("InnerShadow", typeof(RectTransform));
            innerShadow.transform.SetParent(bgObj.transform, false);
            RectTransform shadowRT = innerShadow.GetComponent<RectTransform>();
            shadowRT.anchorMin = new Vector2(0f, 0f);
            shadowRT.anchorMax = new Vector2(1f, 0f);
            shadowRT.pivot = new Vector2(0.5f, 0f);
            shadowRT.sizeDelta = new Vector2(0f, 20f);
            
            Image shadowImage = innerShadow.AddComponent<Image>();
            shadowImage.color = new Color(0, 0, 0, 0.4f);
            shadowImage.raycastTarget = false;
            
            // Glow effect (arka plan parlaması) - daha belirgin ve animasyonlu
            GameObject glowObj = new GameObject("Glow", typeof(RectTransform));
            glowObj.transform.SetParent(bgObj.transform, false);
            RectTransform glowRT = glowObj.GetComponent<RectTransform>();
            glowRT.anchorMin = Vector2.zero;
            glowRT.anchorMax = Vector2.one;
            glowRT.sizeDelta = new Vector2(12f, 12f);
            
            Image glowImage = glowObj.AddComponent<Image>();
            glowImage.color = new Color(1f, 0.8f, 0.2f, 0.5f);
            glowImage.raycastTarget = false;
            
            // Sol kenar accent (renkli çizgi) - daha kalın ve parlak
            GameObject accentObj = new GameObject("Accent", typeof(RectTransform));
            accentObj.transform.SetParent(bgObj.transform, false);
            RectTransform accentRT = accentObj.GetComponent<RectTransform>();
            accentRT.anchorMin = new Vector2(0f, 0f);
            accentRT.anchorMax = new Vector2(0f, 1f);
            accentRT.pivot = new Vector2(0f, 0.5f);
            accentRT.sizeDelta = new Vector2(10f, 0f);
            accentRT.anchoredPosition = Vector2.zero;
            
            Image accentImage = accentObj.AddComponent<Image>();
            accentImage.color = new Color(1f, 0.84f, 0f, 1f);
            accentImage.raycastTarget = false;
            
            // Accent glow (sol çizgi parlaması)
            GameObject accentGlowObj = new GameObject("AccentGlow", typeof(RectTransform));
            accentGlowObj.transform.SetParent(accentObj.transform, false);
            RectTransform accentGlowRT = accentGlowObj.GetComponent<RectTransform>();
            accentGlowRT.anchorMin = Vector2.zero;
            accentGlowRT.anchorMax = Vector2.one;
            accentGlowRT.sizeDelta = new Vector2(15f, 0f);
            accentGlowRT.anchoredPosition = new Vector2(7f, 0f);
            
            Image accentGlowImage = accentGlowObj.AddComponent<Image>();
            accentGlowImage.color = new Color(1f, 0.84f, 0f, 0.4f);
            accentGlowImage.raycastTarget = false;
            
            // Üst kenar highlight (parlak çizgi)
            GameObject highlightObj = new GameObject("TopHighlight", typeof(RectTransform));
            highlightObj.transform.SetParent(bgObj.transform, false);
            RectTransform highlightRT = highlightObj.GetComponent<RectTransform>();
            highlightRT.anchorMin = new Vector2(0f, 1f);
            highlightRT.anchorMax = new Vector2(1f, 1f);
            highlightRT.pivot = new Vector2(0.5f, 1f);
            highlightRT.sizeDelta = new Vector2(0f, 2f);
            highlightRT.anchoredPosition = Vector2.zero;
            
            Image highlightImage = highlightObj.AddComponent<Image>();
            highlightImage.color = new Color(1f, 1f, 1f, 0.2f);
            highlightImage.raycastTarget = false;
            
            // Icon container - daha büyük ve etkileyici
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform));
            iconObj.transform.SetParent(notifObj.transform, false);
            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0f, 0.5f);
            iconRT.anchorMax = new Vector2(0f, 0.5f);
            iconRT.pivot = new Vector2(0f, 0.5f);
            iconRT.sizeDelta = new Vector2(90f, 90f);
            iconRT.anchoredPosition = new Vector2(30f, 0f);
            
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = new Color(1f, 0.84f, 0f, 1f);
            iconImage.raycastTarget = false;
            
            // Icon outer glow (dış parlaklık) - 3 katman
            for (int i = 0; i < 3; i++)
            {
                GameObject iconGlowObj = new GameObject($"IconGlow{i}", typeof(RectTransform));
                iconGlowObj.transform.SetParent(iconObj.transform, false);
                iconGlowObj.transform.SetAsFirstSibling();
                RectTransform iconGlowRT = iconGlowObj.GetComponent<RectTransform>();
                iconGlowRT.anchorMin = Vector2.zero;
                iconGlowRT.anchorMax = Vector2.one;
                iconGlowRT.sizeDelta = new Vector2(20f + (i * 10f), 20f + (i * 10f));
                
                Image iconGlowImage = iconGlowObj.AddComponent<Image>();
                iconGlowImage.color = new Color(1f, 0.84f, 0f, 0.3f / (i + 1));
                iconGlowImage.raycastTarget = false;
            }
            
            // Icon background circle - gradient
            GameObject iconBgObj = new GameObject("IconBackground", typeof(RectTransform));
            iconBgObj.transform.SetParent(iconObj.transform, false);
            iconBgObj.transform.SetAsFirstSibling();
            RectTransform iconBgRT = iconBgObj.GetComponent<RectTransform>();
            iconBgRT.anchorMin = Vector2.zero;
            iconBgRT.anchorMax = Vector2.one;
            iconBgRT.sizeDelta = new Vector2(18f, 18f);
            
            Image iconBgImage = iconBgObj.AddComponent<Image>();
            iconBgImage.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
            iconBgImage.raycastTarget = false;
            
            // Text container
            GameObject textContainer = new GameObject("TextContainer", typeof(RectTransform));
            textContainer.transform.SetParent(notifObj.transform, false);
            RectTransform textRT = textContainer.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0f, 0f);
            textRT.anchorMax = new Vector2(1f, 1f);
            textRT.offsetMin = new Vector2(135f, 18f);
            textRT.offsetMax = new Vector2(-25f, -18f);
            
            VerticalLayoutGroup vlg = textContainer.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleLeft;
            vlg.spacing = 10f;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            
            // Title text - daha büyük, bold ve parlak
            GameObject titleObj = new GameObject("Title", typeof(RectTransform));
            titleObj.transform.SetParent(textContainer.transform, false);
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "BAŞARIM KAZANILDI!";
            titleText.fontSize = 26f;
            titleText.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            titleText.color = new Color(1f, 0.84f, 0f, 1f);
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.raycastTarget = false;
            titleText.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            titleText.overflowMode = TMPro.TextOverflowModes.Overflow;
            
            // Outline ve glow efekti
            titleText.outlineWidth = 0.25f;
            titleText.outlineColor = new Color(0, 0, 0, 0.8f);
            titleText.fontSharedMaterial = new Material(titleText.fontSharedMaterial);
            titleText.fontSharedMaterial.EnableKeyword("GLOW_ON");
            
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 38f;
            
            // Description text - daha okunabilir ve yumuşak
            GameObject descObj = new GameObject("Description", typeof(RectTransform));
            descObj.transform.SetParent(textContainer.transform, false);
            
            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "Yeni bir başarım kilidi açtınız!";
            descText.fontSize = 19f;
            descText.fontStyle = FontStyles.Normal;
            descText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            descText.alignment = TextAlignmentOptions.Left;
            descText.raycastTarget = false;
            descText.textWrappingMode = TMPro.TextWrappingModes.Normal;
            
            // Hafif gölge
            descText.outlineWidth = 0.15f;
            descText.outlineColor = new Color(0, 0, 0, 0.5f);
            
            LayoutElement descLE = descObj.AddComponent<LayoutElement>();
            descLE.preferredHeight = 32f;
            
            // AchievementNotification component ekle
            var notifComponent = notifObj.AddComponent<Gazze.UI.AchievementNotification>();
            
            // AchievementNotificationEffects component ekle
            notifObj.AddComponent<Gazze.UI.AchievementNotificationEffects>();
            
            // Referansları direkt ata (artık public)
            notifComponent.titleText = titleText;
            notifComponent.descriptionText = descText;
            notifComponent.iconImage = iconImage;
            notifComponent.backgroundImage = bgImage;
            
            // Prefab olarak kaydet
            string path = "Assets/Prefabs/UI/AchievementNotificationPrefab.prefab";
            string directory = System.IO.Path.GetDirectoryName(path);
            
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            PrefabUtility.SaveAsPrefabAsset(notifObj, path);
            Object.DestroyImmediate(notifObj);
            
            AssetDatabase.Refresh();
            
            Debug.Log($"Başarım bildirimi prefab'ı oluşturuldu: {path}");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        
        [MenuItem("Gazze/UI/Setup Achievement Notification Manager")]
        public static void SetupNotificationManager()
        {
            // Manager objesini oluştur veya bul
            var existing = Object.FindFirstObjectByType<Gazze.UI.AchievementNotificationManager>();
            if (existing != null)
            {
                Debug.Log("AchievementNotificationManager zaten sahnede mevcut.");
                Selection.activeGameObject = existing.gameObject;
                return;
            }
            
            GameObject managerObj = new GameObject("AchievementNotificationManager");
            var manager = managerObj.AddComponent<Gazze.UI.AchievementNotificationManager>();
            
            // Prefab'ı yükle ve ata
            string prefabPath = "Assets/Prefabs/UI/AchievementNotificationPrefab.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab != null)
            {
                var type = typeof(Gazze.UI.AchievementNotificationManager);
                var field = type.GetField("notificationPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(manager, prefab);
                
                Debug.Log("AchievementNotificationManager oluşturuldu ve prefab atandı.");
            }
            else
            {
                Debug.LogWarning("Prefab bulunamadı. Önce 'Create Achievement Notification Prefab' menüsünü çalıştırın.");
            }
            
            Selection.activeGameObject = managerObj;
        }
    }
}
