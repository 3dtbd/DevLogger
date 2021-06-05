using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WizardsCode.Social;

namespace WizardsCode.DevLogger
{
    /// <summary>
    /// The scheduling panel presents and controls the sceduled events.
    /// This allows the user to set a number of scheudled events that
    /// will then be used to ensure they stay on top of their Dev Log
    /// responsibilities.
    /// </summary>
    public class SchedulingPanel
    {
        string[] m_ScheduledEventGUIDs;

        [SerializeField]
        ScheduledEvent[] m_EventsCache;
        DevLogEntries m_Entries;

        float m_CacheTTL = 2;
        float m_CacheUpdateTime = 0;

        public SchedulingPanel(DevLogEntries entries)
        {
            this.m_Entries = entries;
        }

        internal void OnEnable()
        {
            UpdateEventCache();
        }

        private void UpdateEventCache()
        {
            m_ScheduledEventGUIDs = AssetDatabase.FindAssets("t:ScheduledEvent");
            m_EventsCache = new ScheduledEvent[m_ScheduledEventGUIDs.Length];
            for (int i = 0; i < m_ScheduledEventGUIDs.Length; i++)
            {
                m_EventsCache[i] = AssetDatabase.LoadAssetAtPath<ScheduledEvent>(AssetDatabase.GUIDToAssetPath(m_ScheduledEventGUIDs[i]));
            }
            m_CacheUpdateTime = Time.realtimeSinceStartup + m_CacheTTL;
        }

        internal void OnDisable()
        {

        }

        internal void OnGUI()
        {
            if (Time.realtimeSinceStartup > m_CacheUpdateTime)
            {
                UpdateEventCache();
            }

            if (m_EventsCache != null)
            {
                for (int i = 0; i < m_ScheduledEventGUIDs.Length; i++)
                {

                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.LabelField(m_EventsCache[i].name);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Day Of Week");
                    m_EventsCache[i].m_DayOfWeek = (DayOfWeek)EditorGUILayout.EnumPopup(m_EventsCache[i].m_DayOfWeek);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    DateTime dt = DateTime.UtcNow;
                    dt = dt.Date + TimeSpan.FromSeconds(m_EventsCache[i].m_TimeOfDay);
                    int hour = dt.Hour;
                    int minute = dt.Minute;
                    
                    EditorGUILayout.PrefixLabel("Time (24hr)");
                    hour = EditorGUILayout.IntField(hour);
                    minute = EditorGUILayout.IntField(minute);
                    if ((hour >= 0 && hour <=23) && (minute >= 0 && minute <= 59))
                    {
                        m_EventsCache[i].m_TimeOfDay = (int)((hour * 60 * 60) + (minute * 60));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    m_EventsCache[i].m_PostToTwitter = EditorGUILayout.Toggle("Twitter?", m_EventsCache[i].m_PostToTwitter);
                    EditorGUILayout.EndHorizontal();

                    if (m_EventsCache[i].m_PostToTwitter)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Twitter Hashtag");
                        m_EventsCache[i].m_TwitterHashtag = EditorGUILayout.TextField(m_EventsCache[i].m_TwitterHashtag);
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.BeginHorizontal();
                    m_EventsCache[i].m_PostToDiscord = EditorGUILayout.Toggle("Discord?", m_EventsCache[i].m_PostToDiscord);
                    EditorGUILayout.EndHorizontal();

                    List<DevLogEntry> available = m_Entries.GetAvailableSocialEntries();
                    bool posted = false;
                    if (available.Count > 0 && m_EventsCache[i].IsDue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Due Now");
                        if (GUILayout.Button("Post now"))
                        {
                            if (m_EventsCache[i].m_PostToTwitter)
                            {
                                string response;
                                string[] tags = m_EventsCache[i].m_TwitterHashtag.Split('#');
                                foreach (string tag in tags)
                                {
                                    if (string.IsNullOrEmpty(tag.Trim()))
                                    {
                                        continue;
                                    }

                                    if (!m_EventsCache[i].devLogEntry.metaData.Contains(tag.Trim()))
                                    {
                                        m_EventsCache[i].devLogEntry.metaData.Add("#" + tag.Trim());
                                    }
                                }

                                if (Twitter.PublishTweet(m_EventsCache[i].devLogEntry, out response))
                                {
                                    posted = true;
                                } else
                                {
                                    posted = false;
                                    // TODO handle Twitter failure gracefully
                                    Debug.LogError("Failed to post to twitter: " + response);
                                }
                            }

                            if (m_EventsCache[i].m_PostToDiscord)
                            {
                                Discord.PostEntry(m_EventsCache[i].devLogEntry);
                            }
                        }

                        if (posted)
                        {
                            m_EventsCache[i].MarkDone();
                        }

                        EditorGUILayout.EndHorizontal();
                    } else
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Next due");
                        DateTime dueDate = m_EventsCache[i].GetNextDueDateTimeLocal();
                        string dueLabel;
                        if (dueDate < DateTime.Now && dueDate.Day == DateTime.Now.Day && dueDate.Month == DateTime.Now.Month)
                        {
                            dueLabel = "TODAY at  " + dueDate.ToString("HH:mm");
                        } else
                        {
                            dueLabel = dueDate.ToString("dddd MMMM dd, yyyy HH:mm");
                        }
                        EditorGUILayout.LabelField(dueLabel);
                        EditorGUILayout.EndHorizontal();
                    }

                    //m_EventsCache[i].m_DevLogEntry = (DevLogEntry)EditorGUILayout.ObjectField(m_EventsCache[i].m_DevLogEntry, typeof(DevLogEntry), false);

                    if (available.Count > 0)
                    {
                        string[] options = new string[available.Count];
                        int selected = 0;
                        for (int y = 0; y < available.Count; y++)
                        {
                            options[y] = available[y].title;
                            if (available[y] == m_EventsCache[i].devLogEntry)
                            {
                                selected = y;
                            }
                        }
                        selected = EditorGUILayout.Popup("DevLog Entry", selected, options);

                        EditorGUILayout.PrefixLabel("Dev Log Entry");
                        SerializedObject obj = new SerializedObject(m_EventsCache[i]);
                        obj.Update();

                        m_EventsCache[i].devLogEntry = available[selected];
                        SerializedProperty entry = obj.FindProperty("m_DevLogEntry");
                        if (entry != null)
                        {
                            EditorGUILayout.PropertyField(entry);
                        }
                        EditorGUILayout.EndVertical();
                        EditorUtility.SetDirty(m_EventsCache[i]);
                        //AssetDatabase.SaveAssets();
                        obj.ApplyModifiedProperties();
                    } else
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("No Dev Log entries available.\nEnsure there is an entry with the 'Social' flag set to true that has not already been shared on social media.", EditorStyles.helpBox);
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        internal void Update()
        {
            
        }
    }
}