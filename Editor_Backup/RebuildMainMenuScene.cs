using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class RebuildMainMenuScene 
{
    [MenuItem("Tools/Rebuild Main Menu UI")]
    public static void RebuildUI() 
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null) Object.DestroyImmediate(canvasObj);
        GameObject eventObj = GameObject.Find("EventSystem");
        if (eventObj != null) Object.DestroyImmediate(eventObj);

        GameObject showcase = GameObject.Find("ShowcasePoint");
        if (showcase != null) Object.DestroyImmediate(showcase);
        showcase = new GameObject("ShowcasePoint");
        showcase.transform.position = new Vector3(0, -2, 5); // Biraz aşağı çektik ki metinle çakışmasın

        // --- Instantiate cars directly into the scene hierarchy ---
        GameObject[] carPrefabs = Resources.LoadAll<GameObject>("Cars");
        if (carPrefabs != null) {
            for (int i = 0; i < carPrefabs.Length; i++) {
                GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(carPrefabs[i], showcase.transform);
                if (inst != null) {
                    inst.transform.localPosition = Vector3.zero;
                    inst.transform.localRotation = Quaternion.Euler(90, 0, 0); // Dik duruş (Varsayılan)
                }
            }
        }

        canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        eventObj = new GameObject("EventSystem");
        eventObj.AddComponent<EventSystem>();
        var oldInput = eventObj.GetComponent<UnityEngine.EventSystems.BaseInputModule>();
        if (oldInput != null) Object.DestroyImmediate(oldInput);
        eventObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        MainMenuManager manager = canvasObj.AddComponent<MainMenuManager>();
        manager.showcasePoint = showcase.transform;

        // --- Main Options Panel ---
        GameObject mainPanel = new GameObject("MainOptionsPanel");
        mainPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform mpRect = mainPanel.AddComponent<RectTransform>();
        mpRect.anchorMin = Vector2.zero; mpRect.anchorMax = Vector2.one; 
        mpRect.sizeDelta = Vector2.zero; mpRect.anchoredPosition = Vector2.zero;

        // Title
        GameObject mainTitle = CreateText("TitleText", "GAZZE YARDIM HATTI", mainPanel.transform, 50, Color.white);
        RectTransform mtRect = mainTitle.GetComponent<RectTransform>();
        mtRect.anchorMin = new Vector2(0.5f, 0.8f); mtRect.anchorMax = new Vector2(0.5f, 0.8f);
        mtRect.sizeDelta = new Vector2(800, 100);

        // Main Menu Buttons
        GameObject btnStart = CreateButton("Btn_OyunaBasla", "OYUNA BAŞLA", mainPanel.transform, new Vector2(300, 80));
        btnStart.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0); // Center
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnStart.GetComponent<Button>().onClick, manager.ShowCarSelection);

        GameObject btnSettings = CreateButton("Btn_Ayarlar", "AYARLAR", mainPanel.transform, new Vector2(300, 80));
        btnSettings.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSettings.GetComponent<Button>().onClick, manager.OnSettingsClicked);

        GameObject btnExit = CreateButton("Btn_Cikis", "ÇIKIŞ", mainPanel.transform, new Vector2(300, 80));
        btnExit.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnExit.GetComponent<Button>().onClick, manager.OnExitClicked);


        // --- Car Selection Panel ---
        GameObject carPanel = new GameObject("CarSelectionPanel");
        carPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform cpRect = carPanel.AddComponent<RectTransform>();
        cpRect.anchorMin = Vector2.zero; cpRect.anchorMax = Vector2.one; 
        cpRect.sizeDelta = Vector2.zero; cpRect.anchoredPosition = Vector2.zero;
        carPanel.SetActive(false); // Hide by default

        GameObject carTitle = CreateText("CarTitleText", "ARAÇ SEÇİMİ", carPanel.transform, 40, Color.yellow);
        carTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 350);

        GameObject carName = CreateText("CarNameText", "ARAÇ ADI", carPanel.transform, 40, Color.white);
        carName.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -250);
        manager.carNameText = carName.GetComponent<TextMeshProUGUI>();

        GameObject btnPlay = CreateButton("Btn_SurusBasla", "SÜRÜŞE BAŞLA", carPanel.transform, new Vector2(300, 80));
        btnPlay.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -350);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPlay.GetComponent<Button>().onClick, manager.StartGame);

        GameObject btnBack = CreateButton("Btn_Geri", "GERİ", carPanel.transform, new Vector2(150, 60));
        btnBack.GetComponent<RectTransform>().anchoredPosition = new Vector2(-400, 350);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnBack.GetComponent<Button>().onClick, manager.ShowMainOptions);

        GameObject btnPrev = CreateButton("Btn_Prev", "<", carPanel.transform, new Vector2(80, 80));
        RectTransform bpRect = btnPrev.GetComponent<RectTransform>();
        bpRect.anchorMin = new Vector2(0.2f, 0.5f); bpRect.anchorMax = new Vector2(0.2f, 0.5f);
        bpRect.anchoredPosition = new Vector2(0, 0);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPrev.GetComponent<Button>().onClick, manager.PreviousCar);

        GameObject btnNext = CreateButton("Btn_Next", ">", carPanel.transform, new Vector2(80, 80));
        RectTransform bnRect = btnNext.GetComponent<RectTransform>();
        bnRect.anchorMin = new Vector2(0.8f, 0.5f); bnRect.anchorMax = new Vector2(0.8f, 0.5f);
        bnRect.anchoredPosition = new Vector2(0, 0);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnNext.GetComponent<Button>().onClick, manager.NextCar);

        manager.mainOptionsPanel = mainPanel;
        manager.carSelectionPanel = carPanel;

        EditorSceneManager.SaveScene(scene);
        Debug.Log("UI Rebuilt with Main Options and Car Selection Panel!");
    }

    private static GameObject CreateText(string name, string textStr, Transform parent, int fontSize, Color color) {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = textStr;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = fontSize;
        tmp.color = color;
        return obj;
    }

    private static GameObject CreateButton(string name, string textStr, Transform parent, Vector2 size) {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        btnObj.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        Button btn = btnObj.AddComponent<Button>();
        btnObj.GetComponent<RectTransform>().sizeDelta = size;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = textStr;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontSize = 24;
        
        RectTransform txtRect = textObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero; txtRect.anchoredPosition = Vector2.zero;

        return btnObj;
    }
}
