using UnityEngine;
using System.IO;

/// <summary>
/// Uygulama acilisinda temel ortam bilgilerini bir dosyaya yazarak build tanisini kolaylastirir.
/// </summary>
public static class BuildDiagnostic
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        // Build klasöründe oyunun .exe dosyasının yanına bir log dosyası oluşturur
        string logPath = Application.platform == RuntimePlatform.Android 
            ? Path.Combine(Application.persistentDataPath, "StartupLog.txt")
            : Path.Combine(Application.dataPath, "../StartupLog.txt");
        try 
        {
            File.WriteAllText(logPath, "Unity Başlatıldı: " + System.DateTime.Now.ToString() + "\n");
            File.AppendAllText(logPath, "Platform: " + Application.platform + "\n");
            File.AppendAllText(logPath, "Graphics API: " + SystemInfo.graphicsDeviceType + "\n");
        }
        catch (System.Exception e) 
        {
            Debug.LogError("Diagnostic Log Error: " + e.Message);
        }
    }
}
