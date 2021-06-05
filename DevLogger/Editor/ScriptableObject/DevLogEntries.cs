using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WizardsCode.DevLogger;

namespace WizardsCode.DevLogger
{
    public class DevLogEntries : ScriptableObject
    {
        [SerializeField, Tooltip("The entries in this Dev Log.")]
        List<DevLogEntry> m_Entries = new List<DevLogEntry>();

        internal void AddEntry(DevLogEntry entry)
        {
            m_Entries.Add(entry);
        }

        internal DevLogEntry GetEntry(int index)
        {
            return m_Entries[index];
        }

        internal List<DevLogEntry> GetEntries()
        {
            return m_Entries;
        }

        /// <summary>
        /// Get all the elements in this set of entries that are of a specified status.
        /// </summary>
        /// <param name="status">The status we want to retrieve.</param>
        /// <returns>A list of entries with the given status</returns>
        internal List<DevLogEntry> GetEntries(DevLogEntry.Status status)
        {
            return (List<DevLogEntry>)Enumerable.ToList(m_Entries.Where(l => l.status == status));
        }

        /// <summary>
        /// Get list of all Entries that are marked for social sharing
        /// but have not yet been shared socially.
        /// </summary>
        /// <returns></returns>
        internal List<DevLogEntry> GetAvailableSocialEntries()
        {
            // TODO is LINQ faster?
            List<DevLogEntry>  results = new List<DevLogEntry>();
            for (int i = 0; i < m_Entries.Count; i++)
            {
                if (m_Entries[i].isSocial && !m_Entries[i].tweeted && !m_Entries[i].discordPost) {
                    results.Add(m_Entries[i]);
                }
            }
            return results;
        }

        internal void RemoveEntry(DevLogEntry entry)
        {
            m_Entries.Remove(entry);
        }
    }
}
