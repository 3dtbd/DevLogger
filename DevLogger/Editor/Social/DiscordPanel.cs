using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.Net;
using System.Collections.Specialized;
using System.IO;

namespace WizardsCode.DevLogger
{
    [Serializable]
    public class DiscordPanel
    {
        [SerializeField] bool showDiscordSettings = false;

        static UnityWebRequest www;

        public void OnSettingsGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            showDiscordSettings = EditorGUILayout.Foldout(showDiscordSettings, "Discord Settings", EditorStyles.foldout);
            if (showDiscordSettings)
            {
                EditorGUILayout.PrefixLabel("Bot Username");
                DiscordSettings.Username = EditorGUILayout.TextField(DiscordSettings.Username);

                EditorGUILayout.PrefixLabel("Webhook URL");
                DiscordSettings.WebHookURL = EditorGUILayout.TextField(DiscordSettings.WebHookURL);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
