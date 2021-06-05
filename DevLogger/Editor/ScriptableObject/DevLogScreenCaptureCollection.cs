using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.DevLogger
{
    /// <summary>
    /// A collection of screen captures used in a DevLog.
    /// </summary>
    public class DevLogScreenCaptureCollection : ScriptableObject
    {
        [SerializeField]
        public List<DevLogScreenCapture> captures = new List<DevLogScreenCapture>();

        /// <summary>
        /// Get a count of the number of ScreenCaptures in this collection.
        /// </summary>
        public int Count { get { return captures.Count; } }

        public int SelectedCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Count; i++)
                {
                    count += captures[i].IsSelected ? 1 : 0;
                }
                return count;
            }
        }
    }
}
