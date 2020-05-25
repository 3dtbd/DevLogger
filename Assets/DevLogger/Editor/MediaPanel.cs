﻿using Moments;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using WizardsCode.EditorUtils;

namespace WizardsCode.DevLogger
{
    [Serializable]
    public class MediaPanel
    {
        const string DATABASE_PATH = "Assets/ScreenCaptures.asset";

        [SerializeField] Vector2 mediaScrollPosition;

        [SerializeField] Recorder _recorder;
        [SerializeField] DevLogScreenCapture currentScreenCapture;
        [SerializeField] bool removeRecorder;
        [SerializeField] bool originalFinalBlitToCameraTarget;
        [SerializeField] bool m_IsSaving;
        [SerializeField] Camera m_Camera;

        // Animated GIF setup
        [SerializeField] bool preserveAspect = true; // Automatically compute height from the current aspect ratio
        [SerializeField] int width = 360; // Width in pixels
        [SerializeField] int fps = 16; // Height in pixels
        [SerializeField] int bufferSize = 10; // Number of seconds to record
        [SerializeField] int repeat = 0; // -1: no repeat, 0: infinite, >0: repeat count
        [SerializeField] int quality = 15; // Quality of color quantization, lower = better but slower (min 1, max 100)

        public MediaPanel(DevLogScreenCaptures captures, Camera camera)
        {
            ScreenCaptures = captures;
            CaptureCamera = camera;
        }

        internal Camera CaptureCamera { get; set; }
        internal DevLogScreenCaptures ScreenCaptures { get; set; }
        
        internal void OnEnable()
        {
            CaptureCamera = AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString("DevLogCaptureCamera_" + Application.productName), typeof(Camera)) as Camera;
        }

        internal void OnDisable()
        {
            EditorPrefs.SetString("DevLogCaptureCamera_" + Application.productName, AssetDatabase.GetAssetPath(CaptureCamera));
        }
            
        private Recorder Recorder
        {
            get
            {
                if (_recorder == null && m_Camera)
                {
                    _recorder = m_Camera.GetComponent<Recorder>();
                    if (_recorder == null)
                    {
                        _recorder = m_Camera.gameObject.AddComponent<Recorder>();
                        _recorder.Init();

                        PostProcessLayer pp = Camera.main.GetComponent<PostProcessLayer>();
                        if (pp != null)
                        {
                            originalFinalBlitToCameraTarget = pp.finalBlitToCameraTarget;
                            pp.finalBlitToCameraTarget = false;
                        }
                        removeRecorder = true;
                    }
                    else
                    {
                        removeRecorder = false;
                    }
                }

                return _recorder;
            }
        }

        internal void OnDestroy()
        {
            if (removeRecorder)
            {
                GameObject.DestroyImmediate(_recorder);
            }
            PostProcessLayer pp = Camera.main.GetComponent<PostProcessLayer>();
            if (pp != null)
            {
                pp.finalBlitToCameraTarget = originalFinalBlitToCameraTarget;
            }
        }

        private void OnFileSaved(int arg1, string arg2)
        {
            m_IsSaving = false;
            _recorder.Record();
        }

        private void OnProcessingDone()
        {
            m_IsSaving = true;
        }

        public void OnGUI()
        {
            Skin.StartSection("Media");

            mediaScrollPosition = EditorGUILayout.BeginScrollView(mediaScrollPosition, GUILayout.Height(140));
            if (ScreenCaptures.captures != null && ScreenCaptures.captures.Count > 0)
            {
                ImageSelectionGUI();
            }
            EditorGUILayout.EndScrollView();

            Skin.EndSection();

            Skin.StartSection("Capture");
            EditorGUILayout.BeginHorizontal();
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Scene View"))
                {
                    CaptureWindowScreenshot("UnityEditor.SceneView");
                }

                if (GUILayout.Button("Game View"))
                {
                    CaptureWindowScreenshot("UnityEditor.GameView");
                }

                EditorGUILayout.BeginVertical();

                switch (Recorder.State)
                {
                    case RecorderState.Paused: // We are paused so start recording. This allows saving of the last X seconds
                        Recorder.Setup(preserveAspect, width, width / 2, fps, bufferSize, repeat, quality);
                        Recorder.Record();

                        EditorGUILayout.LabelField("Starting Recording");
                        break;
                    case RecorderState.Recording:
                        if (GUILayout.Button("Save Animated GIF"))
                        {
                            currentScreenCapture = ScriptableObject.CreateInstance<DevLogScreenCapture>();
                            currentScreenCapture.Encoding = DevLogScreenCapture.ImageEncoding.gif;
                            currentScreenCapture.name = "In Game Footage";

                            Recorder.OnPreProcessingDone = OnProcessingDone;
                            Recorder.OnFileSaved = OnFileSaved;

                            Recorder.SaveFolder = currentScreenCapture.GetAbsoluteImageFolder();
                            Recorder.Filename = currentScreenCapture.Filename;

                            Recorder.Save(true);
                        }
                        break;
                    case RecorderState.PreProcessing:
                        EditorGUILayout.LabelField("Processing");
                        break;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Buffer (in seconds)");
                bufferSize = int.Parse(GUILayout.TextField(bufferSize.ToString()));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Quality (lower is better)");
                quality = int.Parse(GUILayout.TextField(quality.ToString()));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Width");
                width = int.Parse(GUILayout.TextField(width.ToString()));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Hierarchy"))
                {
                    CaptureWindowScreenshot("UnityEditor.SceneHierarchyWindow");
                }

                if (GUILayout.Button("Inspector"))
                {
                    CaptureWindowScreenshot("UnityEditor.InspectorWindow");
                }

                if (GUILayout.Button("Project"))
                {
                    CaptureWindowScreenshot("UnityEditor.ProjectBrowser");
                }

                if (GUILayout.Button("Scene View"))
                {
                    CaptureWindowScreenshot("UnityEditor.SceneView");
                }

                if (GUILayout.Button("Game View"))
                {
                    CaptureWindowScreenshot("UnityEditor.GameView");
                }

                if (GUILayout.Button("Console"))
                {
                    CaptureWindowScreenshot("UnityEditor.ConsoleWindow");
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Package Manager"))
                {
                    CaptureWindowScreenshot("UnityEditor.PackageManager.UI.PackageManagerWindow");
                }

                if (GUILayout.Button("Asset Store"))
                {
                    CaptureWindowScreenshot("UnityEditor.AssetStoreWindow");
                }

                if (GUILayout.Button("Project Settings"))
                {
                    CaptureWindowScreenshot("UnityEditor.ProjectSettingsWindow");
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            Skin.EndSection();
        }

        private void ImageSelectionGUI()
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = ScreenCaptures.captures.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal();
                DevLogScreenCapture capture = ScreenCaptures.captures[i];
                
                // TODO This shouldn't be necessary, there is a bug somewhere, this will guard against it until we can squish it
                if (capture == null)
                {
                    Debug.LogError("Screen Captures has a record to a capture but it is null.");
                    continue;
                }

                if (GUILayout.Button(capture.Texture, GUILayout.Width(100), GUILayout.Height(100)))
                {
                    ScreenCaptures.captures[i].IsSelected = !ScreenCaptures.captures[i].IsSelected;
                }
                ScreenCaptures.captures[i].IsSelected = EditorGUILayout.Toggle(ScreenCaptures.captures[i].IsSelected);
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("View"))
                {
                    string filepath = (DevLog.GetAbsoluteDirectory() + capture.Filename).Replace(@"/", @"\");
                    System.Diagnostics.Process.Start("Explorer.exe", @"/open,""" + filepath);
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void AddToLatestCaptures(DevLogScreenCapture screenCapture)
        {
            if (screenCapture != null)
            {
                ScreenCaptures.captures.Add(screenCapture);
            }
        }

        internal void Update()
        {
            if (Recorder == null || Recorder.State == RecorderState.PreProcessing)
            {
                return;
            }

            if (currentScreenCapture != null && !m_IsSaving && !currentScreenCapture.IsImageSaved)
            {
                AddToLatestCaptures(currentScreenCapture);

                AssetDatabase.AddObjectToAsset(currentScreenCapture, DATABASE_PATH);
                AssetDatabase.SaveAssets();

                currentScreenCapture.IsImageSaved = true;
            }
        }

        /// <summary>
        /// Captures a screenshot of the desired editor window. To get a list of all the
        /// windows available in the editor use:
        /// ```
        /// EditorWindow[] allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
        ///        foreach (EditorWindow window in allWindows)
        ///        {
        ///          Debug.Log(window);
        ///        }
        ///```
        /// </summary>
        /// <param name="windowName">The name of the window to be captured, for example:
        /// WizardsCode.DevLogger.DevLoggerWindow
        /// UnityEditor.AssetStoreWindow
        /// UnityEditor.TimelineWindow
        /// UnityEditor.AnimationWindow
        /// UnityEditor.Graphs.AnimatorControllerTool
        /// UnityEditor.NavMeshEditorWindow
        /// UnityEditor.LightingWindow
        /// </param>
        public void CaptureWindowScreenshot(string windowName)
        {
            DevLogScreenCapture screenCapture = ScriptableObject.CreateInstance<DevLogScreenCapture>();
            screenCapture.Encoding = DevLogScreenCapture.ImageEncoding.png;
            screenCapture.name = windowName;

            EditorWindow window;
            if (windowName.StartsWith("UnityEditor."))
            {
                window = EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType(windowName));
            }
            else
            {
                Type t = Type.GetType(windowName);
                window = EditorWindow.GetWindow(t);
            }
            window.Focus();

            EditorApplication.delayCall += () =>
            {
                int width = (int)window.position.width;
                int height = (int)window.position.height;
                Vector2 position = window.position.position;
                position.y += 18;

                Color[] pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(position, width, height);

                Texture2D windowTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
                windowTexture.SetPixels(pixels);

                byte[] bytes = windowTexture.EncodeToPNG();
                System.IO.File.WriteAllBytes(screenCapture.GetRelativeImagePath(), bytes);
                screenCapture.IsImageSaved = true;

                AddToLatestCaptures(screenCapture);

                AssetDatabase.AddObjectToAsset(screenCapture, DATABASE_PATH);
                AssetDatabase.SaveAssets();

                // TODO This used to be in the window, but now it's not so how do we get focus back?
                // this.Focus();
            };
        }
    }
}