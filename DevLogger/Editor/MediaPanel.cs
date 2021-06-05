using Moments;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
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

        // Animated GIF setup
        [SerializeField] bool preserveAspect = true; // Automatically compute height from the current aspect ratio
        [SerializeField] int width = 360; // Width in pixels
        [SerializeField] int fps = 16; // Height in pixels
        [SerializeField] int bufferSize = 10; // Number of seconds to record
        [SerializeField] int repeat = 0; // -1: no repeat, 0: infinite, >0: repeat count
        [SerializeField] int framesPerColorSample = 6; // the number of frames between redefining the color pallete, smaller is better
        [SerializeField] int quality = 15; // Quality of color quantization, lower = better but slower (min 1, max 100)

        public MediaPanel(DevLogScreenCaptureCollection captures, Camera camera)
        {
            ScreenCaptures = captures;
            CaptureCamera = camera;
        }

        internal Camera CaptureCamera { get; set; }

        internal DevLogScreenCaptureCollection ScreenCaptures { get; set; }
        internal string CapturesFolderPath(DevLogScreenCapture capture) {
            string path = Settings.CaptureFileFolderPath;

            if (Settings.OrganizeCapturesByProject)
            {
                path += Path.DirectorySeparatorChar + Application.productName;
            }

            if (Settings.OrganizeCapturesByScene)
            {
                path += Path.DirectorySeparatorChar + SceneManager.GetActiveScene().name;
            }
            path += Path.DirectorySeparatorChar;

            Directory.CreateDirectory(path);

            return path;
        }
        
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
                if (!CaptureCamera)
                {
                    CaptureCamera = Camera.main;
                }

                if (_recorder == null && CaptureCamera)
                {
                    _recorder = CaptureCamera.GetComponent<Recorder>();
                    if (_recorder == null)
                    {
                        _recorder = CaptureCamera.gameObject.AddComponent<Recorder>();
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

        public bool hasSelectedImages { 
            get
            {
                return ScreenCaptures != null && ScreenCaptures.SelectedCount > 0;
            }
        }

        internal void OnImageSelection()
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

        private void OnFileSaved(int workerID, string filePath)
        {
            m_IsSaving = false;
            _recorder.Record();
            Debug.Log("GIF saved to " + filePath);
            Debug.Log("ScreenCapture object:\n" + currentScreenCapture.ToString());
        }

        private void OnPreProcessingDone()
        {
            m_IsSaving = true;
            Debug.Log("GIF Pre Processing is complete");
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

            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Open Media Folder"))
            {
                string path = Settings.CaptureFileFolderPath;
                if (Settings.OrganizeCapturesByProject)
                {
                    path += Path.DirectorySeparatorChar + Application.productName;
                }

                if (Settings.OrganizeCapturesByScene)
                {
                    path += Path.DirectorySeparatorChar + SceneManager.GetActiveScene().name;
                }
                System.Diagnostics.Process.Start("Explorer.exe", string.Format("/open, \"{0}\"", path.Replace(@"/", @"\")));
            }
            EditorGUILayout.EndVertical();

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
                        Recorder.Setup(preserveAspect, width, width / 2, fps, bufferSize, repeat, quality, framesPerColorSample);
                        Recorder.Record();

                        EditorGUILayout.LabelField("Starting Recording");
                        break;
                    case RecorderState.Recording:
                        if (GraphicsSettings.currentRenderPipeline == null)
                        {
                            if (GUILayout.Button("Save Animated GIF"))
                            {
                                Debug.Log("Save Animated Gif button pressed");
                                CaptureGif();
                            }
                        } else
                        {
                            GUILayout.Label("Capturing a GIF is not currently supported in HDRP/URP");
                        }
                        break;
                    case RecorderState.PreProcessing:
                        EditorGUILayout.LabelField("Processing");
                        break;
                }

                if (GraphicsSettings.currentRenderPipeline == null && Recorder.State == RecorderState.Recording)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Buffer (in seconds)");
                    bufferSize = int.Parse(GUILayout.TextField(bufferSize.ToString()));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Quality (lower is better)");
                    quality = int.Parse(GUILayout.TextField(quality.ToString()));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Frames per Color Sample (lower is better)");
                    framesPerColorSample = int.Parse(GUILayout.TextField(framesPerColorSample.ToString()));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Width");
                    width = int.Parse(GUILayout.TextField(width.ToString()));
                    EditorGUILayout.EndHorizontal();

                    if (GUILayout.Button("Apply"))
                    {
                        Recorder.Init();
                    }

                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();

                // Buttons to capture primary windows
                string[] primaryWindowTitles = { "Hierarchy", "Inspector", "Project", "Scene", "Game", "Console" };
                GUILayoutOption layoutOption = GUILayout.Height(60);
                if (GUILayout.Button("Hierarchy", layoutOption))
                {
                    CaptureWindowScreenshot("UnityEditor.SceneHierarchyWindow");
                }

                if (GUILayout.Button("Inspector", layoutOption))
                {
                    CaptureWindowScreenshot("UnityEditor.InspectorWindow");
                }

                if (GUILayout.Button("Project", layoutOption))
                {
                    CaptureWindowScreenshot("UnityEditor.ProjectBrowser");
                }

                if (GUILayout.Button("Scene View", layoutOption))
                {
                    CaptureWindowScreenshot("UnityEditor.SceneView");
                }

                if (GUILayout.Button("Game View", layoutOption))
                {
                    CaptureWindowScreenshot("UnityEditor.GameView");
                }

                if (GUILayout.Button("Console", layoutOption))
                {
                    CaptureWindowScreenshot("UnityEditor.ConsoleWindow");
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                // Buttons to capture secondary windows
                string[] excludedWindowTitles = { "Asset Store" };
                EditorWindow[] allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
                foreach (EditorWindow window in allWindows)
                {
                    if (!primaryWindowTitles.Contains<string>(window.titleContent.text) && !excludedWindowTitles.Contains<string>(window.titleContent.text))
                    {
                        if (GUILayout.Button(window.titleContent.text))
                        {
                            CaptureWindowScreenshot(window.GetType().FullName);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            Skin.EndSection();
        }
        
        internal void CaptureGif()
        {
            currentScreenCapture = ScriptableObject.CreateInstance<DevLogScreenCapture>();

            currentScreenCapture.productName = Application.productName;
            currentScreenCapture.version = Application.version;
            currentScreenCapture.timestamp = DateTime.Now;
            currentScreenCapture.sceneName = SceneManager.GetActiveScene().name;
            currentScreenCapture.encoding = DevLogScreenCapture.ImageEncoding.gif;
            currentScreenCapture.windowName = "In_Game_Footage";
            currentScreenCapture.name = Application.productName + " v" + Application.version + currentScreenCapture.timestamp.ToLongDateString();
            currentScreenCapture.AbsoluteSaveFolder = CapturesFolderPath(currentScreenCapture);

            Debug.Log("Created ScreenCapture object:\n" + currentScreenCapture.ToString());

            Recorder.OnPreProcessingDone = OnPreProcessingDone;
            Recorder.OnFileSaved = OnFileSaved;

            Recorder.SavePath = currentScreenCapture.ImagePath;
            Recorder.Save();
        }

        private void ImageSelectionGUI()
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = ScreenCaptures.captures.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal();
                DevLogScreenCapture capture = ScreenCaptures.captures[i];
                
                if (GUILayout.Button(capture.Texture, GUILayout.Width(100), GUILayout.Height(100)))
                {
                    ScreenCaptures.captures[i].IsSelected = !ScreenCaptures.captures[i].IsSelected;
                }
                ScreenCaptures.captures[i].IsSelected = EditorGUILayout.Toggle(ScreenCaptures.captures[i].IsSelected);
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("View"))
                {
                    System.Diagnostics.Process.Start("Explorer.exe", string.Format("/open, \"{0}\"", capture.ImagePath));
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void AddToLatestCaptures(DevLogScreenCapture screenCapture)
        {
            if (screenCapture == null) return;

            AssetDatabase.AddObjectToAsset(screenCapture, ScreenCaptures);
            screenCapture.IsSelected = true;

            ScreenCaptures.captures.Add(screenCapture);
            EditorUtility.SetDirty(ScreenCaptures);

            AssetDatabase.SaveAssets();
        }

        internal void Update()
        {
            if (Recorder == null || Recorder.State == RecorderState.PreProcessing)
            {
                return;
            }

            if (currentScreenCapture != null && !m_IsSaving && !currentScreenCapture.IsImageSaved)
            {
                currentScreenCapture.IsImageSaved = true;
                AddToLatestCaptures(currentScreenCapture);
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
        /// </param>
        public void CaptureWindowScreenshot(string windowName)
        {
            DevLogScreenCapture screenCapture = ScriptableObject.CreateInstance<DevLogScreenCapture>();
            screenCapture.productName = Application.productName;
            screenCapture.version = Application.version;
            screenCapture.timestamp = DateTime.Now;
            screenCapture.sceneName = SceneManager.GetActiveScene().name;
            screenCapture.encoding = DevLogScreenCapture.ImageEncoding.png;
            screenCapture.windowName = windowName;
            screenCapture.name = Application.productName + " v" + Application.version + " " + SceneManager.GetActiveScene().name;
            screenCapture.AbsoluteSaveFolder = CapturesFolderPath(screenCapture);

            EditorWindow window;
            if (windowName.StartsWith("UnityEditor."))
            {
                window = EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType(windowName));
            }
            else
            {
                Type type = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName.Equals(windowName));
                window = EditorWindow.GetWindow(type);
            }
            window.Focus();

            EditorApplication.delayCall += () =>
            {
                int width = (int)window.position.width;
                int height = (int)window.position.height;
                Vector2 position = window.position.position;
                
                if (window.maximized && Settings.TrimTabsWhenMaximized)
                {
                    position.y += 18; // trim the tabs
                }

                if (windowName.EndsWith("SceneView") || (windowName.EndsWith("GameView") && Settings.TrimGameViewToolbar))
                {
                    position.y += 18; // trim the toolbar
                    height -= 18;
                }

                Color[] pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(position, width, height);

                Texture2D windowTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
                windowTexture.SetPixels(pixels);

                byte[] bytes = windowTexture.EncodeToPNG();
                System.IO.File.WriteAllBytes(screenCapture.ImagePath, bytes);
                screenCapture.IsImageSaved = true;

                AddToLatestCaptures(screenCapture);

                // TODO This used to be in the window, but now it's not so how do we get focus back?
                // this.Focus();
            };
        }
    }
}