using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Gazze.Editor
{
    public class FixBuildSettings
    {
        [MenuItem("Tools/Gazze/Apply Build Fixes")]
        public static void ApplyFixes()
        {
            // 1. Managed Stripping Level to Minimal
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Standalone, ManagedStrippingLevel.Minimal);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Minimal);
            
            // 2. Grafikleri sabitle (Sadece DirectX 11 kullan)
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new UnityEngine.Rendering.GraphicsDeviceType[] { UnityEngine.Rendering.GraphicsDeviceType.Direct3D11 });

            // 3. Crash Report ve Logging aktif edelim, böylece daha sonra logları görebilsin
            PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.Full);
            PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);
            PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
            
            // 4. Ayrıca development build olarak ayarlayalım ki bir sonraki build'de error verirse ekranda gözüksün
            EditorUserBuildSettings.development = true;

            Debug.Log("<color=green>Gazze: Build ayarları (D3D11, Minimal Stripping, Development Build vs.) başarıyla uygulandı.</color>");
        }
    }
}
