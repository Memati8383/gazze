using System.Collections;
using System.IO;
using UnityEngine;

namespace Gazze.UI
{
    /// <summary>
    /// Ekran görüntüsü alır, galeriye kaydeder ve native paylaşım menüsünü açar.
    /// Android ve iOS destekli.
    /// </summary>
    public class ScreenshotShareManager : MonoBehaviour
    {
        public static ScreenshotShareManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
        /// Ekran görüntüsü al → galeriye kaydet → native paylaşım menüsünü aç.
        /// sharePanel: Paylaşım sırasında gizlenecek panel (null olabilir).
        /// </summary>
        public void CaptureAndShare(GameObject sharePanel = null, string gameTitle = "Gazze")
        {
            StartCoroutine(CaptureAndShareRoutine(sharePanel, gameTitle));
        }

        IEnumerator CaptureAndShareRoutine(GameObject sharePanel, string gameTitle)
        {
            // Time.timeScale = 0 olduğunda WaitForEndOfFrame çalışmaz
            // Geçici olarak timeScale'i restore et
            float prevTimeScale = Time.timeScale;
            if (Time.timeScale < 0.001f) Time.timeScale = 0.001f;

            // Panel GÖRÜNÜR kalır — kullanıcı skor ekranını paylaşmak istiyor
            yield return new WaitForEndOfFrame();

            // Ekran görüntüsünü al (skor paneli dahil)
            Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();

            // TimeScale'i geri al
            Time.timeScale = prevTimeScale;

            // Dosya yolu oluştur
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"Gazze_Score_{timestamp}.png";
            string filePath = Path.Combine(Application.temporaryCachePath, fileName);

            // PNG olarak kaydet
            byte[] pngBytes = screenshot.EncodeToPNG();
            Object.Destroy(screenshot);

            File.WriteAllBytes(filePath, pngBytes);
            Debug.Log($"<color=cyan>Gazze:</color> Ekran görüntüsü kaydedildi: {filePath}");

            // Galeriye kaydet
            SaveToGallery(filePath, fileName);

            // Native paylaşım menüsünü aç
            NativeShare(filePath, gameTitle);
        }

        void SaveToGallery(string filePath, string fileName)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                // Android MediaStore ile galeriye kaydet
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var mediaScanner = new AndroidJavaClass("android.media.MediaScannerConnection"))
                {
                    // Dosyayı Pictures klasörüne kopyala
                    string destDir = GetAndroidPicturesPath();
                    if (!string.IsNullOrEmpty(destDir))
                    {
                        string destPath = Path.Combine(destDir, fileName);
                        File.Copy(filePath, destPath, true);
                        
                        // MediaScanner ile galeriye bildirimi yap
                        mediaScanner.CallStatic("scanFile", activity, new string[] { destPath }, null, null);
                        Debug.Log($"<color=cyan>Gazze:</color> Galeriye kaydedildi: {destPath}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Galeriye kaydetme hatası: {e.Message}");
            }
#elif UNITY_IOS && !UNITY_EDITOR
            // iOS NativeGallery API'si (UnityEngine.iOS)
            // Not: iOS'ta galeriye kaydetmek için harici bir plugin gerekebilir.
            // Sıfır bağımlılık için paylaşım menüsü kullanıyoruz.
            Debug.Log("<color=cyan>Gazze:</color> iOS: Paylaşım menüsü üzerinden galeriye kaydedilebilir.");
#else
            Debug.Log($"<color=cyan>Gazze:</color> Editor: Galeriye kaydetme atlandı. Dosya: {filePath}");
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        string GetAndroidPicturesPath()
        {
            try
            {
                using (var env = new AndroidJavaClass("android.os.Environment"))
                {
                    string pictures = env.GetStatic<AndroidJavaObject>("DIRECTORY_PICTURES").Call<string>("toString");
                    using (var extDir = env.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", pictures))
                    {
                        string path = extDir.Call<string>("getAbsolutePath");
                        string gazzeDir = Path.Combine(path, "Gazze");
                        if (!Directory.Exists(gazzeDir)) Directory.CreateDirectory(gazzeDir);
                        return gazzeDir;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Android Pictures yolu alınamadı: {e.Message}");
                return null;
            }
        }
#endif

        void NativeShare(string filePath, string gameTitle)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var intent = new AndroidJavaObject("android.content.Intent"))
                {
                    intent.Call<AndroidJavaObject>("setAction", "android.intent.action.SEND");
                    intent.Call<AndroidJavaObject>("setType", "image/png");

                    // FileProvider ile güvenli URI oluştur
                    string authority = Application.identifier + ".fileprovider";
                    AndroidJavaObject fileObj = null;
                    AndroidJavaObject uri = null;

                    try
                    {
                        using (var fileClass = new AndroidJavaClass("java.io.File"))
                        {
                            fileObj = new AndroidJavaObject("java.io.File", filePath);
                        }

                        using (var fileProvider = new AndroidJavaClass("androidx.core.content.FileProvider"))
                        {
                            uri = fileProvider.CallStatic<AndroidJavaObject>("getUriForFile", activity, authority, fileObj);
                        }
                    }
                    catch
                    {
                        // FileProvider yoksa doğrudan URI kullan (API 24 altı)
                        using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                        {
                            if (fileObj == null) fileObj = new AndroidJavaObject("java.io.File", filePath);
                            uri = uriClass.CallStatic<AndroidJavaObject>("fromFile", fileObj);
                        }
                    }

                    intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.STREAM", uri);
                    intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.TEXT", 
                        $"{gameTitle} oyunundaki skorumu gör!");
                    intent.Call<AndroidJavaObject>("addFlags", 1); // FLAG_GRANT_READ_URI_PERMISSION

                    // Chooser oluştur
                    string shareTitle = LocalizationManager.Instance != null 
                        ? LocalizationManager.Instance.GetTranslation("Game_Share") 
                        : "Paylas";
                    using (var chooser = intent.CallStatic<AndroidJavaObject>("createChooser", intent, shareTitle))
                    {
                        activity.Call("startActivity", chooser);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Android paylaşım hatası: {e.Message}");
                // Fallback: URL paylaşımı
                FallbackShare();
            }
#elif UNITY_IOS && !UNITY_EDITOR
            // iOS native paylaşım (UIActivityViewController)
            // Not: iOS'ta SocialNativeShare veya harici plugin olmadan, 
            // en basit yol URL scheme kullanmaktır.
            FallbackShare();
#else
            FallbackShare();
#endif
        }

        void FallbackShare()
        {
            if (PlayerController.Instance != null)
            {
                string text = $"Gazze oyununda skorumu gör!";
                GUIUtility.systemCopyBuffer = text;
                Debug.Log($"<color=cyan>Gazze:</color> Skor panoya kopyalandı: {text}");
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
