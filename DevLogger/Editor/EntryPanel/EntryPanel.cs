using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using WizardsCode.EditorUtils;

namespace WizardsCode.DevLogger
{
    /// <summary>
    /// The GUI Panel for a new DevLog Entry
    /// </summary>
    [Serializable]
    public class EntryPanel
    {
        [SerializeField] Vector2 windowScrollPos = Vector2.zero;
        [SerializeField] internal string shortText = "";
        [SerializeField] internal DevLogEntry.Status status;
        private Vector2 detailScrollPosition;
        [SerializeField] internal string detailText = "";
        [SerializeField] bool isSocial = false;
        [SerializeField] string gitCommit = "";
        [SerializeField] string newMetaDataItem;

        internal bool isNewEntry = true;

        private DevLoggerWindow m_DevLogWindow;
        internal DevLoggerWindow devLogWindow
        {
            get
            {
                if (m_DevLogWindow == null)
                {
                    m_DevLogWindow = EditorWindow.GetWindow(typeof(DevLoggerWindow)) as DevLoggerWindow;
                }
                return m_DevLogWindow;
            }
        }

        public EntryPanel(DevLogEntries entries)
        {
            this.entries = entries;
        }

        internal DevLogEntries entries { get; set; }

        internal DevLogScreenCaptureCollection ScreenCaptures
        {
            get
            {
                if (m_ScreenCaptures == null)
                {
                    m_ScreenCaptures = AssetDatabase.LoadAssetAtPath(Settings.ScreenCaptureScriptableObjectPath, typeof(DevLogScreenCaptureCollection)) as DevLogScreenCaptureCollection;
                }
                return m_ScreenCaptures;
            }
        }

        bool m_IsAssetManagerPresent = false;
        float m_TimeOfNextCheckForAssetManager = 0;
        private DevLogScreenCaptureCollection m_ScreenCaptures;

        internal void Populate(string hash, string description)
        {
            shortText = description;
            gitCommit = hash;
        }

        public void OnGUI()
        {
            //windowScrollPos = EditorGUILayout.BeginScrollView(windowScrollPos);
            Skin.StartSection("Log Entry", false);
            if (GUILayout.Button("New Entry"))
            {
                isNewEntry = true;
                windowScrollPos = Vector2.zero;
                shortText = "";
                status = DevLogEntry.Status.Idea;
                detailScrollPosition = Vector2.zero;
                detailText = "";
                isSocial = false;
                gitCommit = "";
                newMetaDataItem = "";
            }

            LogEntryGUI();
            Skin.EndSection();

            Skin.StartSection("Meta Data", false);
            MetaDataGUI();
            Skin.EndSection();
            //EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Get a string containing all the selected hashtags for this tweet.
        /// </summary>
        /// <returns>Space separated list of hashtags</returns>
        public string GetSelectedMetaData(bool includeHashtags = true)
        {
            StringBuilder sb = new StringBuilder();
            MetaDataItems items = EntryPanelSettings.GetSuggestedMetaDataItems();
            for (int i = 0; i < items.Count; i++)
            {
                if (items.GetItem(i).IsSelected && (includeHashtags || !items.GetItem(i).name.StartsWith("#")))
                {
                    sb.Append(" ");
                    sb.Append(items.GetItem(i).name);
                }
            }
            return sb.ToString();
        }

        private void LogEntryGUI()
        {
            status = (DevLogEntry.Status)EditorGUILayout.EnumPopup("Status", status, GUILayout.Height(35));
            
            EditorStyles.textField.wordWrap = true;
            EditorGUILayout.LabelField(EntryPanelSettings.guiShortTextLabel);
            shortText = EditorGUILayout.TextArea(shortText, GUILayout.Height(35));

            EditorGUILayout.LabelField("Long Entry (optional)");
            detailScrollPosition = GUILayout.BeginScrollView(detailScrollPosition);
            detailText = EditorGUILayout.TextArea(detailText, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
        }

        private void MetaDataGUI()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            MetaDataItems items = EntryPanelSettings.GetSuggestedMetaDataItems();
            for (int i = 0; i < items.Count; i++)
            {
                MetaDataItem item = items.GetItem(i);
                item.IsSelected = EditorGUILayout.ToggleLeft(item.name, item.IsSelected);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(EntryPanelSettings.guiNewMetaDataLabel);
            newMetaDataItem = EditorGUILayout.TextField(newMetaDataItem);
            if (GUILayout.Button("Add"))
            {
                EntryPanelSettings.AddSuggestedMetaDataItem(new MetaDataItem(newMetaDataItem, true));
                newMetaDataItem = "";
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            isSocial = EditorGUILayout.Toggle(EntryPanelSettings.guiSocialLabel, isSocial);
            gitCommit = EditorGUILayout.TextField(EntryPanelSettings.guiGitCommitLabel, gitCommit);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

#if DOUBTECH_ASSET_MANAGER
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Credits from Asset Manager"))
            {
                detailText += new DoubTech.AssetManager.Api.Credits.CreditGenerator().GenerateCredits();
            }
            EditorGUILayout.EndHorizontal();
#endif
        }

        internal void DevLogPostingGUI()
        {
            if (!string.IsNullOrEmpty(shortText))
            {
                EditorGUILayout.BeginHorizontal();
                if (isNewEntry)
                {
                    if (GUILayout.Button("Post Devlog Only"))
                    {
                        AppendDevlogEntry();
                    }
                } else
                {
                    if (GUILayout.Button("Update Devlog"))
                    {
                        UpdateDevLogEntry();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("No valid actions at this time.");
            }
        }

        internal void EditEntry(DevLogEntry entry)
        {
            isNewEntry = false;
            windowScrollPos = Vector2.zero;
            status = entry.status;
            detailScrollPosition = Vector2.zero;
            shortText = entry.shortDescription;
            detailText = entry.longDescription;
            isSocial = entry.isSocial;
            gitCommit = entry.commitHash;

            MetaDataItems items = EntryPanelSettings.GetSuggestedMetaDataItems();
            for (int i = 0; i < items.Count; i++)
            {
                if (devLogWindow.currentEntry.metaData.Contains(items.GetItem(i).name))
                {
                    items.GetItem(i).IsSelected = true;
                }
            }

            List<string> mediaFilePaths = new List<string>();
            for (int i = 0; i < ScreenCaptures.Count; i++)
            {
                DevLogScreenCapture capture = ScreenCaptures.captures[i];
                if (devLogWindow.currentEntry.captures.Contains(capture))
                {
                    ScreenCaptures.captures[i].IsSelected = true;
                }
            }
        }

        public void UpdateDevLogEntry()
        {
            devLogWindow.currentEntry.status = status;

            devLogWindow.currentEntry.shortDescription = shortText;
            devLogWindow.currentEntry.isSocial = isSocial;

            devLogWindow.currentEntry.commitHash = gitCommit;
            
            MetaDataItems items = EntryPanelSettings.GetSuggestedMetaDataItems();
            for (int i = 0; i < items.Count; i++)
            {
                if (!items.GetItem(i).IsSelected)
                {
                    devLogWindow.currentEntry.metaData.Remove(items.GetItem(i).name);
                } 
                else if (items.GetItem(i).IsSelected && !devLogWindow.currentEntry.metaData.Contains(items.GetItem(i).name))
                {
                    devLogWindow.currentEntry.metaData.Add(items.GetItem(i).name);
                }
            }

            List<string> mediaFilePaths = new List<string>();
            for (int i = 0; i < ScreenCaptures.Count; i++)
            {
                DevLogScreenCapture capture = ScreenCaptures.captures[i];
                if (!ScreenCaptures.captures[i].IsSelected)
                {
                    devLogWindow.currentEntry.captures.Remove(capture);
                } else if (ScreenCaptures.captures[i].IsSelected && !devLogWindow.currentEntry.captures.Contains(capture))
                {
                    devLogWindow.currentEntry.captures.Add(capture);
                    ScreenCaptures.captures[i].IsSelected = false;
                }
            }
            devLogWindow.currentEntry.longDescription = detailText;

            EditorUtility.SetDirty(entries);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Append a devlog entry.
        /// </summary>
        /// <param name="withTweet">If true record that the entry was tweeted at the current time.</param>
        /// <param name="withDiscord">If true record that the entry was posted to discord at the current time.</param>
        /// <returns></returns>
        public DevLogEntry AppendDevlogEntry(bool withTweet = false, bool withDiscord = false)
        {
            DevLogEntry entry = ScriptableObject.CreateInstance<DevLogEntry>();
            entry.name = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();

            entry.status = status;

            entry.shortDescription = shortText;
            StringBuilder text = new StringBuilder(entry.shortDescription);

            entry.isSocial = isSocial;

            if (!string.IsNullOrEmpty(gitCommit))
            {
                entry.commitHash = gitCommit;

                text.Append("\n\nGit Commit: " + gitCommit);
                gitCommit = "";
            }

            MetaDataItems items = EntryPanelSettings.GetSuggestedMetaDataItems();
            for (int i = 0; i < items.Count; i++)
            {
                if (items.GetItem(i).IsSelected) entry.metaData.Add(items.GetItem(i).name);
            }

            if (withTweet)
            {
                entry.tweeted = true;
                entry.lastTweetFileTime = DateTime.Now.ToFileTimeUtc();
                text.Append("\n\n[This DevLog entry was last Tweeted at " + entry.lastTweetPrettyTime + ".]");
            }

            if (withDiscord)
            {
                entry.discordPost = true;
                entry.lastDiscordPostFileTime = DateTime.Now.ToFileTimeUtc();
                text.Append("\n\n[This DevLog entry was last posted to Discord at " + entry.lastTweetPrettyTime + ".]");
            }

            List<string> mediaFilePaths = new List<string>();
            for (int i = 0; i < ScreenCaptures.Count; i++)
            {
                if (ScreenCaptures.captures[i].IsSelected)
                {
                    DevLogScreenCapture capture = ScreenCaptures.captures[i];
                    entry.captures.Add(capture);
                    ScreenCaptures.captures[i].IsSelected = false;
                }
            }
            entry.longDescription = detailText;

            entries.AddEntry(entry);
            AssetDatabase.AddObjectToAsset(entry, entries);
            EditorUtility.SetDirty(entries);
            AssetDatabase.SaveAssets();

            shortText = "";
            detailText = "";

            return entry;
        }
    }
}
