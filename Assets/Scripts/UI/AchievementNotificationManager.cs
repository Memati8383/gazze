using UnityEngine;
using System.Collections.Generic;

namespace Gazze.UI
{
    /// <summary>
    /// Başarım bildirimlerini yöneten singleton manager
    /// </summary>
    public class AchievementNotificationManager : MonoBehaviour
    {
        private static AchievementNotificationManager instance;
        public static AchievementNotificationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<AchievementNotificationManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("AchievementNotificationManager");
                        instance = go.AddComponent<AchievementNotificationManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        [Header("Prefab")]
        [SerializeField] private GameObject notificationPrefab;
        
        [Header("Settings")]
        [SerializeField] private int maxSimultaneousNotifications = 3;
        [SerializeField] private float verticalSpacing = 75f;
        [SerializeField] private float topMargin = 180f; // 75 -> 180 (Daha aşağı alındı)
        
        private Canvas canvas;
        private Queue<NotificationData> notificationQueue = new Queue<NotificationData>();
        private List<AchievementNotification> activeNotifications = new List<AchievementNotification>();
        
        private struct NotificationData
        {
            public string title;
            public string description;
            public Sprite icon;
        }
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // KOD İLE ZORLAYALIM (1080p Referans İçin)
            topMargin = 220f; 
            verticalSpacing = 112f; // 118 yetmedi, 110 değiyordu. 112-113 minimal sınır.
            
            SetupCanvas();
        }
        
        private void SetupCanvas()
        {
            canvas = GetComponentInChildren<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("NotificationCanvas");
                canvasObj.transform.SetParent(transform);
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1000;
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // Mevcut veya yeni canvas'ın scaler'ını zorla güncelle
            var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); // HD Referans
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1.0f; // Yüksekliğe sabitle (Mobilde en tutarlı yöntem)
        }
        
        /// <summary>
        /// Başarım bildirimi gösterir
        /// </summary>
        public void ShowAchievement(string achievementName, string description = "", Sprite icon = null)
        {
            string defaultDesc = LocalizationManager.Instance != null 
                ? LocalizationManager.Instance.GetTranslation("Achievement_Unlocked") 
                : "Başarım Kazanıldı!";
                
            NotificationData data = new NotificationData
            {
                title = achievementName,
                description = string.IsNullOrEmpty(description) ? defaultDesc : description,
                icon = icon
            };
            
            notificationQueue.Enqueue(data);
            ProcessQueue();
        }
        
        private void ProcessQueue()
        {
            if (notificationQueue.Count == 0) return;
            if (activeNotifications.Count >= maxSimultaneousNotifications) return;
            
            NotificationData data = notificationQueue.Dequeue();
            ShowNotificationInternal(data);
        }
        
        private void ShowNotificationInternal(NotificationData data)
        {
            GameObject notifObj = null;
            
            // KESİNLİKLE YENİ 'Resources' PREFAB'INI KULLAN (Eski referansı ezer)
            GameObject loadedPrefab = Resources.Load<GameObject>("AchievementNotification");
            if (loadedPrefab != null)
            {
                notifObj = Instantiate(loadedPrefab, canvas.transform);
            }
            else if (notificationPrefab != null)
            {
                notifObj = Instantiate(notificationPrefab, canvas.transform);
            }
            else
            {
                notifObj = CreateNotificationUI();
            }
            
            AchievementNotification notification = notifObj.GetComponent<AchievementNotification>();
            if (notification == null)
            {
                notification = notifObj.AddComponent<AchievementNotification>();
            }
            
            // Pozisyonu ayarla
            RectTransform rt = notifObj.GetComponent<RectTransform>();
            
            // ANCHOR VE PIVOT'I ZORLA: Mobilde tutarlılık için her zaman Sağ-Üst
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            
            // BOYUT VE YERLEŞİM AYARI (1080p Referans + %110 Ölçek)
            rt.localScale = Vector3.one * 1.1f; 
            
            float yPos = -topMargin - (activeNotifications.Count * verticalSpacing);
            rt.anchoredPosition = new Vector2(400f, yPos); // offscreen
            
            activeNotifications.Add(notification);
            notification.Show(data.title, data.description, data.icon);
            
            StartCoroutine(RemoveAfterDelay(notification, 4f));
        }
        
        private System.Collections.IEnumerator RemoveAfterDelay(AchievementNotification notification, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            
            activeNotifications.Remove(notification);
            if (notification != null && notification.gameObject != null)
            {
                Destroy(notification.gameObject);
            }
            
            // Silinenin yerini doldurmak için diğerlerini yukarı kaydır
            RepositionNotifications();
            
            ProcessQueue();
        }

        private void RepositionNotifications()
        {
            for (int i = 0; i < activeNotifications.Count; i++)
            {
                if (activeNotifications[i] == null) continue;
                RectTransform rt = activeNotifications[i].GetComponent<RectTransform>();
                float targetY = -topMargin - (i * verticalSpacing);
                
                // Yukarı doğru akıcı şekilde kaymasına olanak tanır
                StartCoroutine(SmoothMove(rt, targetY));
            }
        }

        private System.Collections.IEnumerator SmoothMove(RectTransform rt, float targetY)
        {
            if (rt == null) yield break;
            
            float startY = rt.anchoredPosition.y;
            float elapsed = 0f;
            float duration = 0.3f; // 0.3 saniyede yukarı kayar
            
            while (elapsed < duration && rt != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, Mathf.Lerp(startY, targetY, t));
                yield return null;
            }
            
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, targetY);
            }
        }
        
        private GameObject CreateNotificationUI()
        {
            Debug.LogWarning("Prefab bulunamadı, fallback UI üretiliyor..");
            GameObject notifObj = new GameObject("AchievementNotification", typeof(RectTransform));
            notifObj.transform.SetParent(canvas.transform, false);
            return notifObj;
        }
    }
}
