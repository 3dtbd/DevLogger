using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.DevLogger
{
    /// <summary>
    /// A single Log entry for a DevLog.
    /// </summary>
    public class DevLogEntry : ScriptableObject
    {
        public enum Status { Idea = 0, ToDo = 10, InProgress = 20, Testing = 30, Done = 40, Social = 50 }
        [SerializeField, Tooltip("The current status of this entry.")]
        public Status status = Status.Idea;
        [SerializeField, Tooltip("The date and time the log entry was created.")]
        public DateTime created = DateTime.Now;
        [SerializeField, Tooltip("The short description of this entry used as the summary for an entry in both the log an dsocial media content.")]
        public string shortDescription = "";
        [SerializeField, Tooltip("The long description of this entry used as the detail for a Dev Log entry.")]
        [TextArea(6, 12)]
        public string longDescription = "";
        [SerializeField, Tooltip("The meta data such as links and hashtags associated with this entry.")]
        public List<string> metaData = new List<string>();
        [SerializeField, Tooltip("A list of the screen captures for this entry")]
        public List<DevLogScreenCapture> captures = new List<DevLogScreenCapture>();
        [SerializeField, Tooltip("The commit hash related to this change in the project.")]
        public string commitHash = "";
        [SerializeField, Tooltip("Whether or not this entry is intended for posting to social media.")]
        public bool isSocial = false;
        [SerializeField, Tooltip("Whether this entry has every been tweeted.")]
        public bool tweeted;
        [SerializeField, Tooltip("The date and time (UTC file time) this entry was last tweeted.")]
        public long lastTweetFileTime;
        [SerializeField, Tooltip("Whether this entry has every been posted to Discord.")]
        public bool discordPost;
        [SerializeField, Tooltip("The date and time (UTC file time) this entry was last tweeted.")]
        public long lastDiscordPostFileTime;

        /// <summary>
        /// The title is the first line or sentence of the short description.
        /// If there is no line break or period then the whole of the short description
        /// is returned.
        /// </summary>
        public string title
        {
            get {
                int lineBreak = shortDescription.IndexOf("\n");
                int period = shortDescription.IndexOf(".");
                if (lineBreak > 0)
                {
                    return shortDescription.Substring(0, lineBreak + 1).Trim();
                }
                else if (period > 0)
                {
                    return shortDescription.Substring(0, period + 1).Trim();
                }
                return shortDescription.Trim();
            }
        }

        public string lastTweetPrettyTime { 
            get
            {
                return DateTime.FromFileTimeUtc(lastTweetFileTime).ToLocalTime().ToString("dddd dd-MMM-yyyy HH:MM");
            }
        }
        public string lastDiscordPostPrettyTime
        {
            get
            {
                return DateTime.FromFileTimeUtc(lastDiscordPostFileTime).ToLocalTime().ToString("dddd dd-MMM-yyyy HH:MM");
            }
        }
    }
}
