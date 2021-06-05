using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text;

namespace WizardsCode.DevLogger
{
    public class EntryPanelSettings
    {
        #region GUI
        static internal GUIContent guiShortTextLabel = new GUIContent("Short Descrption (required)", "The title of the DevLog entry. Also used as for Tweet content.");
        static internal GUIContent guiLongTextLabel = new GUIContent("Long Descrption", "The main body of the devlog. This will also be included in Discord posts.");
        static internal GUIContent guiSocialLabel = new GUIContent("Use for social?", "Should this be used for social amplificaton?");
        static internal GUIContent guiGitCommitLabel = new GUIContent("Git Commit Hash", "The hash of the Git commit this devlog relates to.");
        static internal GUIContent guiNewMetaDataLabel = new GUIContent("New Meta Data Label", "Add a new Meta Data label that can be optionally applied to this and future DevLog entries.");
        #endregion

        #region MetaData
        private static MetaDataItems m_CachedMetaData;

        public static int MetaDataItemCount
        {
            get { return GetSuggestedMetaDataItems().Count; }
        }

        public static MetaDataItems GetSuggestedMetaDataItems()
        {
            if (m_CachedMetaData == null)
            {
                string json = EditorPrefs.GetString("suggestedMetaData_" + Application.productName, "{}");

                m_CachedMetaData = JsonUtility.FromJson<MetaDataItems>(json);
                if (m_CachedMetaData == null || m_CachedMetaData.Count == 0)
                {
                    m_CachedMetaData = new MetaDataItems();
                    m_CachedMetaData.Add(new MetaDataItem("#MadeWithUnity", false));
                    SetSuggestedMetaDataItems(m_CachedMetaData);
                }
            }
            return m_CachedMetaData;
        }

        public static void SetSuggestedMetaDataItems(MetaDataItems data)
        {
            m_CachedMetaData = data;
            EditorPrefs.SetString("suggestedMetaData_" + Application.productName, JsonUtility.ToJson(data)) ;
        }

        internal static void SaveSuggestedMetaDataItems()
        {
            SetSuggestedMetaDataItems(m_CachedMetaData);
        }

        public static void AddSuggestedMetaDataItem(MetaDataItem item)
        {
            MetaDataItems items = GetSuggestedMetaDataItems();
            items.Add(item);
            SetSuggestedMetaDataItems(items);
        }

        internal static void ResetMetaData()
        {
            EditorPrefs.SetString("suggestedMetaData_" + Application.productName, "");
        }
        #endregion

        public static void Reset()
        {
            ResetMetaData();
        }
    }

    [Serializable]
    public class MetaDataItem {
        public string name;
        bool m_IsSelected;

        public bool IsSelected
        {
            get { return m_IsSelected; }
            set { if (m_IsSelected != value)
                {
                    m_IsSelected = value;
                    EntryPanelSettings.SaveSuggestedMetaDataItems();
                } 
            }
        }

        public MetaDataItem(string name, bool isSelected)
        {
            this.name = name;
            this.m_IsSelected = isSelected;
        }
    }

    [Serializable]
    public class MetaDataItems
    {
        public List<MetaDataItem> items = new List<MetaDataItem>();

        public int Count { get { return items == null ? 0 : items.Count; } }

        public void Add (MetaDataItem item)
        {
            items.Add(item);
        }

        public MetaDataItem GetItem(int index)
        {
            return items[index];
        }
    }
}
