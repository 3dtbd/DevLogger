using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.DevLogger
{
    [CreateAssetMenu(fileName = "Dev Log Scheduled Event", menuName = "Wizards Code/Dev Log Scheduled Event")]
    public class ScheduledEvent : ScriptableObject
    {
        public enum ScheduleType { DayOfWeek }

        [SerializeField, Tooltip("The type of schedule this is, e.g. day of week, day of month, every x days etc.")]
        internal ScheduleType m_ScheduleType = ScheduleType.DayOfWeek;
        [SerializeField, Tooltip("The day of the week that this event should occur on.")]
        internal DayOfWeek m_DayOfWeek = DayOfWeek.Saturday;
        [SerializeField, Tooltip("The time of day the event should occur, in seconds past midnight, UTC")]
        internal int m_TimeOfDay = 68400; // 7pm

        [Header("Twitter")]
        [SerializeField, Tooltip("Whether to post to Twitter on this schedule.")]
        internal bool m_PostToTwitter = true;
        [SerializeField, Tooltip("The twitter hashtags to use when posting to Twitter on this schedule. This will be in addition to any hashtags defined in the post itself.")]
        internal string m_TwitterHashtag = "";

        [Header("Discord")]
        [SerializeField, Tooltip("Whether to post to Discord on this schedule.")]
        internal bool m_PostToDiscord = true;

        [SerializeField]
        internal string m_DevLogGUID;        

        [SerializeField]
        internal long m_LastDoneDateTime;

        [HideInInspector]
        public DevLogEntry m_DevLogEntry; // TODO Hmmm.. this needs to be public so FindProperty in SchedulingPanel works :-(
        internal DevLogEntry devLogEntry
        {
            get
            {
                if (m_DevLogEntry == null)
                {
                    m_DevLogEntry = AssetDatabase.LoadAssetAtPath<DevLogEntry>(AssetDatabase.GUIDToAssetPath(m_DevLogGUID));
                }
                return m_DevLogEntry;
            }
            set
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(value, out string guid, out long localId)) {
                    m_DevLogEntry = value;
                    m_DevLogGUID = guid;
                }
            }
        }

        internal bool IsDue
        {
            get
            {
                return DateTime.UtcNow.ToFileTimeUtc() >= GetNextDueDateTimeLocal().ToFileTimeUtc();
            }
        }

        internal DateTime GetNextDueDateTimeLocal()
        {
            DateTime dt;
            if (m_LastDoneDateTime == 0)
            {
                dt = DateTime.Now;
            } else {
                dt = DateTime.FromFileTimeUtc(m_LastDoneDateTime).ToLocalTime();
            }

            int daysUntilNext = ((int)m_DayOfWeek - (int)dt.DayOfWeek + 7) % 7;
            dt = dt.AddDays(daysUntilNext);

            dt = dt.Date.AddSeconds(m_TimeOfDay);

            return dt;
        }

        internal void MarkDone()
        {
            m_LastDoneDateTime = DateTime.UtcNow.ToFileTimeUtc();
        }
    }
}
