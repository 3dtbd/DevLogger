using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace WizardsCode.DevLogger {
    using System.Linq;
    using UnityEditor;
    /// <summary>
    /// DevLogList extends the UnityEditor ReorderableList to provide a way to manage
    /// lists of DevLogEntries in the UI.
    /// </summary>
    public class DevLogList : ReorderableList
    {
        const float listLabelWidth = 80;
        const int listDescriptionLines = 6;

        public static bool isDirty = false;

        string title;
        private DevLoggerWindow devLoggerWindow;
        DevLogEntries allEntries;
        List<DevLogEntry> validEntries;

        public DevLogList(DevLogEntries elements, DevLogEntry.Status status) 
            : base(elements.GetEntries(status), typeof(DevLogEntry), true, true, true, true)
        {
            allEntries = elements;
            
            validEntries = elements.GetEntries(status);
            this.list = elements.GetEntries(status);

            title = status.ToString();

            devLoggerWindow = EditorWindow.GetWindow(typeof(DevLoggerWindow)) as DevLoggerWindow;
        }
        
        internal void SaveReorderedList(ReorderableList list)
        {
            EditorUtility.SetDirty(allEntries);
            AssetDatabase.SaveAssets();
        }

        internal void OnSelect(ReorderableList list)
        {
            DevLogEntry entry = validEntries[list.index];
            EditorGUIUtility.PingObject(entry);
            Selection.activeObject = entry;
        }

        internal float ElementHeightCallback(int index)
        {
            float height = EditorGUIUtility.singleLineHeight; // status
            height += EditorGUIUtility.singleLineHeight;// Title
            height += EditorGUIUtility.singleLineHeight;// Date
            height += EditorGUIUtility.singleLineHeight * listDescriptionLines; // descrption
            height += EditorGUIUtility.singleLineHeight * validEntries[index].metaData.Count; // meta data
            height += EditorGUIUtility.singleLineHeight * validEntries[index].captures.Count; // capture
            height += EditorGUIUtility.singleLineHeight;// Commit Hash
            height += EditorGUIUtility.singleLineHeight;// Social Flag
            height += EditorGUIUtility.singleLineHeight;// Tweeted
            height += EditorGUIUtility.singleLineHeight;// Discord
            height += EditorGUIUtility.singleLineHeight;// Buttons
            height += 10; // space
            return height;
        }
        internal void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, title);
        }

        internal void DrawLogListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            DevLogEntry entry = validEntries[index];

            Rect labelRect = new Rect(rect.x, rect.y, listLabelWidth, EditorGUIUtility.singleLineHeight);
            Rect fieldRect = new Rect(labelRect.x, labelRect.y, rect.width, EditorGUIUtility.singleLineHeight);
            
            DevLogEntry.Status originalStatus = entry.status;
            entry.status = (DevLogEntry.Status)EditorGUI.EnumPopup(fieldRect, entry.status);
            if (originalStatus != entry.status)
            {
                isDirty = true;
            }

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Title"));
            fieldRect = new Rect(fieldRect.x + listLabelWidth, labelRect.y, fieldRect.width - listLabelWidth, labelRect.height);
            entry.shortDescription = EditorGUI.TextField(fieldRect, entry.shortDescription);

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Description"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight * listDescriptionLines);
            entry.longDescription = EditorGUI.TextArea(fieldRect, entry.longDescription);

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * listDescriptionLines, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Created"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, labelRect.height);
            EditorGUI.LabelField(fieldRect, entry.created.ToString("dd MMM yyyy"));

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            if (validEntries[index].metaData.Count != 0)
            {
                EditorGUI.PrefixLabel(labelRect, new GUIContent("Meta Data"));
                for (int i = 0; i < entry.metaData.Count; i++)
                {
                    fieldRect = new Rect(fieldRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * i, fieldRect.width, EditorGUIUtility.singleLineHeight);
                    entry.metaData[i] = EditorGUI.TextField(fieldRect, entry.metaData[i]);
                }
            }

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * validEntries[index].metaData.Count, labelRect.width, labelRect.height);
            if (validEntries[index].captures.Count != 0)
            {
                EditorGUI.PrefixLabel(labelRect, new GUIContent("Captures"));
                for (int i = 0; i < entry.captures.Count; i++)
                {
                    fieldRect = new Rect(fieldRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * i, fieldRect.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.ObjectField(fieldRect, entry.captures[i], typeof(ScreenCapture), false);
                }
            }

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * validEntries[index].captures.Count, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Commit"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.TextArea(fieldRect, entry.commitHash);

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Social?"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
            entry.isSocial = EditorGUI.Toggle(fieldRect, entry.isSocial);
            
            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Tweeted?"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.Toggle(fieldRect, entry.tweeted);
            if (entry.tweeted)
            {
                Rect timeRect = new Rect(fieldRect.x + 20, fieldRect.y, fieldRect.width - 50, fieldRect.height);
                EditorGUI.LabelField(timeRect, " most recent " + entry.lastTweetPrettyTime);
            }

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Discord?"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.Toggle(fieldRect, entry.discordPost);
            if (entry.discordPost)
            {
                Rect timeRect = new Rect(fieldRect.x + 20, fieldRect.y, fieldRect.width - 50, fieldRect.height);
                EditorGUI.LabelField(timeRect, "most recent" + entry.lastDiscordPostPrettyTime);
            }

            float width = fieldRect.width / 2;
            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            fieldRect = new Rect(fieldRect.x, labelRect.y, width, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(fieldRect, "Edit"))
            {
                devLoggerWindow.currentEntry = entry;
                devLoggerWindow.EditCurrentEntry();
            }

            fieldRect = new Rect(fieldRect.x + width, labelRect.y, fieldRect.width / 2, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(fieldRect, "Delete"))
            {
                devLoggerWindow.DeleteEntry(entry);
            }
        }
    }
}
