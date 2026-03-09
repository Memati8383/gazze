using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Proje genelinde sahneler arası geçişi ve uzun süren arka plan işlemlerini
/// yöneten, modern ve kullanıcı dostu yükleme ekranı yöneticisidir.
/// </summary>
public class LoadingManager : MonoBehaviour
{
    private static LoadingManager _instance;
    public static LoadingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Önce sahnedeki (belki kullanıcının düzenlediği ve kapalı olan) objeyi bulmaya çalışalım
                _instance = Object.FindFirstObjectByType<LoadingManager>(FindObjectsInactive.Include);
                
                if (_instance == null)
                {
                    // Sahne de hiç yoksa yeni bir tane oluştur (Failsafe)
                    GameObject go = new GameObject("LoadingManager");
                    _instance = go.AddComponent<LoadingManager>();
                    DontDestroyOnLoad(go);
                }
                else
                {
                    // Sahnede bulunduysa (kapalı olsa bile) onun kalıcı olduğundan emin ol
                    if (_instance.transform.parent != null)
                    {
                        DontDestroyOnLoad(_instance.transform.root.gameObject);
                    }
                    else
                    {
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                }
            }
            return _instance;
        }
    }

    [Header("UI Elemanları")]
    public GameObject loadingCanvas;
    public CanvasGroup loadingCanvasGroup;
    public Slider progressBar;
    public Image progressBarFill;
    public TextMeshProUGUI percentageText;
    public TextMeshProUGUI statusText;
    public Image backgroundImage;
    public Image spinner;
    public Image logo;
    public Image barGlow;
    public RectTransform bottomContainer;
    public Button cancelButton;

    [Header("Ayarlar")]
    public bool createUIOnStartIfMissing = true;
    public float minDisplayTime = 1.0f; // Tasarımın keyfini çıkarmak için ideal süre
    public float spinnerRotationSpeed = 360f;
    public float fadeDuration = 0.4f;
    public float progressBarSmoothTime = 0.15f;
    public float floatingAmplitude = 8f;
    public float floatingSpeed = 1.5f;
    
    [Header("Arkaplan Animasyon Ayarları")]
    public bool animateBackground = true;
    public float bgPulseSpeed = 0.8f;
    public float bgPulseAmplitude = 0.03f;
    public float bgCycleSpeed = 0.3f;

    public Color accentColor = new Color(0f, 0.8f, 1f); // Cyan/Electric Blue (Bespoke Accent)

    private AsyncOperation _asyncOperation;
    private Coroutine _activeLoadCoroutine;
    private bool _isCanceled = false;
    private float _targetProgress = 0f;
    private float _currentProgressVelocity = 0f;
    private float _displayProgress = 0f;
    private string _currentBaseMessage = "";
    private float _dotsTimer = 0f;
    private int _dotsCount = 0;
    private Vector2 _originalContainerPos;

    private void Awake()
    {
        // Singleton Çakışma Kontrolü
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;

        // Parent kontrolü ile DontDestroyOnLoad
        if (transform.parent != null)
        {
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }

        // Eksik UI varsa oluştur
        if (loadingCanvas == null && createUIOnStartIfMissing)
        {
            CreateDefaultUI();
        }

        // Başlangıç durumu (Gizli)
        InitializeUIState();

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CancelLoading);
        }
    }

    private void InitializeUIState()
    {
        if (loadingCanvas != null)
        {
            loadingCanvas.SetActive(false);
            if (loadingCanvasGroup != null) loadingCanvasGroup.alpha = 0;
            if (bottomContainer != null) _originalContainerPos = bottomContainer.anchoredPosition;
            
            // FIX: Force runtime anchors for background to always cover screen
            if (backgroundImage != null)
            {
                RectTransform bgRt = backgroundImage.GetComponent<RectTransform>();
                if (bgRt != null)
                {
                    bgRt.anchorMin = Vector2.zero;
                    bgRt.anchorMax = Vector2.one;
                    bgRt.sizeDelta = Vector2.zero;
                    bgRt.anchoredPosition = Vector2.zero;
                }
            }
        }
    }

    /// <summary>
    /// Hierarchy panelinde düzenlenebilir bir UI yapısı oluşturur.
    /// Koddan UI oluşturma işlemi sadece fallback durumları içindir.
    /// </summary>
    [ContextMenu("Generate UI Hierarchy")]
    [ContextMenu("Generate UI Hierarchy")]
    public void CreateDefaultUI()
    {
        if (loadingCanvas != null) return;

        // Root Canvas
        GameObject canvasGO = new GameObject("LoadingCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        canvasGO.transform.SetParent(this.transform);
        loadingCanvas = canvasGO;
        loadingCanvasGroup = canvasGO.GetComponent<CanvasGroup>();
        
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; 
        
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Arka Plan
        GameObject bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(canvasGO.transform, false);
        backgroundImage = bgGO.GetComponent<Image>();
        RectTransform bgRt = bgGO.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero; // FIX: Tam ekran için sıfır
        bgRt.anchoredPosition = Vector2.zero;
        backgroundImage.color = new Color(0.02f, 0.02f, 0.02f, 1f); // Deep Minimalist Black
        
        // Logo
        GameObject logoGO = new GameObject("AppLogo", typeof(RectTransform), typeof(Image));
        logoGO.transform.SetParent(canvasGO.transform, false);
        logo = logoGO.GetComponent<Image>();
        RectTransform logoRt = logoGO.GetComponent<RectTransform>();
        logoRt.anchorMin = new Vector2(0.5f, 0.60f);
        logoRt.anchorMax = new Vector2(0.5f, 0.60f);
        logoRt.sizeDelta = new Vector2(120, 120);
        logoRt.anchoredPosition = Vector2.zero;
        logo.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        logo.color = new Color(1f, 1f, 1f, 0.9f);

        // Alt Konteynır
        GameObject containerGO = new GameObject("BottomContainer", typeof(RectTransform));
        containerGO.transform.SetParent(canvasGO.transform, false);
        bottomContainer = containerGO.GetComponent<RectTransform>();
        bottomContainer.anchorMin = new Vector2(0.5f, 0.15f);
        bottomContainer.anchorMax = new Vector2(0.5f, 0.15f);
        bottomContainer.sizeDelta = new Vector2(1000, 150);
        bottomContainer.anchoredPosition = Vector2.zero;
        _originalContainerPos = bottomContainer.anchoredPosition;

        // Bar
        GameObject pbGO = new GameObject("ProgressBar", typeof(RectTransform), typeof(Slider));
        pbGO.transform.SetParent(bottomContainer.transform, false);
        progressBar = pbGO.GetComponent<Slider>();
        progressBar.transition = Selectable.Transition.None;
        progressBar.interactable = false;
        RectTransform pbRt = pbGO.GetComponent<RectTransform>();
        pbRt.anchorMin = new Vector2(0.5f, 0.5f);
        pbRt.anchorMax = new Vector2(0.5f, 0.5f);
        pbRt.sizeDelta = new Vector2(600, 2); // Avant-garde thin bar
        pbRt.anchoredPosition = Vector2.zero;

        GameObject pbBgGO = new GameObject("BarBg", typeof(RectTransform), typeof(Image));
        pbBgGO.transform.SetParent(pbGO.transform, false);
        RectTransform pbBgRt = pbBgGO.GetComponent<RectTransform>();
        pbBgRt.anchorMin = Vector2.zero;
        pbBgRt.anchorMax = Vector2.one;
        pbBgRt.sizeDelta = Vector2.zero;
        pbBgGO.GetComponent<Image>().color = new Color(1, 1, 1, 0.05f);

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(pbGO.transform, false);
        fillArea.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        fillArea.GetComponent<RectTransform>().anchorMax = Vector2.one;
        fillArea.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        
        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        progressBar.fillRect = fill.GetComponent<RectTransform>();
        progressBarFill = fill.GetComponent<Image>();
        progressBarFill.color = new Color(1f, 1f, 1f, 1f); // Pure white fill

        // Glow
        GameObject glowGO = new GameObject("Glow", typeof(RectTransform), typeof(Image));
        glowGO.transform.SetParent(fill.transform, false);
        RectTransform glowRt = glowGO.GetComponent<RectTransform>();
        glowRt.anchorMin = Vector2.zero;
        glowRt.anchorMax = Vector2.one;
        glowRt.sizeDelta = new Vector2(0, 10);
        barGlow = glowGO.GetComponent<Image>();
        barGlow.color = new Color(1f, 1f, 1f, 0.15f);
        barGlow.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");

        // Percentage
        GameObject percGO = new GameObject("PercentageText", typeof(RectTransform), typeof(TextMeshProUGUI));
        percGO.transform.SetParent(bottomContainer.transform, false);
        percentageText = percGO.GetComponent<TextMeshProUGUI>();
        percentageText.alignment = TextAlignmentOptions.Right;
        percentageText.fontSize = 16;
        percentageText.fontStyle = FontStyles.Normal;
        percentageText.color = new Color(1, 1, 1, 0.5f);
        percentageText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        percentageText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        percentageText.rectTransform.sizeDelta = new Vector2(100, 30);
        percentageText.rectTransform.anchoredPosition = new Vector2(300, 25);

        // Status
        GameObject statGO = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
        statGO.transform.SetParent(bottomContainer.transform, false);
        statusText = statGO.GetComponent<TextMeshProUGUI>();
        statusText.alignment = TextAlignmentOptions.Left;
        statusText.fontSize = 14;
        statusText.characterSpacing = 2f; // Modern typography spacing
        statusText.color = new Color(1, 1, 1, 0.4f);
        statusText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        statusText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        statusText.rectTransform.sizeDelta = new Vector2(600, 30);
        statusText.rectTransform.anchoredPosition = new Vector2(-150, 25);

        // Cancel button completely omitted globally for minimal interface. 
        // Avant-garde designs don't let you cancel load processes abruptly with an ugly button.
    }

    private void Update()
    {
        if (loadingCanvas == null || !loadingCanvas.activeSelf) return;

        // 1. Sleek Background Pulsing
        if (backgroundImage != null && animateBackground)
        {
            float pulse = 1.0f + Mathf.Sin(Time.unscaledTime * bgPulseSpeed) * bgPulseAmplitude;
            backgroundImage.transform.localScale = new Vector3(pulse, pulse, 1f);
        }

        // 2. Ultra-Smooth Progress Bar
        _displayProgress = Mathf.SmoothDamp(_displayProgress, _targetProgress, ref _currentProgressVelocity, progressBarSmoothTime);
        if (progressBar != null) progressBar.value = _displayProgress;
        if (percentageText != null) percentageText.text = $"{(Mathf.RoundToInt(_displayProgress * 100))}%";

        // 3. Spinner Rotasyonu (Eğer varsa)
        if (spinner != null)
            spinner.transform.Rotate(Vector3.forward, -spinnerRotationSpeed * Time.unscaledDeltaTime);

        // 4. Logo & Subtle Glow Pulsing
        float pulseTime = Mathf.Sin(Time.unscaledTime * 3f);
        if (logo != null) logo.transform.localScale = Vector3.one * (1.0f + pulseTime * 0.02f);
        if (barGlow != null) barGlow.color = new Color(1f, 1f, 1f, 0.05f + (pulseTime + 1f) * 0.05f);

        // 5. Minimalist Floating UI
        if (bottomContainer != null)
        {
            float floatY = Mathf.Sin(Time.unscaledTime * floatingSpeed) * (floatingAmplitude * 0.5f);
            bottomContainer.anchoredPosition = _originalContainerPos + new Vector2(0, floatY);
        }

        // 6. Typography Focus Dots
        UpdateStatusAnimator();
    }

    private void UpdateStatusAnimator()
    {
        if (statusText == null) return;

        _dotsTimer += Time.unscaledDeltaTime;
        if (_dotsTimer >= 0.5f)
        {
            _dotsTimer = 0;
            _dotsCount = (_dotsCount + 1) % 4;
            string dots = new string('.', _dotsCount);
            statusText.text = (_currentBaseMessage + dots).ToUpper();
        }
    }

    /// <summary>
    /// Sahne yükleme işlemini başlatır.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

        // Zaten bir yükleme varsa durdur ve temizle
        if (_activeLoadCoroutine != null) StopCoroutine(_activeLoadCoroutine);
        
        _isCanceled = false;
        _activeLoadCoroutine = StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        float startTime = Time.unscaledTime;
        _targetProgress = 0f;
        _displayProgress = 0f;

        // Fade-In
        if (loadingCanvas != null)
        {
            loadingCanvas.SetActive(true);
            yield return StartCoroutine(FadeCanvas(0, 1));
            string systemReady = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Load_Readying") : "SİSTEM HAZIRLANIYOR";
            UpdateUIState(0, systemReady);
        }

        // Asenkron Yükleme
        _asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        if (_asyncOperation == null) { yield break; }
        
        _asyncOperation.allowSceneActivation = false;

        while (!_asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(_asyncOperation.progress / 0.9f);
            UpdateUIState(progress, GetStatusMessage(progress));

            if (_isCanceled)
            {
                _asyncOperation = null;
                yield return StartCoroutine(FadeCanvas(1, 0));
                loadingCanvas.SetActive(false);
                yield break;
            }

            if (_asyncOperation.progress >= 0.9f)
            {
                // Minimum görüntülenme süresi kontrolü
                float elapsed = Time.unscaledTime - startTime;
                if (elapsed < minDisplayTime)
                {
                    yield return new WaitForSecondsRealtime(minDisplayTime - elapsed);
                }

                string loadComplete = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Load_Complete") : "YÜKLEME TAMAMLANDI";
                UpdateUIState(1.0f, loadComplete);
                
                // Sahne değişiminden önce kısa bir bekleme (Arayüz geçiş doygunluğu)
                yield return new WaitForSecondsRealtime(0.2f);
                
                yield return StartCoroutine(FadeCanvas(1, 0));
                _asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        if (loadingCanvas != null) loadingCanvas.SetActive(false);
        _activeLoadCoroutine = null;
    }

    private IEnumerator FadeCanvas(float start, float end)
    {
        if (loadingCanvasGroup == null) yield break;
        
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            loadingCanvasGroup.alpha = Mathf.Lerp(start, end, t / fadeDuration);
            yield return null;
        }
        loadingCanvasGroup.alpha = end;
    }

    private void UpdateUIState(float progress, string message)
    {
        _targetProgress = progress;
        if (_currentBaseMessage != message)
        {
            _currentBaseMessage = message;
            if (statusText != null) statusText.text = (message + new string('.', _dotsCount)).ToUpper();
        }
    }

    private string GetStatusMessage(float progress)
    {
        if (Gazze.UI.LocalizationManager.Instance == null)
        {
            if (progress < 0.25f) return "VERİLER AYIKLANIYOR";
            if (progress < 0.50f) return "VARLIKLAR YÜKLENİYOR";
            if (progress < 0.75f) return "DÜNYA OLUŞTURULUYOR";
            return "SON AYARLAR YAPILIYOR";
        }

        if (progress < 0.25f) return Gazze.UI.LocalizationManager.Instance.GetTranslation("Load_Data");
        if (progress < 0.50f) return Gazze.UI.LocalizationManager.Instance.GetTranslation("Load_Assets");
        if (progress < 0.75f) return Gazze.UI.LocalizationManager.Instance.GetTranslation("Load_World");
        return Gazze.UI.LocalizationManager.Instance.GetTranslation("Load_Final");
    }

    public void CancelLoading()
    {
        if (Settings.AudioManager.Instance != null) Settings.AudioManager.Instance.PlayClickSound();
        
        _isCanceled = true;
        // Eğer bir yükleme devam ediyorsa, Unity'nin LoadSceneAsync işlemini doğrudan iptal etme yeteneği kısıtlıdır.
        // En güvenli yol, coroutine içindeki kontrolle MainMenu'ye dönmektir.
        SceneManager.LoadScene("MainMenu");
    }

    public void Show(string initialStatus = "LÜTFEN BEKLEYİN")
    {
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

        if (loadingCanvas != null)
        {
            _isCanceled = false;
            _targetProgress = 0f;
            _displayProgress = 0f;
            _currentProgressVelocity = 0f;
            loadingCanvas.SetActive(true);
            StartCoroutine(FadeCanvas(0, 1));
            UpdateUIState(0, initialStatus);
        }
    }

    public void Hide()
    {
        if (loadingCanvas != null && loadingCanvas.activeSelf)
        {
            StartCoroutine(HideCoroutine());
        }
    }

    private IEnumerator HideCoroutine()
    {
        yield return StartCoroutine(FadeCanvas(1, 0));
        if (loadingCanvas != null) loadingCanvas.SetActive(false);
    }

    #if UNITY_EDITOR
    [UnityEditor.MenuItem("Gazze/Managers/Create LoadingManager")]
    public static void CreateInScene()
    {
        LoadingManager existing = Object.FindFirstObjectByType<LoadingManager>(FindObjectsInactive.Include);
        if (existing != null)
        {
            UnityEditor.Selection.activeGameObject = existing.gameObject;
            return;
        }

        GameObject go = new GameObject("LoadingManager", typeof(LoadingManager));
        LoadingManager manager = go.GetComponent<LoadingManager>();
        manager.CreateDefaultUI();
        
        UnityEditor.Selection.activeGameObject = go;
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create LoadingManager");
    }
    #endif
}

