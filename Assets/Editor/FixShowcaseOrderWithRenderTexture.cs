using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class FixShowcaseOrderWithRenderTexture
{
    [MenuItem("Tools/Fix Car UI Order (Use RenderTexture)")]
    public static void Fix()
    {
        string layerName = "ShowcaseCar";
        int showcaseLayer = LayerMask.NameToLayer(layerName);
        if (showcaseLayer < 0) { Debug.LogError("ShowcaseCar layeri eksik!"); return; }

        // 1) Kamerayı Bul ve Yapılandır
        GameObject camObj = GameObject.Find("ShowcaseCamera");
        if (camObj == null) { Debug.LogError("ShowcaseCamera bulunamadi!"); return; }
        Camera scCam = camObj.GetComponent<Camera>();

        // 2) RenderTexture Oluştur (Eğer yoksa)
        string rtPath = "Assets/UI_CarRenderTexture.renderTexture";
        RenderTexture rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
        if (rt == null)
        {
            rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            AssetDatabase.CreateAsset(rt, rtPath);
            Debug.Log("RenderTexture olusturuldu: " + rtPath);
        }

        // Kamera ayarlarını RenderTexture için güncelle
        scCam.targetTexture = rt;
        scCam.clearFlags = CameraClearFlags.SolidColor;
        scCam.backgroundColor = new Color(0, 0, 0, 0); // Şeffaf arka plan
        scCam.depth = -1; // Ekran siralama derinligini dusur (cunku dokuya yaziyor)
        
        // URP Stack'ten çıkar (çünkü artık bağımsız render ediyor)
        TryRemoveFromURPStack(Camera.main, scCam);

        // 3) UI içine RawImage (Araç Penceresi) ekle
        GameObject carPanel = FindInactive("CarSelectionPanel");
        if (carPanel == null) { Debug.LogError("CarSelectionPanel bulunamadi!"); return; }

        GameObject rawImgObj = GameObject.Find("CarRenderDisplay");
        if (rawImgObj == null)
        {
            rawImgObj = new GameObject("CarRenderDisplay");
            rawImgObj.transform.SetParent(carPanel.transform, false);
        }

        // Hiyerarşi Sırası: Background'dan hemen sonra (index 1)
        rawImgObj.transform.SetSiblingIndex(1);

        RawImage rawImg = rawImgObj.GetComponent<RawImage>() ?? rawImgObj.AddComponent<RawImage>();
        rawImg.texture = rt;
        rawImg.color = Color.white;
        rawImg.raycastTarget = false;

        // Boyutlandırma (Ortaya yay)
        RectTransform rect = rawImg.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.1f);
        rect.anchorMax = new Vector2(0.9f, 0.9f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Debug.Log("Araç artik 'CarRenderDisplay' adli RawImage icinde gösteriliyor. Hiyerarsi sirasi düzeltildi.");
        EditorSceneManager.MarkSceneDirty(carPanel.scene);
    }

    private static void TryRemoveFromURPStack(Camera baseCam, Camera overlayCam)
    {
        System.Type urpDataType = null;
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            urpDataType = assembly.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData");
            if (urpDataType != null) break;
        }

        if (urpDataType == null || baseCam == null) return;

        Component baseData = baseCam.GetComponent(urpDataType);
        if (baseData == null) return;

        var stackProp = urpDataType.GetProperty("cameraStack");
        if (stackProp != null)
        {
            var stack = stackProp.GetValue(baseData) as List<Camera>;
            if (stack != null && stack.Contains(overlayCam))
            {
                stack.Remove(overlayCam);
                Debug.Log("ShowcaseCamera URP Camera Stack'ten cikarildi (RT kullaniliyor).");
            }
        }
        
        // Overlay cam'i Base'e geri çek (çünkü RT'ye render ediyor)
        Component overlayData = overlayCam.GetComponent(urpDataType);
        if (overlayData != null)
        {
            var renderTypeProp = urpDataType.GetProperty("renderType");
            if (renderTypeProp != null)
                renderTypeProp.SetValue(overlayData, System.Enum.ToObject(renderTypeProp.PropertyType, 0)); // Base=0
        }
    }

    private static GameObject FindInactive(string name)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.name == name && go.scene.IsValid()) return go;
        return null;
    }
}
