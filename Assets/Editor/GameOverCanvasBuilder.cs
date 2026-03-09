using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.UI;

namespace Gazze.Editor
{
    public static class GameOverCanvasBuilder
    {
        [MenuItem("Tools/Build Game Over Canvas")]
        public static void Build()
        {
            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
                var canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }

            // Yeni Input System uyumlu EventSystem kontrolü
            var esObj = GameObject.Find("EventSystem");
            if (esObj == null)
            {
                esObj = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
            }
            else
            {
                // Mevcut StandaloneInputModule varsa kaldır ve yenisini ekle
                var oldModule = esObj.GetComponent<StandaloneInputModule>();
                if (oldModule != null)
                {
                    Undo.DestroyObjectImmediate(oldModule);
                    Undo.AddComponent<InputSystemUIInputModule>(esObj);
                }
            }

            GameObject panel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            Undo.RegisterCreatedObjectUndo(panel, "Create GameOverPanel");
            panel.transform.SetParent(canvasObj.transform, false);
            var pr = panel.GetComponent<RectTransform>();
            pr.anchorMin = Vector2.zero;
            pr.anchorMax = Vector2.one;
            pr.offsetMin = Vector2.zero;
            pr.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

            GameObject content = new GameObject("Content", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(content, "Create Content");
            content.transform.SetParent(panel.transform, false);
            var cr = content.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(0.5f, 0.5f);
            cr.anchorMax = new Vector2(0.5f, 0.5f);
            cr.sizeDelta = new Vector2(900, 700);
            cr.anchoredPosition = Vector2.zero;

            var title = CreateTMP(content.transform, "OYUN SONU", 72, new Vector2(0, 280));
            title.color = new Color(1f, 0.85f, 0.2f);
            var finalScore = CreateTMP(content.transform, "YARDIM KOLİSİ: 0\nMESAFE: 0m", 40, new Vector2(0, 200));
            var highScore = CreateTMP(content.transform, "EN YÜKSEK SKOR: 0m", 36, new Vector2(0, 140));
            var level = CreateTMP(content.transform, "SEVİYE: 1", 32, new Vector2(0, 90));
            var playTime = CreateTMP(content.transform, "SÜRE: 00:00", 32, new Vector2(0, 45));
            var achievements = CreateTMP(content.transform, "BAŞARIMLAR: YOK", 30, new Vector2(0, 0));
            achievements.rectTransform.sizeDelta = new Vector2(800, 120);

            GameObject buttonsRow = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            Undo.RegisterCreatedObjectUndo(buttonsRow, "Create Buttons");
            buttonsRow.transform.SetParent(content.transform, false);
            var br = buttonsRow.GetComponent<RectTransform>();
            br.anchorMin = new Vector2(0.5f, 0.5f);
            br.anchorMax = new Vector2(0.5f, 0.5f);
            br.sizeDelta = new Vector2(800, 100);
            br.anchoredPosition = new Vector2(0, -190);
            var hlg = buttonsRow.GetComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 20;
            hlg.padding = new RectOffset(10, 10, 10, 10);

            var restart = CreateButton(buttonsRow.transform, "RestartButton", "Yeniden Oyna", new Color(0.2f, 0.8f, 0.2f));
            var menu = CreateButton(buttonsRow.transform, "MenuButton", "Ana Menü", new Color(0.2f, 0.5f, 1f));
            var share = CreateButton(buttonsRow.transform, "ShareButton", "Skoru Paylaş", new Color(1f, 0.6f, 0.2f));
            var settings = CreateButton(buttonsRow.transform, "SettingsButton", "Ayarlar", new Color(0.9f, 0.2f, 0.6f));

            // Not: PlayerController artık dinamik Gazze.UI.GameOverPanelBuilder kullanıyor.
            // Bu araç sadece görsel tasarım/test amaçlı panel oluşturur.
            
            panel.SetActive(true); // Tasarım için görünür yapalım
            Selection.activeGameObject = panel;
        }

        static TextMeshProUGUI CreateTMP(Transform parent, string txt, int size, Vector2 pos)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = txt;
            t.fontSize = size;
            t.fontStyle = FontStyles.Bold;
            t.alignment = TextAlignmentOptions.Center;
            var rt = t.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(800, 60);
            Undo.RegisterCreatedObjectUndo(go, "Create TMP");
            return t;
        }

        static Button CreateButton(Transform parent, string name, string label, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(180, 60);
            var t = CreateTMP(go.transform, label, 28, Vector2.zero);
            t.color = Color.white;
            Undo.RegisterCreatedObjectUndo(go, "Create Button");
            return go.GetComponent<Button>();
        }
    }
}
