using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gazze.CameraSystem;
using Gazze.UI;

namespace Gazze.Cutscene
{
    /// <summary>
    /// Advanced cutscene manager with cinematic effects, audio, subtitles, and enhanced features
    /// </summary>
    public class CutsceneManager : MonoBehaviour
    {
        #region Singleton Pattern
        public static CutsceneManager Instance { get; private set; }
        #endregion

        #region Inspector Fields
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private SmoothCameraFollow cameraFollow;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Segments")]
        [SerializeField] private List<CutsceneSegment> segments = new List<CutsceneSegment>();

        [Header("Playback Settings")]
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool findPointsByName = true;
        [SerializeField] public bool startCountdownAfter = true;
        [SerializeField] private bool allowSkip = true;
        [SerializeField] private float skipCooldown = 0.5f;
        [SerializeField] private bool loopCutscene = false;
        [SerializeField] private int loopCount = -1; // -1 = infinite

        [Header("Cinematic Effects")]
        [SerializeField] private bool useCinematicBars = true;
        [SerializeField] private bool useFadeTransitions = true;
        [SerializeField] private float barHeight = 120f;
        [SerializeField] private float barsAnimationDuration = 0.4f;
        [SerializeField] private Color barsColor = Color.black;
        [SerializeField] private Color cinematicTint = new Color(0.9f, 0.9f, 1f);

        [Header("Audio Settings")]
        [SerializeField] private bool fadeInMusic = true;
        [SerializeField] private bool fadeOutMusic = true;
        [SerializeField] private float audioFadeDuration = 1f;
        [SerializeField] private float masterVolume = 1f;

        [Header("Subtitle Settings")]
        [SerializeField] private bool useSubtitles = true;
        [SerializeField] private Font subtitleFont;
        [SerializeField] private int subtitleFontSize = 24;
        [SerializeField] private Color subtitleColor = Color.white;
        [SerializeField] private Color subtitleOutlineColor = Color.black;
        [SerializeField] private float subtitleOutlineWidth = 2f;

        [Header("UI Control")]
        [Tooltip("Cutscene oynatılırken sahnedeki diğer tüm Canvas'ları (Oyun İçi UI vb.) otomatik gizler.")]
        [SerializeField] private bool autoHideGameplayUI = true;
        [Tooltip("Eğer Canvas olmayan veya ekstra gizlemek istediğiniz objeler varsa buraya ekleyebilirsiniz.")]
        [SerializeField] private List<GameObject> additionalUIElementsToHide = new List<GameObject>();

        [Header("Performance")]
        [SerializeField] private bool useObjectPooling = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private bool logEvents = false;

        [Header("Events")]
        [SerializeField] private UnityEngine.Events.UnityEvent onCutsceneStart;
        [SerializeField] private UnityEngine.Events.UnityEvent onCutsceneEnd;
        [SerializeField] private UnityEngine.Events.UnityEvent onCutsceneSkip;
        [SerializeField] private UnityEngine.Events.UnityEvent<int> onSegmentChange;
        [SerializeField] private UnityEngine.Events.UnityEvent onCutscenePause;
        [SerializeField] private UnityEngine.Events.UnityEvent onCutsceneResume;
        #endregion

        #region Private Fields
        private bool isPlaying = false;
        private bool isPaused = false;
        private bool canSkip = true;
        private Coroutine cutsceneRoutine;
        private float lastSkipTime;
        private int currentSegmentIndex = -1;
        private int currentLoopCount = 0;
        
        // UI Elements
        private GameObject cinematicCanvasObj;
        private List<Canvas> autoHiddenCanvases = new List<Canvas>();
        private RectTransform topBar;
        private RectTransform bottomBar;
        private Image fadeOverlay;
        private Text subtitleText;
        private GameObject subtitlePanel;
        private GameObject debugPanel;
        private Text debugText;
        
        // Cache
        private float originalFOV;
        private bool wasPlayerEnabled;
        private bool wasCameraEnabled;
        private Dictionary<string, GameObject> objectPool;
        
        // Performance tracking
        private float cutsceneStartTime;
        private List<float> segmentCompletionTimes = new List<float>();
        
        // Audio
        private float originalMusicVolume;
        private Coroutine audioFadeRoutine;
        #endregion

        #region Serializable Classes
        [Serializable]
        public class CutsceneSegment
        {
            [Header("Identification")]
            public string segmentName = "New Segment";
            public string segmentID = ""; // Unique identifier for save/load
            
            [Header("Camera Movement")]
            public Transform startPoint;
            public Transform endPoint;
            public float duration = 3f;
            public bool smoothTransition = true;
            
            [Header("Field of View")]
            public float startFOV = 60f;
            public float endFOV = 60f;
            public bool useDynamicFOV = false;
            
            [Header("Animation")]
            public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            public AnimationCurve fovCurve = AnimationCurve.Linear(0, 0, 1, 1);
            public CameraEaseType easeType = CameraEaseType.EaseInOut;
            
            [Header("Look At")]
            public bool lookAtPlayer = true;
            public Transform customLookAtTarget;
            public Vector3 lookAtOffset = Vector3.up;
            public bool smoothLookAt = true;
            public float lookAtSpeed = 5f;
            
            [Header("Effects")]
            public bool useSlowMotion = false;
            [Range(0.1f, 1f)] public float timeScale = 0.5f;
            public bool shakeCameraOnStart = false;
            public float shakeIntensity = 0.2f;
            public float shakeDuration = 0.3f;
            public bool useMotionBlur = false;
            public float motionBlurIntensity = 0.5f;
            
            [Header("Audio")]
            public AudioClip backgroundMusic;
            public AudioClip soundEffect;
            [Range(0f, 1f)] public float musicVolume = 1f;
            [Range(0f, 1f)] public float sfxVolume = 1f;
            public bool fadeInAudio = true;
            public bool fadeOutAudio = true;
            
            [Header("Subtitles")]
            public bool showSubtitle = false;
            [TextArea(2, 4)] public string subtitleText = "";
            public float subtitleStartTime = 0f;
            public float subtitleDuration = 0f; // 0 = auto (segment duration)
            
            [Header("Wait Conditions")]
            public bool waitForInput = false;
            public float waitDuration = 0f; // Additional wait time at end
            
            [Header("Events")]
            public UnityEngine.Events.UnityEvent onSegmentStart;
            public UnityEngine.Events.UnityEvent onSegmentEnd;
            public UnityEngine.Events.UnityEvent<float> onSegmentProgress; // 0-1
        }

        public enum CameraEaseType
        {
            Linear,
            EaseIn,
            EaseOut,
            EaseInOut,
            SmoothStep,
            SmootherStep,
            Spring
        }

        [Serializable]
        public class CutsceneState
        {
            public int currentSegment;
            public float segmentProgress;
            public bool isPlaying;
            public bool isPaused;
            public int loopCount;
            public float timestamp;
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeObjectPool();
        }

        private void Start()
        {
            InitializeReferences();
            SetupCinematicElements();
            SetupAudioSources();
            
            if (playOnStart)
            {
                PlayCutsceneDelayed(0.1f);
            }
        }

        private void Update()
        {
            HandleSkipInput();
            HandlePauseInput();
            UpdateDebugInfo();
        }

        private void OnDestroy()
        {
            CleanupCutscene();
            
            if (Instance == this)
            {
                Instance = null;
            }
        }
        #endregion

        #region Initialization
        private void InitializeReferences()
        {
            // Ensure timescale is normal
            Time.timeScale = 1f;

            // Auto-find player if not assigned
            if (player == null && PlayerController.Instance != null)
            {
                player = PlayerController.Instance.transform;
            }
            
            // Auto-find camera follow if not assigned
            if (cameraFollow == null && SmoothCameraFollow.Instance != null)
            {
                cameraFollow = SmoothCameraFollow.Instance;
            }

            // Auto-find main camera if not assigned
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            
            // Store original FOV
            if (mainCamera != null)
            {
                originalFOV = mainCamera.fieldOfView;
            }

            // Auto-setup segments if enabled
            if (findPointsByName && segments.Count == 0)
            {
                AutoSetupSegments();
            }
            
            // Generate IDs for segments without them
            GenerateSegmentIDs();
        }

        private void SetupCinematicElements()
        {
            if (useCinematicBars)
            {
                CreateCinematicBars();
            }
            
            if (useSubtitles)
            {
                CreateSubtitlePanel();
            }
            
            if (showDebugInfo)
            {
                CreateDebugPanel();
            }
        }

        private void SetupAudioSources()
        {
            // Create audio sources if not assigned
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("CutsceneMusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = false;
                musicSource.playOnAwake = false;
            }
            
            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("CutsceneSFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
            
            originalMusicVolume = musicSource.volume;
        }

        private void InitializeObjectPool()
        {
            if (!useObjectPooling) return;
            
            objectPool = new Dictionary<string, GameObject>();
        }

        private void GenerateSegmentIDs()
        {
            for (int i = 0; i < segments.Count; i++)
            {
                if (string.IsNullOrEmpty(segments[i].segmentID))
                {
                    segments[i].segmentID = $"segment_{i}_{Guid.NewGuid().ToString().Substring(0, 8)}";
                }
            }
        }
        #endregion

        #region Cinematic Bars
        private void CreateCinematicBars()
        {
            // Create canvas
            cinematicCanvasObj = new GameObject("CinematicBarsCanvas");
            Canvas canvas = cinematicCanvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            
            CanvasScaler scaler = cinematicCanvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            cinematicCanvasObj.AddComponent<GraphicRaycaster>();
            
            // Create top bar
            topBar = CreateBar("TopBar", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            
            // Create bottom bar
            bottomBar = CreateBar("BottomBar", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            
            // Create fade overlay
            CreateFadeOverlay();
        }

        private RectTransform CreateBar(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            GameObject barObj = new GameObject(name);
            barObj.transform.SetParent(cinematicCanvasObj.transform, false);
            
            Image barImage = barObj.AddComponent<Image>();
            barImage.color = barsColor;
            barImage.raycastTarget = false;
            
            RectTransform rectTransform = barImage.rectTransform;
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(0, 0);
            
            return rectTransform;
        }

        private void CreateFadeOverlay()
        {
            GameObject fadeObj = new GameObject("FadeOverlay");
            fadeObj.transform.SetParent(cinematicCanvasObj.transform, false);
            
            fadeOverlay = fadeObj.AddComponent<Image>();
            fadeOverlay.color = new Color(0, 0, 0, 0);
            fadeOverlay.raycastTarget = false;
            
            RectTransform rectTransform = fadeOverlay.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
        }
        #endregion

        #region Subtitle System
        private void CreateSubtitlePanel()
        {
            if (cinematicCanvasObj == null) return;
            
            // Create subtitle background panel
            subtitlePanel = new GameObject("SubtitlePanel");
            subtitlePanel.transform.SetParent(cinematicCanvasObj.transform, false);
            
            RectTransform panelRect = subtitlePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.1f);
            panelRect.anchorMax = new Vector2(0.9f, 0.2f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = Vector2.zero;
            
            Image panelBg = subtitlePanel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.0f); // Arka planı tamamen şeffaf yap (Sadece yazılar çıksın)
            panelBg.raycastTarget = false;
            
            // Create subtitle text
            GameObject textObj = new GameObject("SubtitleText");
            textObj.transform.SetParent(subtitlePanel.transform, false);
            
            subtitleText = textObj.AddComponent<Text>();
            subtitleText.font = subtitleFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            subtitleText.fontSize = subtitleFontSize;
            subtitleText.color = subtitleColor;
            subtitleText.alignment = TextAnchor.MiddleCenter;
            subtitleText.horizontalOverflow = HorizontalWrapMode.Overflow;
            subtitleText.verticalOverflow = VerticalWrapMode.Overflow;
            subtitleText.supportRichText = true;
            subtitleText.raycastTarget = false;
            
            // Add outline
            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = subtitleOutlineColor;
            outline.effectDistance = new Vector2(subtitleOutlineWidth, -subtitleOutlineWidth);
            
            RectTransform textRect = subtitleText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(-20, -20);
            
            subtitlePanel.SetActive(false);
        }

        private void ShowSubtitle(string text, float duration)
        {
            if (!useSubtitles || subtitlePanel == null) return;
            
            subtitleText.text = text;
            subtitlePanel.SetActive(true);
            
            if (duration > 0)
            {
                StartCoroutine(HideSubtitleAfterDelay(duration));
            }
        }

        private void HideSubtitle()
        {
            if (subtitlePanel != null)
            {
                subtitlePanel.SetActive(false);
            }
        }

        private IEnumerator HideSubtitleAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            HideSubtitle();
        }
        #endregion

        #region Debug Panel
        private void CreateDebugPanel()
        {
            if (cinematicCanvasObj == null) return;
            
            debugPanel = new GameObject("DebugPanel");
            debugPanel.transform.SetParent(cinematicCanvasObj.transform, false);
            
            RectTransform panelRect = debugPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0.7f);
            panelRect.anchorMax = new Vector2(0.3f, 1f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = Vector2.zero;
            
            Image panelBg = debugPanel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.8f);
            
            GameObject textObj = new GameObject("DebugText");
            textObj.transform.SetParent(debugPanel.transform, false);
            
            debugText = textObj.AddComponent<Text>();
            debugText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            debugText.fontSize = 14;
            debugText.color = Color.green;
            debugText.alignment = TextAnchor.UpperLeft;
            
            RectTransform textRect = debugText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(-10, -10);
        }

        private void UpdateDebugInfo()
        {
            if (!showDebugInfo || debugText == null) return;
            
            string info = $"<b>CUTSCENE DEBUG</b>\n";
            info += $"Playing: {isPlaying}\n";
            info += $"Paused: {isPaused}\n";
            info += $"Segment: {currentSegmentIndex + 1}/{segments.Count}\n";
            info += $"Loop: {currentLoopCount}/{(loopCount == -1 ? "∞" : loopCount.ToString())}\n";
            
            if (isPlaying && currentSegmentIndex >= 0 && currentSegmentIndex < segments.Count)
            {
                var segment = segments[currentSegmentIndex];
                info += $"Current: {segment.segmentName}\n";
                info += $"Duration: {segment.duration:F1}s\n";
            }
            
            info += $"FPS: {(1f / Time.unscaledDeltaTime):F0}\n";
            
            debugText.text = info;
        }
        #endregion

        #region Auto Setup
        private void AutoSetupSegments()
        {
            segments.Clear();
            
            // Try to find a container first
            GameObject container = GameObject.Find("CutscenePoints");
            Transform searchRoot = container != null ? container.transform : transform;
            
            // Find all child transforms with "CutscenePoint" or "CameraPoint" in name
            Transform[] allTransforms = searchRoot.GetComponentsInChildren<Transform>();
            List<Transform> cutscenePoints = new List<Transform>();
            
            foreach (Transform t in allTransforms)
            {
                if (t.name.Contains("CutscenePoint") || t.name.Contains("CameraPoint") || t.name.Contains("Point"))
                {
                    // Skip the container itself
                    if (container != null && t == container.transform) continue;
                    cutscenePoints.Add(t);
                }
            }
            
            // Sort by name
            cutscenePoints.Sort((a, b) => string.Compare(a.name, b.name));
            
            // Create segments from pairs
            for (int i = 0; i < cutscenePoints.Count - 1; i++)
            {
                CutsceneSegment segment = new CutsceneSegment
                {
                    segmentName = $"Segment {i + 1}",
                    startPoint = cutscenePoints[i],
                    endPoint = cutscenePoints[i + 1],
                    duration = 3f
                };
                segments.Add(segment);
            }
            
            if (logEvents)
            {
                Debug.Log($"Auto-setup: Created {segments.Count} segments from {cutscenePoints.Count} points under {searchRoot.name}");
            }
        }
        #endregion

        #region Playback Control
        public void PlayCutscene()
        {
            if (isPlaying)
            {
                if (logEvents) Debug.LogWarning("Cutscene already playing");
                return;
            }
            
            if (segments.Count == 0)
            {
                Debug.LogError("No segments to play!");
                return;
            }
            
            cutsceneRoutine = StartCoroutine(PlayCutsceneSequence());
        }

        public void PlayCutsceneDelayed(float delay)
        {
            StartCoroutine(PlayCutsceneDelayedCoroutine(delay));
        }

        private IEnumerator PlayCutsceneDelayedCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayCutscene();
        }

        public void StopCutscene()
        {
            if (cutsceneRoutine != null)
            {
                StopCoroutine(cutsceneRoutine);
                cutsceneRoutine = null;
            }
            
            EndCutscene();
        }

        public void PauseCutscene()
        {
            if (!isPlaying || isPaused) return;
            
            isPaused = true;
            Time.timeScale = 0f;
            
            onCutscenePause?.Invoke();
            
            if (logEvents)
            {
                Debug.Log("Cutscene paused");
            }
        }

        public void ResumeCutscene()
        {
            if (!isPlaying || !isPaused) return;
            
            isPaused = false;
            Time.timeScale = 1f;
            
            onCutsceneResume?.Invoke();
            
            if (logEvents)
            {
                Debug.Log("Cutscene resumed");
            }
        }

        public void SkipCutscene()
        {
            if (!isPlaying || !allowSkip || !canSkip) return;
            
            if (Time.time - lastSkipTime < skipCooldown) return;
            
            lastSkipTime = Time.time;
            
            if (logEvents)
            {
                Debug.Log("Cutscene skipped");
            }
            
            onCutsceneSkip?.Invoke();
            
            StopCutscene();
        }

        public void JumpToSegment(int index)
        {
            if (index < 0 || index >= segments.Count)
            {
                Debug.LogError($"Invalid segment index: {index}");
                return;
            }
            
            bool wasPlaying = isPlaying;
            
            if (wasPlaying)
            {
                StopCutscene();
            }
            
            currentSegmentIndex = index - 1; // Will increment to index in PlayCutscene
            
            if (wasPlaying)
            {
                PlayCutscene();
            }
        }
        #endregion

        #region Cutscene Sequence
        private IEnumerator PlayCutsceneSequence()
        {
            isPlaying = true;
            cutsceneStartTime = Time.time;
            segmentCompletionTimes.Clear();
            
            // Start cutscene
            StartCutscene();
            
            // Play through all segments
            do
            {
                for (int i = 0; i < segments.Count; i++)
                {
                    currentSegmentIndex = i;
                    var segment = segments[i];
                    
                    if (logEvents)
                    {
                        Debug.Log($"Playing segment {i + 1}/{segments.Count}: {segment.segmentName}");
                    }
                    
                    onSegmentChange?.Invoke(i);
                    
                    yield return StartCoroutine(PlaySegment(segment));
                    
                    segmentCompletionTimes.Add(Time.time - cutsceneStartTime);
                }
                
                currentLoopCount++;
                
            } while (loopCutscene && (loopCount == -1 || currentLoopCount < loopCount));
            
            // End cutscene
            EndCutscene();
        }

        private void StartCutscene()
        {
            // Disable player
            if (player != null)
            {
                var controller = player.GetComponent<MonoBehaviour>();
                if (controller != null)
                {
                    wasPlayerEnabled = controller.enabled;
                    controller.enabled = false;
                }
            }
            
            // Disable camera follow
            if (cameraFollow != null)
            {
                wasCameraEnabled = cameraFollow.enabled;
                cameraFollow.enabled = false;
            }
            
            // Hide gameplay UI
            if (autoHideGameplayUI)
            {
                HideGameplayUI();
            }
            
            // Hide additional elements
            foreach (var element in additionalUIElementsToHide)
            {
                if (element != null)
                {
                    element.SetActive(false);
                }
            }
            
            // Show cinematic bars
            if (useCinematicBars && cinematicCanvasObj != null)
            {
                cinematicCanvasObj.SetActive(true);
                StartCoroutine(AnimateBars(0, barHeight, barsAnimationDuration));
            }
            
            // Fade in if needed
            if (useFadeTransitions)
            {
                StartCoroutine(FadeToColor(Color.clear, barsAnimationDuration));
            }
            
            onCutsceneStart?.Invoke();
            
            if (logEvents)
            {
                Debug.Log("Cutscene started");
            }
        }

        private void EndCutscene()
        {
            isPlaying = false;
            isPaused = false;
            currentSegmentIndex = -1;
            currentLoopCount = 0;
            
            // Restore time scale
            Time.timeScale = 1f;
            
            // Re-enable player
            if (player != null)
            {
                var controller = player.GetComponent<MonoBehaviour>();
                if (controller != null)
                {
                    controller.enabled = wasPlayerEnabled;
                }
            }
            
            // Re-enable camera follow
            if (cameraFollow != null)
            {
                cameraFollow.enabled = wasCameraEnabled;
            }
            
            // Restore FOV
            if (mainCamera != null)
            {
                mainCamera.fieldOfView = originalFOV;
            }
            
            // Show gameplay UI
            if (autoHideGameplayUI)
            {
                ShowGameplayUI();
            }
            
            // Show additional elements
            foreach (var element in additionalUIElementsToHide)
            {
                if (element != null)
                {
                    element.SetActive(true);
                }
            }
            
            // Hide cinematic bars
            if (useCinematicBars && cinematicCanvasObj != null)
            {
                StartCoroutine(AnimateBars(barHeight, 0, barsAnimationDuration, true));
            }
            
            // Hide subtitle
            HideSubtitle();
            
            // Stop audio
            StopAllAudio();
            
            onCutsceneEnd?.Invoke();
            
            if (logEvents)
            {
                Debug.Log($"Cutscene ended. Total time: {Time.time - cutsceneStartTime:F2}s");
            }
            
            // Start countdown if enabled
            if (startCountdownAfter && CountdownManager.Instance != null)
            {
                CountdownManager.Instance.StartCountdown();
            }
        }

        private IEnumerator PlaySegment(CutsceneSegment segment)
        {
            float startTime = Time.time;
            
            // Validate segment
            if (segment.startPoint == null || segment.endPoint == null)
            {
                Debug.LogError($"Segment '{segment.segmentName}' has missing start or end point!");
                yield break;
            }
            
            // Invoke start event
            segment.onSegmentStart?.Invoke();
            
            // Apply time scale
            float previousTimeScale = Time.timeScale;
            if (segment.useSlowMotion)
            {
                Time.timeScale = segment.timeScale;
            }
            
            // Play audio
            PlaySegmentAudio(segment);
            
            // Show subtitle
            if (segment.showSubtitle && !string.IsNullOrEmpty(segment.subtitleText))
            {
                float subtitleDur = segment.subtitleDuration > 0 ? segment.subtitleDuration : segment.duration;
                StartCoroutine(ShowSubtitleAtTime(segment.subtitleText, segment.subtitleStartTime, subtitleDur));
            }
            
            // Camera shake
            if (segment.shakeCameraOnStart)
            {
                StartCoroutine(CameraShake(segment.shakeIntensity, segment.shakeDuration));
            }
            
            // Main camera animation
            float elapsed = 0f;
            Vector3 startPos = segment.startPoint.position;
            Quaternion startRot = segment.startPoint.rotation;
            Vector3 endPos = segment.endPoint.position;
            Quaternion endRot = segment.endPoint.rotation;
            
            while (elapsed < segment.duration)
            {
                // Handle pause
                while (isPaused)
                {
                    yield return null;
                }
                
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / segment.duration);
                
                // Apply easing
                float easedT = ApplyEasing(t, segment.easeType, segment.movementCurve);
                
                // Interpolate position
                Vector3 currentPos = Vector3.Lerp(startPos, endPos, easedT);
                mainCamera.transform.position = currentPos;
                
                // Interpolate rotation or look at target
                if (segment.lookAtPlayer && player != null)
                {
                    Vector3 lookTarget = player.position + segment.lookAtOffset;
                    Quaternion targetRot = Quaternion.LookRotation(lookTarget - currentPos);
                    
                    if (segment.smoothLookAt)
                    {
                        mainCamera.transform.rotation = Quaternion.Slerp(
                            mainCamera.transform.rotation,
                            targetRot,
                            Time.deltaTime * segment.lookAtSpeed
                        );
                    }
                    else
                    {
                        mainCamera.transform.rotation = targetRot;
                    }
                }
                else if (segment.customLookAtTarget != null)
                {
                    Vector3 lookTarget = segment.customLookAtTarget.position + segment.lookAtOffset;
                    Quaternion targetRot = Quaternion.LookRotation(lookTarget - currentPos);
                    
                    if (segment.smoothLookAt)
                    {
                        mainCamera.transform.rotation = Quaternion.Slerp(
                            mainCamera.transform.rotation,
                            targetRot,
                            Time.deltaTime * segment.lookAtSpeed
                        );
                    }
                    else
                    {
                        mainCamera.transform.rotation = targetRot;
                    }
                }
                else
                {
                    mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, easedT);
                }
                
                // Interpolate FOV
                if (segment.useDynamicFOV)
                {
                    float fovT = segment.fovCurve.Evaluate(t);
                    mainCamera.fieldOfView = Mathf.Lerp(segment.startFOV, segment.endFOV, fovT);
                }
                
                // Invoke progress event
                segment.onSegmentProgress?.Invoke(t);
                
                yield return null;
            }
            
            // Ensure final position
            mainCamera.transform.position = endPos;
            if (!segment.lookAtPlayer && segment.customLookAtTarget == null)
            {
                mainCamera.transform.rotation = endRot;
            }
            if (segment.useDynamicFOV)
            {
                mainCamera.fieldOfView = segment.endFOV;
            }
            
            // Wait for input if needed
            if (segment.waitForInput)
            {
                yield return new WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0));
            }
            
            // Additional wait
            if (segment.waitDuration > 0)
            {
                yield return new WaitForSeconds(segment.waitDuration);
            }
            
            // Restore time scale
            Time.timeScale = previousTimeScale;
            
            // Invoke end event
            segment.onSegmentEnd?.Invoke();
        }

        private float ApplyEasing(float t, CameraEaseType easeType, AnimationCurve curve)
        {
            switch (easeType)
            {
                case CameraEaseType.Linear:
                    return t;
                    
                case CameraEaseType.EaseIn:
                    return t * t;
                    
                case CameraEaseType.EaseOut:
                    return t * (2f - t);
                    
                case CameraEaseType.EaseInOut:
                    return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
                    
                case CameraEaseType.SmoothStep:
                    return t * t * (3f - 2f * t);
                    
                case CameraEaseType.SmootherStep:
                    return t * t * t * (t * (6f * t - 15f) + 10f);
                    
                case CameraEaseType.Spring:
                    return 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
                    
                default:
                    return curve != null ? curve.Evaluate(t) : t;
            }
        }
        #endregion

        #region Audio System
        private void PlaySegmentAudio(CutsceneSegment segment)
        {
            // Play background music
            if (segment.backgroundMusic != null && musicSource != null)
            {
                musicSource.clip = segment.backgroundMusic;
                musicSource.volume = segment.musicVolume * masterVolume;
                
                if (segment.fadeInAudio && fadeInMusic)
                {
                    StartCoroutine(FadeInAudio(musicSource, segment.musicVolume * masterVolume, audioFadeDuration));
                }
                else
                {
                    musicSource.Play();
                }
            }
            
            // Play sound effect
            if (segment.soundEffect != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(segment.soundEffect, segment.sfxVolume * masterVolume);
            }
        }

        private void StopAllAudio()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                if (fadeOutMusic)
                {
                    StartCoroutine(FadeOutAudio(musicSource, audioFadeDuration));
                }
                else
                {
                    musicSource.Stop();
                }
            }
            
            if (sfxSource != null)
            {
                sfxSource.Stop();
            }
        }

        private IEnumerator FadeInAudio(AudioSource source, float targetVolume, float duration)
        {
            source.volume = 0f;
            source.Play();
            
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }
            
            source.volume = targetVolume;
        }

        private IEnumerator FadeOutAudio(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }
            
            source.volume = 0f;
            source.Stop();
        }
        #endregion

        #region Camera Effects
        private IEnumerator CameraShake(float intensity, float duration)
        {
            Vector3 originalPos = mainCamera.transform.localPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float x = UnityEngine.Random.Range(-1f, 1f) * intensity;
                float y = UnityEngine.Random.Range(-1f, 1f) * intensity;
                
                mainCamera.transform.localPosition = originalPos + new Vector3(x, y, 0);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            mainCamera.transform.localPosition = originalPos;
        }
        #endregion

        #region Subtitle Helpers
        private IEnumerator ShowSubtitleAtTime(string text, float startTime, float duration)
        {
            if (startTime > 0)
            {
                yield return new WaitForSeconds(startTime);
            }
            
            ShowSubtitle(text, duration);
        }
        #endregion

        #region UI Management
        private void HideGameplayUI()
        {
            autoHiddenCanvases.Clear();
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            
            foreach (Canvas canvas in allCanvases)
            {
                // Don't hide our cinematic canvas
                if (canvas.gameObject == cinematicCanvasObj) continue;
                
                // Don't hide DontDestroyOnLoad canvases
                if (canvas.gameObject.scene.name == "DontDestroyOnLoad") continue;
                
                if (canvas.enabled)
                {
                    canvas.enabled = false;
                    autoHiddenCanvases.Add(canvas);
                }
            }
        }

        private void ShowGameplayUI()
        {
            foreach (Canvas canvas in autoHiddenCanvases)
            {
                if (canvas != null)
                {
                    canvas.enabled = true;
                }
            }
            
            autoHiddenCanvases.Clear();
        }
        #endregion

        #region Fade Transitions
        private IEnumerator FadeToColor(Color targetColor, float duration)
        {
            if (fadeOverlay == null) yield break;
            
            Color startColor = fadeOverlay.color;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                fadeOverlay.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }
            
            fadeOverlay.color = targetColor;
        }

        private void SetImageAlpha(Image image, float toAlpha)
        {
            if (image == null) return;
            
            Color color = image.color;
            color.a = toAlpha;
            image.color = color;
        }

        private IEnumerator AnimateBars(float startHeight, float endHeight, float duration, bool disableOnComplete = false)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                
                // Smoothstep interpolation
                t = t * t * (3f - 2f * t);
                
                float currentHeight = Mathf.Lerp(startHeight, endHeight, t);

                if (topBar != null)
                {
                    topBar.sizeDelta = new Vector2(0, currentHeight);
                }
                
                if (bottomBar != null)
                {
                    bottomBar.sizeDelta = new Vector2(0, currentHeight);
                }

                yield return null;
            }

            // Ensure final values
            if (topBar != null)
            {
                topBar.sizeDelta = new Vector2(0, endHeight);
            }
            
            if (bottomBar != null)
            {
                bottomBar.sizeDelta = new Vector2(0, endHeight);
            }

            if (disableOnComplete && cinematicCanvasObj != null)
            {
                cinematicCanvasObj.SetActive(false);
            }
        }
        #endregion

        #region Input Handling
        private void HandleSkipInput()
        {
            if (!isPlaying || !allowSkip || !canSkip) return;

            bool skipPressed = false;

            // Mouse input
            if (UnityEngine.InputSystem.Mouse.current != null && 
                UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
            {
                skipPressed = true;
            }

            // Touch input
            if (UnityEngine.InputSystem.Touchscreen.current != null && 
                UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                skipPressed = true;
            }

            // Keyboard input
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame || 
                    UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    skipPressed = true;
                }
            }

            if (skipPressed)
            {
                SkipCutscene();
            }
        }

        private void HandlePauseInput()
        {
            if (!isPlaying) return;
            
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.pKey.wasPressedThisFrame)
            {
                if (isPaused)
                {
                    ResumeCutscene();
                }
                else
                {
                    PauseCutscene();
                }
            }
        }
        #endregion

        #region Save/Load System
        public CutsceneState SaveState()
        {
            return new CutsceneState
            {
                currentSegment = currentSegmentIndex,
                segmentProgress = 0f, // Could be enhanced to save exact progress
                isPlaying = isPlaying,
                isPaused = isPaused,
                loopCount = currentLoopCount,
                timestamp = Time.time
            };
        }

        public void LoadState(CutsceneState state)
        {
            if (state == null)
            {
                Debug.LogError("Cannot load null state");
                return;
            }
            
            currentSegmentIndex = state.currentSegment;
            currentLoopCount = state.loopCount;
            
            if (state.isPlaying && !isPlaying)
            {
                JumpToSegment(state.currentSegment);
            }
            
            if (state.isPaused)
            {
                PauseCutscene();
            }
        }

        public string SerializeState()
        {
            CutsceneState state = SaveState();
            return JsonUtility.ToJson(state);
        }

        public void DeserializeState(string json)
        {
            try
            {
                CutsceneState state = JsonUtility.FromJson<CutsceneState>(json);
                LoadState(state);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize cutscene state: {e.Message}");
            }
        }
        #endregion

        #region Cleanup
        private void CleanupCutscene()
        {
            if (cinematicCanvasObj != null)
            {
                Destroy(cinematicCanvasObj);
            }
            
            StopAllAudio();
        }
        #endregion

        #region Gizmos
        private void OnDrawGizmos()
        {
            if (!showGizmos || segments == null || segments.Count == 0) return;

            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                
                if (segment.startPoint == null || segment.endPoint == null) continue;

                // Color based on segment index
                float hue = (float)i / segments.Count;
                Gizmos.color = Color.HSVToRGB(hue, 0.7f, 0.9f);

                // Draw path
                Gizmos.DrawLine(segment.startPoint.position, segment.endPoint.position);
                
                // Draw start point
                Gizmos.DrawWireSphere(segment.startPoint.position, 0.4f);
                
                // Draw end point
                Gizmos.DrawWireCube(segment.endPoint.position, Vector3.one * 0.4f);
                
                // Draw direction arrows
                Vector3 direction = (segment.endPoint.position - segment.startPoint.position).normalized;
                Vector3 midPoint = Vector3.Lerp(segment.startPoint.position, segment.endPoint.position, 0.5f);
                
                Gizmos.DrawRay(midPoint, direction * 0.5f);
                
                // Draw look at target
                if (segment.lookAtPlayer && player != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(midPoint, player.position + segment.lookAtOffset);
                }
                else if (segment.customLookAtTarget != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(midPoint, segment.customLookAtTarget.position + segment.lookAtOffset);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos || segments == null || segments.Count == 0) return;

            // Draw FOV visualization for selected segment
            foreach (var segment in segments)
            {
                if (segment.startPoint == null || segment.endPoint == null) continue;
                
                Gizmos.color = Color.yellow;
                
                // Visualize camera frustum at start and end points
                DrawCameraFrustum(segment.startPoint.position, segment.startPoint.rotation, segment.startFOV);
                
                Gizmos.color = Color.cyan;
                DrawCameraFrustum(segment.endPoint.position, segment.endPoint.rotation, segment.endFOV);
            }
        }

        private void DrawCameraFrustum(Vector3 position, Quaternion rotation, float fov)
        {
            float distance = 2f;
            float aspect = 16f / 9f;
            float halfHeight = Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * distance;
            float halfWidth = halfHeight * aspect;

            Vector3 forward = rotation * Vector3.forward;
            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;

            Vector3 farCenter = position + forward * distance;
            Vector3 farTopLeft = farCenter + up * halfHeight - right * halfWidth;
            Vector3 farTopRight = farCenter + up * halfHeight + right * halfWidth;
            Vector3 farBottomLeft = farCenter - up * halfHeight - right * halfWidth;
            Vector3 farBottomRight = farCenter - up * halfHeight + right * halfWidth;

            // Draw frustum lines
            Gizmos.DrawLine(position, farTopLeft);
            Gizmos.DrawLine(position, farTopRight);
            Gizmos.DrawLine(position, farBottomLeft);
            Gizmos.DrawLine(position, farBottomRight);
            
            // Draw far plane
            Gizmos.DrawLine(farTopLeft, farTopRight);
            Gizmos.DrawLine(farTopRight, farBottomRight);
            Gizmos.DrawLine(farBottomRight, farBottomLeft);
            Gizmos.DrawLine(farBottomLeft, farTopLeft);
        }
        #endregion

        #region Public API
        /// <summary>
        /// Checks if a cutscene is currently playing
        /// </summary>
        public bool IsPlaying => isPlaying;

        /// <summary>
        /// Checks if cutscene is paused
        /// </summary>
        public bool IsPaused => isPaused;

        /// <summary>
        /// Gets the total number of segments
        /// </summary>
        public int SegmentCount => segments.Count;

        /// <summary>
        /// Gets the current segment index
        /// </summary>
        public int CurrentSegmentIndex => currentSegmentIndex;

        /// <summary>
        /// Gets the current loop count
        /// </summary>
        public int CurrentLoopCount => currentLoopCount;

        /// <summary>
        /// Adds a new segment to the cutscene
        /// </summary>
        public void AddSegment(CutsceneSegment segment)
        {
            if (segment != null)
            {
                if (string.IsNullOrEmpty(segment.segmentID))
                {
                    segment.segmentID = $"segment_{segments.Count}_{Guid.NewGuid().ToString().Substring(0, 8)}";
                }
                segments.Add(segment);
            }
        }

        /// <summary>
        /// Inserts a segment at the specified index
        /// </summary>
        public void InsertSegment(int index, CutsceneSegment segment)
        {
            if (segment != null && index >= 0 && index <= segments.Count)
            {
                if (string.IsNullOrEmpty(segment.segmentID))
                {
                    segment.segmentID = $"segment_{index}_{Guid.NewGuid().ToString().Substring(0, 8)}";
                }
                segments.Insert(index, segment);
            }
        }

        /// <summary>
        /// Removes a segment at the specified index
        /// </summary>
        public void RemoveSegment(int index)
        {
            if (index >= 0 && index < segments.Count)
            {
                segments.RemoveAt(index);
            }
        }

        /// <summary>
        /// Gets a segment by index
        /// </summary>
        public CutsceneSegment GetSegment(int index)
        {
            if (index >= 0 && index < segments.Count)
            {
                return segments[index];
            }
            return null;
        }

        /// <summary>
        /// Gets a segment by ID
        /// </summary>
        public CutsceneSegment GetSegmentByID(string id)
        {
            return segments.FirstOrDefault(s => s.segmentID == id);
        }

        /// <summary>
        /// Clears all segments
        /// </summary>
        public void ClearSegments()
        {
            segments.Clear();
        }

        /// <summary>
        /// Sets the bars color
        /// </summary>
        public void SetBarsColor(Color color)
        {
            barsColor = color;
            
            if (topBar != null)
            {
                topBar.GetComponent<Image>().color = color;
            }
            
            if (bottomBar != null)
            {
                bottomBar.GetComponent<Image>().color = color;
            }
        }

        /// <summary>
        /// Sets the master volume
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// Sets whether to show debug info
        /// </summary>
        public void SetShowDebugInfo(bool show)
        {
            showDebugInfo = show;
            if (debugPanel != null)
            {
                debugPanel.SetActive(show);
            }
        }

        /// <summary>
        /// Gets performance metrics
        /// </summary>
        public Dictionary<string, float> GetPerformanceMetrics()
        {
            var metrics = new Dictionary<string, float>();
            
            if (isPlaying)
            {
                metrics["CurrentRuntime"] = Time.time - cutsceneStartTime;
            }
            
            if (segmentCompletionTimes.Count > 0)
            {
                metrics["AverageSegmentTime"] = segmentCompletionTimes.Average();
                metrics["TotalPlaytime"] = segmentCompletionTimes.Sum();
            }
            
            metrics["SegmentCount"] = segments.Count;
            metrics["LoopCount"] = currentLoopCount;
            
            return metrics;
        }
        #endregion
    }
}
