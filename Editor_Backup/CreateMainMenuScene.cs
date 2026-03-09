using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CreateMainMenuScene 
{
    [MenuItem("Tools/Create Main Menu Scene")]
    public static void CreateScene() 
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 1. Showcase Point
        GameObject showcase = new GameObject("ShowcasePoint");
        showcase.transform.position = new Vector3(0, 0, 5); // Kameranın önüne
        
        // 2. Işık
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        // 3. UI
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        // Manager
        MainMenuManager manager = canvasObj.AddComponent<MainMenuManager>();
        manager.showcasePoint = showcase.transform;

        // Title
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "GAZZE YARDIM HATTI\nARACINI SEÇ";
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 50;
        titleText.color = Color.white;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -50);
        titleRect.sizeDelta = new Vector2(800, 200);

        // Car Name Text
        GameObject carNameObj = new GameObject("CarNameText");
        carNameObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI nameText = carNameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "ARAÇ ADI";
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.fontSize = 40;
        nameText.color = Color.yellow;
        RectTransform nameRect = carNameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 0.2f);
        nameRect.anchorMax = new Vector2(0.5f, 0.2f);
        nameRect.pivot = new Vector2(0.5f, 0.5f);
        nameRect.anchoredPosition = new Vector2(0, 0);
        nameRect.sizeDelta = new Vector2(600, 100);
        manager.carNameText = nameText;

        // Start Button
        GameObject startBtnObj = CreateButton("StartButton", "OYUNA BAŞLA", canvasObj.transform);
        RectTransform startRect = startBtnObj.GetComponent<RectTransform>();
        startRect.anchorMin = new Vector2(0.5f, 0.1f);
        startRect.anchorMax = new Vector2(0.5f, 0.1f);
        startRect.pivot = new Vector2(0.5f, 0.5f);
        startRect.anchoredPosition = new Vector2(0, 0);
        startRect.sizeDelta = new Vector2(300, 80);
        Button startBtn = startBtnObj.GetComponent<Button>();
        startBtn.onClick.AddListener(manager.StartGame);

        // Next Button
        GameObject nextBtnObj = CreateButton("NextButton", ">", canvasObj.transform);
        RectTransform nextRect = nextBtnObj.GetComponent<RectTransform>();
        nextRect.anchorMin = new Vector2(0.8f, 0.5f);
        nextRect.anchorMax = new Vector2(0.8f, 0.5f);
        nextRect.pivot = new Vector2(0.5f, 0.5f);
        nextRect.anchoredPosition = new Vector2(0, 0);
        nextRect.sizeDelta = new Vector2(100, 100);
        Button nextBtn = nextBtnObj.GetComponent<Button>();
        nextBtn.onClick.AddListener(manager.NextCar);

        // Prev Button
        GameObject prevBtnObj = CreateButton("PrevButton", "<", canvasObj.transform);
        RectTransform prevRect = prevBtnObj.GetComponent<RectTransform>();
        prevRect.anchorMin = new Vector2(0.2f, 0.5f);
        prevRect.anchorMax = new Vector2(0.2f, 0.5f);
        prevRect.pivot = new Vector2(0.5f, 0.5f);
        prevRect.anchoredPosition = new Vector2(0, 0);
        prevRect.sizeDelta = new Vector2(100, 100);
        Button prevBtn = prevBtnObj.GetComponent<Button>();
        prevBtn.onClick.AddListener(manager.PreviousCar);

        // Kamera ayarı (Arka plan rengi belirgin olsun)
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = new Color(0.1f, 0.1f, 0.2f);

        // Sahneyi kaydet
        string scenePath = "Assets/Scenes/MainMenu.unity";
        EditorSceneManager.SaveScene(scene, scenePath);

        // Build Settings e ekle (en başa)
        var original = EditorBuildSettings.scenes;
        var newScenes = new EditorBuildSettingsScene[original.Length + 1];
        newScenes[0] = new EditorBuildSettingsScene(scenePath, true);
        for (int i = 0; i < original.Length; i++) 
        {
            if (original[i].path == scenePath) continue; // Prevent duplication
            newScenes[i+1] = original[i];
        }
        EditorBuildSettings.scenes = newScenes;

        Debug.Log("MainMenu scene created and saved!");
    }

    private static GameObject CreateButton(string name, string textStr, Transform parent) 
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        btnObj.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        btnObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = textStr;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontSize = 30;
        
        RectTransform txtRect = textObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;

        return btnObj;
    }
}