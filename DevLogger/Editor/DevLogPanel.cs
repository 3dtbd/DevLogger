using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using WizardsCode.Social;

namespace WizardsCode.DevLogger
{
    public class DevLogPanel
    {
        DevLogList ideaList;
        DevLogList todoList;
        DevLogList inProgressList;
        DevLogList testingList;
        DevLogList doneList;
        DevLogList socialList;
        Vector2 listScrollPosition;

        internal DevLogEntries entries { get; set; }

        private DevLoggerWindow devLoggerWindow;

        public DevLogPanel(DevLogEntries entries)
        {
            this.entries = entries;
            devLoggerWindow = EditorWindow.GetWindow(typeof(DevLoggerWindow)) as DevLoggerWindow;
        }

        internal DevLogScreenCaptureCollection ScreenCaptures { get; set; }

        float timeToUpdateLists = 0;
        public void OnGUI()
        {
            if (timeToUpdateLists <= 0)
            {
                ConfigureReorderableDevLogLists();
                timeToUpdateLists = 1;
            } else
            {
                timeToUpdateLists -= Time.deltaTime;
            }

            if (doneList == null)
            {
                EditorGUILayout.LabelField("Setup your log in the 'Settings' Tab.");
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("View Tasks Markdown", GUILayout.Height(30)))
                {
                    DevLogMarkdown.WriteTaskLog(entries);
                    string filepath = DevLogMarkdown.GetAbsoluteProjectDirectory() + DevLogMarkdown.GetRelativeCurrentFilePath();
                    System.Diagnostics.Process.Start(filepath);
                }
                if (GUILayout.Button("View Devlog Markdown", GUILayout.Height(30)))
                {
                    DevLogMarkdown.WriteDevLog(entries);
                    string filepath = DevLogMarkdown.GetAbsoluteProjectDirectory() + DevLogMarkdown.GetRelativeCurrentFilePath();
                    System.Diagnostics.Process.Start(filepath);
                }
                EditorGUILayout.EndHorizontal();

                if (doneList.index >= 0)
                {
                    string response;
                    if (TwitterSettings.IsConfigured)
                    {
                        if (GUILayout.Button("Tweet Selected Entry", GUILayout.Height(30)))
                        {
                            if (Twitter.PublishTweet(entries.GetEntry(doneList.index), out response))
                            {
                                entries.GetEntry(doneList.index).tweeted = true;
                                entries.GetEntry(doneList.index).lastTweetFileTime = DateTime.Now.ToFileTimeUtc();
                            }
                            else
                            {
                                // TODO Handle failed tweet gracefully
                                Debug.LogWarning("Tweet failed. Not currently handling this gracefully. Response " + response);
                            }
                        }
                    }

                    if (DiscordSettings.IsConfigured)
                    {
                        if (GUILayout.Button("Post selected to Discord", GUILayout.Height(30)))
                        {
                            Discord.PostEntry(entries.GetEntry(doneList.index));
                            entries.GetEntry(doneList.index).discordPost = true;
                            entries.GetEntry(doneList.index).lastDiscordPostFileTime = DateTime.Now.ToFileTimeUtc();
                        }
                    }
                }

                listScrollPosition = EditorGUILayout.BeginScrollView(listScrollPosition);

                if (inProgressList.count > 0)
                {
                    inProgressList.DoLayoutList();
                }
                if (todoList.count > 0)
                {
                    todoList.DoLayoutList();
                }
                if (ideaList.count > 0)
                {
                    ideaList.DoLayoutList();
                }
                if (testingList.count > 0)
                {
                    testingList.DoLayoutList();
                }
                if (doneList.count > 0)
                {
                    doneList.DoLayoutList();
                }
                if (socialList.count > 0)
                {
                    socialList.DoLayoutList();
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private DevLogEntries lastEntriesList;
        private void ConfigureReorderableDevLogLists()
        {
            if (DevLogList.isDirty || entries != lastEntriesList)
            {
                ideaList = ConfigureList(DevLogEntry.Status.Idea);
                todoList = ConfigureList(DevLogEntry.Status.ToDo);
                inProgressList = ConfigureList(DevLogEntry.Status.InProgress);
                testingList = ConfigureList(DevLogEntry.Status.Testing);
                doneList = ConfigureList(DevLogEntry.Status.Done);
                socialList = ConfigureList(DevLogEntry.Status.Social);

                lastEntriesList = entries;
            }
            else if (entries == null)
            {
                doneList = null;
            }
        }

        private DevLogList ConfigureList(DevLogEntry.Status status)
        {
            DevLogList list = new DevLogList(entries, status);

            list.drawElementCallback = list.DrawLogListElement;
            list.drawHeaderCallback = list.DrawHeader;
            list.elementHeightCallback = list.ElementHeightCallback;
            list.onReorderCallback = list.SaveReorderedList;
            list.onSelectCallback = list.OnSelect;
            list.displayAdd = false;

            return list;
        }
    }
}
