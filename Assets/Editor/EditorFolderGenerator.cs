using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Gazze.Editor.Tools
{
    /// <summary>
    /// Assets/Editor klasörü içinde standart Unity Editor hiyerarşisini ve dosyalarını otomatik oluşturan araç.
    /// </summary>
    public class EditorFolderGenerator : EditorWindow
    {
        private const string RootPath = "Assets/Editor";
        private StringBuilder report = new StringBuilder();

        [MenuItem("Gazze Tools/Generate Editor Structure")]
        public static void ShowWindow()
        {
            GetWindow<EditorFolderGenerator>("Editor Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Unity Editor Klasör ve Dosya Yapılandırıcı", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu araç, Assets/Editor altında eksik olan standart klasörleri ve temel scriptleri otomatik oluşturur.", MessageType.Info);

            if (GUILayout.Button("Yapıyı Oluştur / Güncelle", GUILayout.Height(40)))
            {
                GenerateStructure();
            }
        }

        private void GenerateStructure()
        {
            report.Clear();
            report.AppendLine("<b>--- Editor Yapılandırma Raporu ---</b>");

            // 1. Klasör Hiyerarşisi
            CreateFolder(RootPath);
            CreateFolder($"{RootPath}/Windows");
            CreateFolder($"{RootPath}/Inspectors");
            CreateFolder($"{RootPath}/Utility");
            CreateFolder($"{RootPath}/Resources");

            // 2. Dosya Şablonları
            CreateFile($"{RootPath}/Gazze.Editor.asmdef", GetAsmdefContent());
            CreateFile($"{RootPath}/Windows/CustomEditorWindow.cs", GetEditorWindowContent());
            CreateFile($"{RootPath}/Inspectors/CustomInspector.cs", GetInspectorContent());
            CreateFile($"{RootPath}/Utility/EditorConstants.cs", GetConstantsContent());

            report.AppendLine("\n<b>İşlem Tamamlandı.</b>");
            // Debug.Log(report.ToString());
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Gazze Editor Generator", "Editor hiyerarşisi başarıyla oluşturuldu/güncellendi. Detaylar için konsola bakın.", "Tamam");
        }

        private void CreateFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string folderName = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folderName);
                report.AppendLine($"<color=green>[KLASÖR]</color> Oluşturuldu: {path}");
            }
            else
            {
                report.AppendLine($"<color=grey>[KLASÖR]</color> Mevcut: {path}");
            }
        }

        private void CreateFile(string path, string content)
        {
            string fullPath = Path.Combine(Application.dataPath.Replace("Assets", ""), path);
            if (!File.Exists(fullPath))
            {
                File.WriteAllText(fullPath, content);
                report.AppendLine($"<color=blue>[DOSYA]</color> Oluşturuldu: {path}");
            }
            else
            {
                report.AppendLine($"<color=grey>[DOSYA]</color> Mevcut: {path}");
            }
        }

        #region Dosya İçerik Şablonları

        private string GetAsmdefContent()
        {
            return @"{
    ""name"": ""Gazze.Editor"",
    ""rootNamespace"": ""Gazze.Editor"",
    ""references"": [
        ""GUID:2bafac87e7f4b9b418d9448d219b01ab"",
        ""GUID:6055be8ebefd69e48b49212b09b47b2f""
    ],
    ""includePlatforms"": [
        ""Editor""
    ],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";
        }

        private string GetEditorWindowContent()
        {
            return @"using UnityEngine;
using UnityEditor;

namespace Gazze.Editor.Windows
{
    public class CustomEditorWindow : EditorWindow
    {
        [MenuItem(""Gazze Tools/Custom Window"")]
        public static void ShowWindow()
        {
            GetWindow<CustomEditorWindow>(""Custom Window"");
        }

        private void OnGUI()
        {
            GUILayout.Label(""Custom Editor Window"", EditorStyles.boldLabel);
            if (GUILayout.Button(""İşlem Yap""))
            {
                // Debug.Log(""Butona basıldı!"");
            }
        }
    }
}";
        }

        private string GetInspectorContent()
        {
            return @"using UnityEngine;
using UnityEditor;

namespace Gazze.Editor.Inspectors
{
    // [CustomEditor(typeof(HedefClass))]
    public class CustomInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.HelpBox(""Bu özel bir inspector görünümüdür."", MessageType.Info);
        }
    }
}";
        }

        private string GetConstantsContent()
        {
            return @"namespace Gazze.Editor.Utility
{
    public static class EditorConstants
    {
        public const string ToolMenuPath = ""Gazze Tools/"";
        public const string Version = ""1.0.0"";
    }
}";
        }

        #endregion
    }
}
