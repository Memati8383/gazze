using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Gazze.UI;

public class AchievementUIBuilder : EditorWindow
{
    [MenuItem("Gazze/UI/Build Achievement Notification Prefab")]
    public static void BuildPrefab()
    {
        // 1. Ana Obje - Sharp HUD Boyutları
        GameObject notifObj = new GameObject("AchievementNotification");
        RectTransform rt = notifObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300f, 65f); // Genişlik artırıldı, yükseklik ideal
        rt.anchorMin = new Vector2(1f, 1f); // Top Right Anchor
        rt.anchorMax = new Vector2(1f, 1f); // Top Right Anchor
        rt.pivot = new Vector2(1f, 1f);     // Top Right Pivot
        
        CanvasGroup cg = notifObj.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        AchievementNotification notifComp = notifObj.AddComponent<AchievementNotification>();
        notifObj.AddComponent<AchievementNotificationEffects>(); // Sabit efektler

        // 2. Arka Plan Siyah Kutu (Tamamen DÜZ / Şeffaf Siyah)
        GameObject bgObj = new GameObject("Background");
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.SetParent(rt, false);
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.sprite = null; // KÖŞESİZ, KESKİN HUD GÖRÜNÜMÜ !
        bgImg.color = new Color(0.05f, 0.05f, 0.05f, 0.95f); // Koyu Antrasit

        // 3. Vurgu Çizgisi (Accent - Sol Köşe)
        GameObject accentObj = new GameObject("Accent");
        RectTransform accentRt = accentObj.AddComponent<RectTransform>();
        accentRt.SetParent(bgRt, false);
        accentRt.anchorMin = new Vector2(0, 0); 
        accentRt.anchorMax = new Vector2(0, 1); 
        accentRt.pivot = new Vector2(0, 0.5f);
        accentRt.anchoredPosition = Vector2.zero;
        accentRt.sizeDelta = new Vector2(4, 0); // 4px HUD-style çizgi
        Image accentImg = accentObj.AddComponent<Image>();
        accentImg.sprite = null;
        accentImg.color = Color.white;

        // Glow Objesi BİLEREK YARATILMADI! Screenshot'taki iğrenç ovalleşen Glow bu yüzden kaldırıldı.

        // 4. İkon Alanı
        GameObject iconImgObj = new GameObject("Icon");
        RectTransform iconImgRt = iconImgObj.AddComponent<RectTransform>();
        iconImgRt.SetParent(rt, false);
        iconImgRt.anchorMin = new Vector2(0, 0.5f);
        iconImgRt.anchorMax = new Vector2(0, 0.5f);
        iconImgRt.pivot = new Vector2(0.5f, 0.5f);
        iconImgRt.sizeDelta = new Vector2(35, 35);
        iconImgRt.anchoredPosition = new Vector2(30, 0); // Soldan 30px
        Image iconMainImg = iconImgObj.AddComponent<Image>();
        iconMainImg.sprite = null; 
        iconMainImg.color = new Color(1, 1, 1, 0); // AchievementNotification arka planı sıfırlayıp yazı ekleyecek

        // 5. BAŞLIK 
        GameObject titleObj = new GameObject("Title");
        RectTransform titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.SetParent(rt, false);
        titleRt.anchorMin = new Vector2(0, 1); // Sol Üst Anchor
        titleRt.anchorMax = new Vector2(0, 1); 
        titleRt.pivot = new Vector2(0, 1); // Sol Üst Pivot (KUSURSUZ KONUMLANDIRMA İÇİN)
        titleRt.sizeDelta = new Vector2(230, 25);
        titleRt.anchoredPosition = new Vector2(65, -10); // X: 65, Y: -10 aşağıda
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "GÖREV BAŞARISI"; 
        titleText.fontSize = 15;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.MidlineLeft; // Aşağıya kaymayı engeller, dikey ortalar
        titleText.color = new Color(1f, 1f, 1f, 1f);
        titleText.characterSpacing = 1.5f; // HUD stili biraz harf aralığı açıldı

        // 6. AÇIKLAMA (Description)
        GameObject descObj = new GameObject("Description");
        RectTransform descRt = descObj.AddComponent<RectTransform>();
        descRt.SetParent(rt, false);
        descRt.anchorMin = new Vector2(0, 1);
        descRt.anchorMax = new Vector2(0, 1);
        descRt.pivot = new Vector2(0, 1);
        descRt.sizeDelta = new Vector2(230, 25);
        descRt.anchoredPosition = new Vector2(65, -35); // Title'ın hemen altında
        
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = "1000 Puan Kazanıldı.";
        descText.fontSize = 12;
        descText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        descText.alignment = TextAlignmentOptions.TopLeft; // Üstten sola hizalı

        // 7. Referans Atamaları
        notifComp.titleText = titleText;
        notifComp.descriptionText = descText;
        notifComp.iconImage = iconMainImg;
        notifComp.backgroundImage = bgImg; // Opsiyonel: Temadaki renkleri istiyorsa bu BG'ye uygulanacak

        // 8. Kayıt İşlemi
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        string localPath = "Assets/Resources/AchievementNotification.prefab";
        
        bool prefabSuccess;
        PrefabUtility.SaveAsPrefabAssetAndConnect(notifObj, localPath, InteractionMode.UserAction, out prefabSuccess);

        if (prefabSuccess)
        {
            Debug.Log("HUD Stili Başarım Prefab'ı başarıyla üretildi: " + localPath);
            GameObject.DestroyImmediate(notifObj);
        }
    }
}
