using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WizardsCode.EditorUtils;
using WizardsCode.Git;
using WizardsCode.Social;

namespace WizardsCode.DevLogger
{

    /// <summary>
    /// The main DevLogger control window.
    /// </summary>
    public class DevLoggerWindow : EditorWindow
    {
        EntryPanel m_EntryPanel;
        TwitterPanel m_TwitterPanel;
        DiscordPanel m_DiscordPanel;
        SchedulingPanel m_SchedulingPanel;
        GitPanel m_GitPanel;
        public MediaPanel mediaPanel;
        DevLogPanel m_DevLogPanel;
        DevLogScreenCaptureCollection m_ScreenCaptures;
        DevLogEntries m_DevLogEntries;
        Camera m_CaptureCamera;

        private static bool startCapture;

        private string[] toolbarLabels = { "Entry", "Dev Log", "Schedule", "Git", "Settings" };
        private int selectedTab = 0;
        private Vector2 entryScrollPosition;
        static EditorWindow window;

        public DevLogEntry currentEntry { get; set; }

        [UnityEditor.MenuItem("Tools/Wizards Code/Dev Logger")]
        public static void ShowWindow()
        {
            window = EditorWindow.GetWindow(typeof(DevLoggerWindow), false, "DevLog: " + Application.productName, true);
        }

        private void Awake()
        {
            m_DevLogEntries = AssetDatabase.LoadAssetAtPath(Settings.DevLogScriptableObjectPath, typeof(DevLogEntries)) as DevLogEntries;
            m_DevLogPanel = new DevLogPanel(m_DevLogEntries);

            //TODO this needs to be done in OnEnable to avoid edge case bugs on an upgrade
            m_ScreenCaptures = AssetDatabase.LoadAssetAtPath(Settings.ScreenCaptureScriptableObjectPath, typeof(DevLogScreenCaptureCollection)) as DevLogScreenCaptureCollection;
            if (m_CaptureCamera == null)
            {
                m_CaptureCamera = Camera.main;
            }

            mediaPanel = new MediaPanel(m_ScreenCaptures,
                m_CaptureCamera);

            m_EntryPanel = new EntryPanel(m_DevLogEntries);
            m_TwitterPanel = new TwitterPanel();
            m_DiscordPanel = new DiscordPanel();
            m_SchedulingPanel = new SchedulingPanel(m_DevLogEntries);
            m_GitPanel = new GitPanel(m_EntryPanel);
        }

        private void OnEnable()
        {
            EditorApplication.update += Update;
            mediaPanel.OnEnable();
            if (m_SchedulingPanel == null)
            {
                // For some reason the scheduling panel is occasionally set to null, this resets it
                // TODO remove this workaround by fixing the actual bug!!!
                m_SchedulingPanel = new SchedulingPanel(m_DevLogEntries);
            }
            m_SchedulingPanel.OnEnable();
        }

        internal void EditCurrentEntry()
        {
            m_EntryPanel.EditEntry(currentEntry);
            SwitchToEntryTab();
        }

        internal void SwitchToEntryTab()
        {
            selectedTab = 0;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
            mediaPanel.OnEnable();
            m_SchedulingPanel.OnDisable();
        }

        private void OnImageSelection()
        {
            mediaPanel.OnImageSelection();
        }

        void Update()
        {
            mediaPanel.Update();
            if (m_SchedulingPanel == null)
            {
                m_SchedulingPanel = new SchedulingPanel(m_DevLogEntries);
                m_SchedulingPanel.OnEnable();
            }
            m_SchedulingPanel.Update();
        }

        #region GUI

        /* Removed as it doesn't have the error checking available in the main windoe.
        [MenuItem("Tools/Wizards Code/Capture GIF %#`")]
        static void CaptureGif()
        {
            startCapture = true;
        }

        [MenuItem("Tools/Wizards Code/Capture GIF", true)]
        static bool ValidateCaptureGif()
        {
            return Application.isPlaying;
        }
        */

        private void OnInspectorUpdate()
        {
            if (startCapture)
            {
                mediaPanel.CaptureGif();
                startCapture = false;
            }
        }

        void OnGUI()
        {
            try
            {
                selectedTab = GUILayout.Toolbar(selectedTab, toolbarLabels);
                switch (selectedTab)
                {
                    case 0:
                        if (m_DevLogEntries != null && m_ScreenCaptures != null) // Check we are correctly configured
                        {
                            Skin.StartSection("Posting", false);

                            m_EntryPanel.DevLogPostingGUI();

                            bool canPostToAll;
                            if (!string.IsNullOrEmpty(m_EntryPanel.shortText))
                            {
                                canPostToAll = DiscordPostingGUI();
                                canPostToAll &= TwitterPostingGUI();

                                if (canPostToAll)
                                {
                                    if (GUILayout.Button("Post to All"))
                                    {
                                        DevLogEntry entry = PostToDevLogAndTwitter();
                                        Discord.PostEntry(entry);
                                    }
                                }
                            }
                            Skin.EndSection();

                            entryScrollPosition = EditorGUILayout.BeginScrollView(entryScrollPosition);
                            mediaPanel.ScreenCaptures = m_ScreenCaptures;
                            m_EntryPanel.entries = m_DevLogEntries;
                            m_EntryPanel.OnGUI();

                            EditorGUILayout.Space();

                            mediaPanel.CaptureCamera = m_CaptureCamera;
                            mediaPanel.ScreenCaptures = m_ScreenCaptures;
                            mediaPanel.OnGUI();
                            EditorGUILayout.EndScrollView();
                        }
                        else
                        {
                            SettingsTabUI();
                        }
                        break;
                    case 1:
                        if (m_DevLogPanel == null) m_DevLogPanel = new DevLogPanel(m_DevLogEntries);

                        DevLogList.isDirty = true;

                        m_DevLogPanel.ScreenCaptures = m_ScreenCaptures;
                        m_DevLogPanel.entries = m_DevLogEntries;
                        m_DevLogPanel.OnGUI();
                        break;
                    case 2:
                        m_SchedulingPanel.OnGUI();
                        break;
                    case 3:
                        m_GitPanel.OnGUI();
                        break;
                    case 4:
                        SettingsTabUI();
                        break;
                }
            } catch (InvalidCastException e)
            {
                //TODO Don't silently catch errors
                // this is a workaround. An exception is thrown when a new scene is loaded.
                Repaint();
            }
        }

        /// <summary>
        /// Display GUI for posting to DevLog and Discord.
        /// </summary>
        /// <returns>True if it is possible to post to Discord.</returns>
        private bool DiscordPostingGUI()
        {
            if (!DiscordSettings.IsConfigured || !m_EntryPanel.isNewEntry) return false;

            if (!string.IsNullOrEmpty(m_EntryPanel.shortText) && mediaPanel.hasSelectedImages)
            {
                if (GUILayout.Button("Post to Devlog and Discord"))
                {
                    Message message;
                    if (string.IsNullOrEmpty(m_EntryPanel.detailText))
                    {
                        message = new Message(DiscordSettings.Username, m_EntryPanel.shortText + m_EntryPanel.GetSelectedMetaData(false), m_ScreenCaptures);
                    }
                    else
                    {
                        message = new Message(DiscordSettings.Username, m_EntryPanel.shortText + m_EntryPanel.GetSelectedMetaData(false), m_EntryPanel.detailText, mediaPanel.ScreenCaptures);
                    }

                    currentEntry = m_EntryPanel.AppendDevlogEntry(false, true);
                    Discord.PostEntry(currentEntry);
                }
                return true;
            } else
            {
                return false;
            }
        }

        private string TweetText
        {
            get { return m_EntryPanel.shortText + m_EntryPanel.GetSelectedMetaData(); }
        }

        internal void DeleteEntry(DevLogEntry entry)
        {
            if (EditorUtility.DisplayDialog("Delete DevLog Entry?",
                "Are you sure you want delete the entry '" + entry.title
                + "'?", "Delete", "Cancel"))
            {
                m_EntryPanel.entries.RemoveEntry(entry);
                AssetDatabase.RemoveObjectFromAsset(entry);
                EditorUtility.SetDirty(m_EntryPanel.entries);
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Display the GUI for posting to DevLog pluse Twitter.
        /// </summary>
        /// <returns>True if it is possible to post to Twitter.</returns>
        private bool TwitterPostingGUI()
        {
            if (!TwitterSettings.IsConfigured || !m_EntryPanel.isNewEntry) return false;

            if (!string.IsNullOrEmpty(TweetText) && TweetText.Length <= 280 && mediaPanel.hasSelectedImages)
            {
                if (GUILayout.Button("Post DevLog and Tweet"))
                {
                    PostToDevLogAndTwitter();
                    EditorGUILayout.EndHorizontal();
                }
                return true;
            } else
            {
                return false;
            }
        }

        /// <summary>
        /// Post to the DevLog and Twitter.
        /// </summary>
        /// <returns>The new DevLog entry.</returns>
        private DevLogEntry PostToDevLogAndTwitter()
        {
            string m_StatusText = "";
            bool isTweeted = false;

            if (mediaPanel.hasSelectedImages) // Images to post
            {
                List<string> mediaFilePaths = new List<string>();
                //TODO only allowed 4 still or 1 animated GIF
                for (int i = 0; i < mediaPanel.ScreenCaptures.Count; i++)
                {
                    if (mediaPanel.ScreenCaptures.captures[i].IsSelected)
                    {
                        DevLogScreenCapture capture = mediaPanel.ScreenCaptures.captures[i];
                        mediaFilePaths.Add(capture.ImagePath);
                    }
                }

                if (Twitter.PublishTweetWithMedia(TweetText, mediaFilePaths, out string response))
                {
                    m_StatusText = "Tweet with image(s) sent succesfully";
                    isTweeted = true;
                }
                else
                {
                    Debug.LogError(response);
                    m_StatusText = response;
                }
            }
            else
            { // No Images to post
                if (Twitter.PublishTweet(TweetText, out string response))
                {
                    m_StatusText = "Tweet sent succesfully";
                    isTweeted = true;
                }
                else
                {
                    Debug.LogError(response);
                }
            }

            EditorGUILayout.LabelField(m_StatusText);

            if (isTweeted)
            {
                return m_EntryPanel.AppendDevlogEntry(true);
            } else
            {
                return m_EntryPanel.AppendDevlogEntry();
            }
        }

        /// <summary>
        /// Display the settings UI.
        /// </summary>
        public void SettingsTabUI()
        {
            string newPath;

            Skin.StartSection("Capture Storage", false);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Captures Save Folder");
            string originalPath = Settings.CaptureFileFolderPath;
            Settings.CaptureFileFolderPath = EditorGUILayout.TextField(Settings.CaptureFileFolderPath, GUILayout.Height(40));
            if (GUILayout.Button("Browse"))
            {
                newPath = EditorUtility.OpenFolderPanel("Select a folder in which to save captures", Settings.CaptureFileFolderPath, "");
                if (!string.IsNullOrEmpty(newPath))
                {
                    Settings.CaptureFileFolderPath = newPath;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("File Organizaton");
            EditorGUILayout.BeginVertical();
            Settings.OrganizeCapturesByProject = EditorGUILayout.ToggleLeft("Organize in Project sub folders (e.g. 'root/Project')", Settings.OrganizeCapturesByProject);
            Settings.OrganizeCapturesByScene = EditorGUILayout.ToggleLeft("Organize in Scene sub folders (e.g. 'root/Project/Scene')", Settings.OrganizeCapturesByScene);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            Skin.EndSection();

            Skin.StartSection("Capturing", false);
            if (!m_CaptureCamera) m_CaptureCamera = Camera.main;
            if (!m_CaptureCamera)
            {
                EditorGUILayout.LabelField("No main camera in scene, please tag a camera as MainCamera or select a camera here.");
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Camera for captures");
            m_CaptureCamera = (Camera)EditorGUILayout.ObjectField(m_CaptureCamera, typeof(Camera), true);
            EditorGUILayout.EndHorizontal();

            Settings.TrimTabsWhenMaximized = EditorGUILayout.ToggleLeft("Trim tabs from the window when it is maximized", Settings.TrimTabsWhenMaximized);
            Settings.TrimSceneViewToolbar = EditorGUILayout.ToggleLeft("Trim the Toolbar from Scene View", Settings.TrimSceneViewToolbar);
            Settings.TrimGameViewToolbar = EditorGUILayout.ToggleLeft("Trim the Toolbar from Game View", Settings.TrimGameViewToolbar);

            Skin.EndSection();

            Skin.StartSection("Dev Log Objects", false);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Dev Log Storage");
            string existingPath = AssetDatabase.GetAssetPath(m_DevLogEntries);
            m_DevLogEntries = EditorGUILayout.ObjectField(m_DevLogEntries, typeof(DevLogEntries), true) as DevLogEntries;
            if (m_DevLogEntries == null)
            {
                if (GUILayout.Button("Create"))
                {
                    string filename = "Assets/Dev Log " + Application.version + ".asset";
                    m_DevLogEntries = ScriptableObject.CreateInstance<DevLogEntries>();
                    AssetDatabase.CreateAsset(m_DevLogEntries, filename);
                    AssetDatabase.SaveAssets();
                }
            }
            newPath = AssetDatabase.GetAssetPath(m_DevLogEntries);
            if (existingPath != newPath)
            {
                Settings.DevLogScriptableObjectPath = newPath;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Screen Capture Storage");
            existingPath = AssetDatabase.GetAssetPath(m_ScreenCaptures);
            m_ScreenCaptures = EditorGUILayout.ObjectField(m_ScreenCaptures, typeof(DevLogScreenCaptureCollection), true) as DevLogScreenCaptureCollection;
            if (m_ScreenCaptures == null)
            {
                if (GUILayout.Button("Create"))
                {
                    string filename = "Assets/Screen Captures " + Application.version + ".asset";
                    m_ScreenCaptures = ScriptableObject.CreateInstance<DevLogScreenCaptureCollection>();
                    AssetDatabase.CreateAsset(m_ScreenCaptures, filename);
                    AssetDatabase.SaveAssets();
                    Settings.ScreenCaptureScriptableObjectPath = filename;
                }
            }
            newPath = AssetDatabase.GetAssetPath(m_ScreenCaptures);
            if (existingPath != newPath)
            {
                Settings.ScreenCaptureScriptableObjectPath = newPath;
            }
            EditorGUILayout.EndHorizontal();
            Skin.EndSection();

            m_DiscordPanel.OnSettingsGUI();
            
            m_TwitterPanel.OnSettingsGUI();

            DevHelpersGUI();

            EditorGUILayout.LabelField("Welcome to " + Application.productName + " v" + Application.version);

            Skin.EndSection();
        }

        private static void DevHelpersGUI()
        {
            Skin.StartSection("Helpers (Dev Only)", false);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Twitter Access"))
            {
                if (EditorUtility.DisplayDialog("Reset Twitter OAuth Tokens?",
                    "Do you also want to clear the Twitter access tokens?",
                    "Yes", "Do Not Clear Them"))
                {
                    TwitterSettings.ClearAccessTokens();
                }
            }

            if (GUILayout.Button("Reset Meta Data"))
            {
                EntryPanelSettings.ResetMetaData();
            }

            if (GUILayout.Button("Reset All EditorPrefs"))
            {
                if (EditorUtility.DisplayDialog("Reset Everything",
                    "Are you sure you want to reset everything? " +
                    "No data will be deleted, but all settings will be reset. " +
                    "This should only be used if you know what you are doing.\n\n " +
                    "Note, the DevLogger window will be closed, you should reopen it from the `Tools/Wizards Code` menu.",
                    "Yes, Reset",
                    "No, do not reset"))
                {
                    Settings.Reset();
                    EntryPanelSettings.Reset();
                    GitSettings.Reset();
                    TwitterSettings.Reset();
                    DiscordSettings.Reset();
                    window.Close();
                }
            }

            if (GUILayout.Button("Dump Window Names"))
            {
                EditorWindow[] allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
                foreach (EditorWindow window in allWindows)
                {
                    Debug.Log("Window name: " + window);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion

    }
}
 