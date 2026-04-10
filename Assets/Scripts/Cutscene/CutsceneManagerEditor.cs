#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Gazze.Cutscene
{
    [CustomEditor(typeof(CutsceneManager))]
    public class CutsceneManagerEditor : Editor
    {
        private SerializedProperty segments;
        private SerializedProperty playOnStart;
        private SerializedProperty useCinematicBars;
        private SerializedProperty useSubtitles;
        private SerializedProperty showDebugInfo;
        
        private bool showSegments = true;
        private bool showPlaybackSettings = true;
        private bool showCinematicSettings = true;
        private bool showAudioSettings = true;
        private bool showSubtitleSettings = true;
        private bool showDebugSettings = true;
        
        private int selectedSegmentIndex = -1;

        
        private GUIStyle headerStyle;
        private GUIStyle segmentStyle;
        private GUIStyle selectedSegmentStyle;
        
        private void OnEnable()
        {
            segments = serializedObject.FindProperty("segments");
            playOnStart = serializedObject.FindProperty("playOnStart");
            useCinematicBars = serializedObject.FindProperty("useCinematicBars");
            useSubtitles = serializedObject.FindProperty("useSubtitles");
            showDebugInfo = serializedObject.FindProperty("showDebugInfo");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            InitializeStyles();
            
            CutsceneManager manager = (CutsceneManager)target;
            
            // Header
            DrawHeader(manager);
            
            EditorGUILayout.Space(10);
            
            // Quick Controls
            if (Application.isPlaying)
            {
                DrawPlaybackControls(manager);
                EditorGUILayout.Space(10);
            }
            
            // References
            DrawReferencesSection();
            
            // Segments
            DrawSegmentsSection(manager);
            
            // Playback Settings
            DrawPlaybackSettingsSection();
            
            // Cinematic Effects
            DrawCinematicEffectsSection();
            
            // Audio Settings
            DrawAudioSettingsSection();
            
            // Subtitle Settings
            DrawSubtitleSettingsSection();
            
            // UI Control
            DrawUIControlSection();
            
            // Performance
            DrawPerformanceSection();
            
            // Debug
            DrawDebugSection();
            
            // Events
            DrawEventsSection();
            
            EditorGUILayout.Space(10);
            
            // Utility Buttons
            DrawUtilityButtons(manager);
            
            serializedObject.ApplyModifiedProperties();
        }

        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter
                };
            }
            
            if (segmentStyle == null)
            {
                segmentStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 5, 5)
                };
            }
            
            if (selectedSegmentStyle == null)
            {
                selectedSegmentStyle = new GUIStyle(segmentStyle);
                selectedSegmentStyle.normal.background = MakeTex(2, 2, new Color(0.3f, 0.6f, 1f, 0.2f));
            }
        }

        private void DrawHeader(CutsceneManager manager)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUILayout.Label("🎬 CUTSCENE MANAGER", headerStyle);
            
            EditorGUILayout.Space(5);
            
            if (Application.isPlaying)
            {
                string status = manager.IsPlaying ? 
                    (manager.IsPaused ? "⏸️ PAUSED" : "▶️ PLAYING") : 
                    "⏹️ STOPPED";
                
                Color statusColor = manager.IsPlaying ? 
                    (manager.IsPaused ? Color.yellow : Color.green) : 
                    Color.gray;
                
                GUI.color = statusColor;
                EditorGUILayout.LabelField("Status: " + status, EditorStyles.boldLabel);
                GUI.color = Color.white;
                
                if (manager.IsPlaying)
                {
                    EditorGUILayout.LabelField($"Segment: {manager.CurrentSegmentIndex + 1} / {manager.SegmentCount}");
                    EditorGUILayout.LabelField($"Loop: {manager.CurrentLoopCount}");
                }
            }
            else
            {
                EditorGUILayout.LabelField($"Segments: {manager.SegmentCount}");
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawPlaybackControls(CutsceneManager manager)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Playback Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = !manager.IsPlaying;
            if (GUILayout.Button("▶ Play", GUILayout.Height(30)))
            {
                manager.PlayCutscene();
            }
            GUI.enabled = true;
            
            GUI.enabled = manager.IsPlaying && !manager.IsPaused;
            if (GUILayout.Button("⏸ Pause", GUILayout.Height(30)))
            {
                manager.PauseCutscene();
            }
            GUI.enabled = true;
            
            GUI.enabled = manager.IsPlaying && manager.IsPaused;
            if (GUILayout.Button("▶ Resume", GUILayout.Height(30)))
            {
                manager.ResumeCutscene();
            }
            GUI.enabled = true;
            
            GUI.enabled = manager.IsPlaying;
            if (GUILayout.Button("⏹ Stop", GUILayout.Height(30)))
            {
                manager.StopCutscene();
            }
            
            if (GUILayout.Button("⏩ Skip", GUILayout.Height(30)))
            {
                manager.SkipCutscene();
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            // Segment jump controls
            if (manager.IsPlaying)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Jump to Segment:", GUILayout.Width(120));
                
                for (int i = 0; i < Mathf.Min(manager.SegmentCount, 10); i++)
                {
                    if (GUILayout.Button((i + 1).ToString(), GUILayout.Width(30)))
                    {
                        manager.JumpToSegment(i);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawReferencesSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("player"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraFollow"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mainCamera"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("musicSource"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sfxSource"));
            EditorGUILayout.EndVertical();
        }

        private void DrawSegmentsSection(CutsceneManager manager)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            showSegments = EditorGUILayout.Foldout(showSegments, $"🎞️ Segments ({segments.arraySize})", true, EditorStyles.foldoutHeader);
            
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                segments.InsertArrayElementAtIndex(segments.arraySize);
                selectedSegmentIndex = segments.arraySize - 1;
            }
            
            GUI.enabled = selectedSegmentIndex >= 0 && selectedSegmentIndex < segments.arraySize;
            if (GUILayout.Button("-", GUILayout.Width(30)))
            {
                segments.DeleteArrayElementAtIndex(selectedSegmentIndex);
                selectedSegmentIndex = Mathf.Clamp(selectedSegmentIndex, 0, segments.arraySize - 1);
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            if (showSegments)
            {
                EditorGUILayout.Space(5);
                
                // Segment list
                for (int i = 0; i < segments.arraySize; i++)
                {
                    DrawSegmentItem(i, segments.GetArrayElementAtIndex(i));
                }
                
                // Add segment button
                if (segments.arraySize == 0)
                {
                    EditorGUILayout.HelpBox("No segments added yet. Click + to add a segment.", MessageType.Info);
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawSegmentItem(int index, SerializedProperty segment)
        {
            bool isSelected = selectedSegmentIndex == index;
            
            EditorGUILayout.BeginVertical(isSelected ? selectedSegmentStyle : segmentStyle);
            
            EditorGUILayout.BeginHorizontal();
            
            // Segment number and name
            SerializedProperty nameProperty = segment.FindPropertyRelative("segmentName");
            string segmentName = nameProperty.stringValue;
            if (string.IsNullOrEmpty(segmentName)) segmentName = $"Segment {index + 1}";
            
            bool expanded = EditorGUILayout.Foldout(isSelected, $"{index + 1}. {segmentName}", true);
            
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                selectedSegmentIndex = isSelected ? -1 : index;
            }
            
            // Move up/down buttons
            GUI.enabled = index > 0;
            if (GUILayout.Button("▲", GUILayout.Width(30)))
            {
                segments.MoveArrayElement(index, index - 1);
                if (selectedSegmentIndex == index) selectedSegmentIndex--;
            }
            GUI.enabled = true;
            
            GUI.enabled = index < segments.arraySize - 1;
            if (GUILayout.Button("▼", GUILayout.Width(30)))
            {
                segments.MoveArrayElement(index, index + 1);
                if (selectedSegmentIndex == index) selectedSegmentIndex++;
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            if (isSelected && expanded)
            {
                EditorGUI.indentLevel++;
                
                // Identification
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("segmentName"));
                
                EditorGUILayout.Space(5);
                
                // Camera Movement
                EditorGUILayout.LabelField("Camera Movement", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("startPoint"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("endPoint"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("duration"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("smoothTransition"));
                
                EditorGUILayout.Space(5);
                
                // FOV
                EditorGUILayout.LabelField("Field of View", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("useDynamicFOV"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("startFOV"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("endFOV"));
                
                EditorGUILayout.Space(5);
                
                // Animation
                EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("easeType"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("movementCurve"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("fovCurve"));
                
                EditorGUILayout.Space(5);
                
                // Look At
                EditorGUILayout.LabelField("Look At", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("lookAtPlayer"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("customLookAtTarget"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("lookAtOffset"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("smoothLookAt"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("lookAtSpeed"));
                
                EditorGUILayout.Space(5);
                
                // Effects
                EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("useSlowMotion"));
                if (segment.FindPropertyRelative("useSlowMotion").boolValue)
                {
                    EditorGUILayout.PropertyField(segment.FindPropertyRelative("timeScale"));
                }
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("shakeCameraOnStart"));
                if (segment.FindPropertyRelative("shakeCameraOnStart").boolValue)
                {
                    EditorGUILayout.PropertyField(segment.FindPropertyRelative("shakeIntensity"));
                    EditorGUILayout.PropertyField(segment.FindPropertyRelative("shakeDuration"));
                }
                
                EditorGUILayout.Space(5);
                
                // Audio
                EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("backgroundMusic"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("soundEffect"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("musicVolume"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("sfxVolume"));
                
                EditorGUILayout.Space(5);
                
                // Subtitles
                EditorGUILayout.LabelField("Subtitles", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("showSubtitle"));
                if (segment.FindPropertyRelative("showSubtitle").boolValue)
                {
                    EditorGUILayout.PropertyField(segment.FindPropertyRelative("subtitleText"));
                    EditorGUILayout.PropertyField(segment.FindPropertyRelative("subtitleStartTime"));
                    EditorGUILayout.PropertyField(segment.FindPropertyRelative("subtitleDuration"));
                }
                
                EditorGUILayout.Space(5);
                
                // Wait Conditions
                EditorGUILayout.LabelField("Wait Conditions", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("waitForInput"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("waitDuration"));
                
                EditorGUILayout.Space(5);
                
                // Events
                EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("onSegmentStart"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("onSegmentEnd"));
                EditorGUILayout.PropertyField(segment.FindPropertyRelative("onSegmentProgress"));
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawPlaybackSettingsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            showPlaybackSettings = EditorGUILayout.Foldout(showPlaybackSettings, "▶️ Playback Settings", true, EditorStyles.foldoutHeader);
            
            if (showPlaybackSettings)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("playOnStart"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("findPointsByName"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("startCountdownAfter"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("allowSkip"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("skipCooldown"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("loopCutscene"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("loopCount"));
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawCinematicEffectsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            showCinematicSettings = EditorGUILayout.Foldout(showCinematicSettings, "🎥 Cinematic Effects", true, EditorStyles.foldoutHeader);
            
            if (showCinematicSettings)
            {
                EditorGUILayout.PropertyField(useCinematicBars);
                if (useCinematicBars.boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("barHeight"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("barsAnimationDuration"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("barsColor"));
                }
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("useFadeTransitions"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cinematicTint"));
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawAudioSettingsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            showAudioSettings = EditorGUILayout.Foldout(showAudioSettings, "🔊 Audio Settings", true, EditorStyles.foldoutHeader);
            
            if (showAudioSettings)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeInMusic"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeOutMusic"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("audioFadeDuration"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("masterVolume"));
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawSubtitleSettingsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            showSubtitleSettings = EditorGUILayout.Foldout(showSubtitleSettings, "💬 Subtitle Settings", true, EditorStyles.foldoutHeader);
            
            if (showSubtitleSettings)
            {
                EditorGUILayout.PropertyField(useSubtitles);
                if (useSubtitles.boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("subtitleFont"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("subtitleFontSize"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("subtitleColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("subtitleOutlineColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("subtitleOutlineWidth"));
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawUIControlSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoHideGameplayUI"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("additionalUIElementsToHide"));
            EditorGUILayout.EndVertical();
        }

        private void DrawPerformanceSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("⚡ Performance", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useObjectPooling"));
            EditorGUILayout.EndVertical();
        }

        private void DrawDebugSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            showDebugSettings = EditorGUILayout.Foldout(showDebugSettings, "🐛 Debug", true, EditorStyles.foldoutHeader);
            
            if (showDebugSettings)
            {
                EditorGUILayout.PropertyField(showDebugInfo);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("showGizmos"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("logEvents"));
                
                if (Application.isPlaying)
                {
                    EditorGUILayout.Space(5);
                    
                    CutsceneManager manager = (CutsceneManager)target;
                    var metrics = manager.GetPerformanceMetrics();
                    
                    if (metrics.Count > 0)
                    {
                        EditorGUILayout.LabelField("Performance Metrics:", EditorStyles.boldLabel);
                        
                        foreach (var metric in metrics)
                        {
                            EditorGUILayout.LabelField($"{metric.Key}: {metric.Value:F2}");
                        }
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawEventsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📢 Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onCutsceneStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onCutsceneEnd"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onCutsceneSkip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onSegmentChange"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onCutscenePause"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onCutsceneResume"));
            EditorGUILayout.EndVertical();
        }

        private void DrawUtilityButtons(CutsceneManager manager)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🛠️ Utilities", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Auto Setup Segments"))
            {
                // This would need to be implemented in CutsceneManager
                Debug.Log("Auto setup not available in edit mode");
            }
            
            if (GUILayout.Button("Clear All Segments"))
            {
                if (EditorUtility.DisplayDialog("Clear Segments", 
                    "Are you sure you want to clear all segments?", "Yes", "No"))
                {
                    segments.ClearArray();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Generate IDs"))
            {
                for (int i = 0; i < segments.arraySize; i++)
                {
                    var segment = segments.GetArrayElementAtIndex(i);
                    var idProp = segment.FindPropertyRelative("segmentID");
                    if (string.IsNullOrEmpty(idProp.stringValue))
                    {
                        idProp.stringValue = $"segment_{i}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
                    }
                }
            }
            
            if (GUILayout.Button("Export Timeline"))
            {
                ExportTimeline(manager);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void ExportTimeline(CutsceneManager manager)
        {
            string path = EditorUtility.SaveFilePanel("Export Timeline", "", "cutscene_timeline.txt", "txt");
            
            if (!string.IsNullOrEmpty(path))
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("CUTSCENE TIMELINE");
                sb.AppendLine("================");
                sb.AppendLine();
                
                float totalTime = 0f;
                
                for (int i = 0; i < segments.arraySize; i++)
                {
                    var segment = segments.GetArrayElementAtIndex(i);
                    string name = segment.FindPropertyRelative("segmentName").stringValue;
                    float duration = segment.FindPropertyRelative("duration").floatValue;
                    
                    sb.AppendLine($"[{totalTime:F2}s - {(totalTime + duration):F2}s] {name} ({duration:F2}s)");
                    
                    totalTime += duration;
                }
                
                sb.AppendLine();
                sb.AppendLine($"Total Duration: {totalTime:F2}s");
                
                System.IO.File.WriteAllText(path, sb.ToString());
                
                Debug.Log($"Timeline exported to: {path}");
            }
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            
            return result;
        }
    }
}
#endif
