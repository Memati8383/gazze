using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class SetupShowcaseCamera
{
    [MenuItem("Tools/Setup Showcase Camera (Fix Car Behind BG)")]
    public static void Setup()
    {
        string layerName = "ShowcaseCar";
        int showcaseLayer = LayerMask.NameToLayer(layerName);
        
        if (showcaseLayer < 0) 
        { 
            Debug.LogError($"Layer '{layerName}' bulunamadi! Lutfen Tags & Layers ayarlarindan ekleyin."); 
            return; 
        }

        // 1) ShowcasePoint ve icindekileri tasi
        GameObject showcasePoint = FindInactive("ShowcasePoint");
        if (showcasePoint == null) 
        { 
            Debug.LogError("ShowcasePoint sahne icinde bulunamadi!"); 
            return; 
        }
        
        SetLayerRecursively(showcasePoint, showcaseLayer);
        Debug.Log($"'{showcasePoint.name}' ve tum alt nesneleri '{layerName}' layer'ina tasindi.");

        // 2) Main Camera Ayari
        Camera mainCam = Camera.main;
        if (mainCam == null) 
        { 
            Debug.LogError("Ana Kamera (Main Camera) bulunamadi!"); 
            return; 
        }
        
        // Ana kameradan bu layeri cikar
        mainCam.cullingMask &= ~(1 << showcaseLayer);
        mainCam.depth = 0;
        Debug.Log("Main Camera'dan ShowcaseCar maskesi cikarildi.");

        // 3) Canvas Ayari
        Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = mainCam;
            canvas.planeDistance = 5f; // Biraz mesafe birakalim
            Debug.Log("Canvas 'Screen Space - Camera' moduna alindi.");
        }

        // 4) Showcase Camera Olustur/Guncelle
        GameObject camObj = GameObject.Find("ShowcaseCamera");
        if (camObj == null)
        {
            camObj = new GameObject("ShowcaseCamera");
        }
        
        camObj.transform.position = mainCam.transform.position;
        camObj.transform.rotation = mainCam.transform.rotation;

        Camera scCam = camObj.GetComponent<Camera>();
        if (scCam == null) scCam = camObj.AddComponent<Camera>();
        
        scCam.clearFlags = CameraClearFlags.Depth;
        scCam.cullingMask = 1 << showcaseLayer;
        scCam.depth = 10; // Canvas'in da ustunde olsun (Canvas planeDistance ile main cam arkasinda olsa da scCam depth ile her seyi ezer)
        scCam.fieldOfView = mainCam.fieldOfView;
        scCam.nearClipPlane = 0.01f;
        scCam.farClipPlane = 1000f;

        // URP Kontrolu
        TrySetupURPStack(mainCam, scCam);

        EditorSceneManager.MarkSceneDirty(showcasePoint.scene);
        Debug.Log("Showcase kurulumu basariyla tamamlandi. Sahneyi kaydedin.");
    }

    private static void TrySetupURPStack(Camera baseCam, Camera overlayCam)
    {
        // URP UniversalAdditionalCameraData tipini bul
        System.Type urpDataType = null;
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            urpDataType = assembly.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData");
            if (urpDataType != null) break;
        }

        if (urpDataType == null)
        {
            Debug.Log("URP tespit edilemedi, Built-in Render Pipeline kullaniliyor. Depth degeri yeterli olacaktir.");
            return;
        }

        // Overseas Camera Data
        Component overlayData = overlayCam.GetComponent(urpDataType);
        if (overlayData == null) overlayData = overlayCam.gameObject.AddComponent(urpDataType);

        // renderType = Overlay (1)
        var renderTypeProp = urpDataType.GetProperty("renderType");
        if (renderTypeProp != null)
        {
            var enumType = renderTypeProp.PropertyType;
            renderTypeProp.SetValue(overlayData, System.Enum.ToObject(enumType, 1));
        }

        // Base Camera Data
        Component baseData = baseCam.GetComponent(urpDataType);
        if (baseData == null) baseData = baseCam.gameObject.AddComponent(urpDataType);

        // cameraStack listesine ekle
        var stackProp = urpDataType.GetProperty("cameraStack");
        if (stackProp != null)
        {
            var stack = stackProp.GetValue(baseData) as List<Camera>;
            if (stack != null)
            {
                if (!stack.Contains(overlayCam))
                {
                    stack.Add(overlayCam);
                    Debug.Log("ShowcaseCamera, Main Camera'nin URP Stack'ine eklendi.");
                }
            }
        }
    }

    private static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private static GameObject FindInactive(string name)
    {
        GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in all)
        {
            if (go.name == name && go.scene.IsValid()) return go;
        }
        return null;
    }
}
