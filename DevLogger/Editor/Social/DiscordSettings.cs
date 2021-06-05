using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace WizardsCode.DevLogger
{
    public static class DiscordSettings
    {
        const string EDITOR_PREFS_DISCORD_IS_CONFIGURED = "DiscordIsConfigured_";
        const string EDITOR_PREFS_DISCORD_USERNAME = "DiscordUsername_";
        const string EDITOR_PREFS_DISCORD_WEBHOOK_URL = "DiscordWebhookURL_";

        public static string Username
        {
            get { return EditorPrefs.GetString(EDITOR_PREFS_DISCORD_USERNAME + Application.productName, "Dev Logger (test)"); }
            set { EditorPrefs.SetString(EDITOR_PREFS_DISCORD_USERNAME + Application.productName, value); }
        }

        public static string WebHookURL
        {
            get { return EditorPrefs.GetString(EDITOR_PREFS_DISCORD_WEBHOOK_URL + Application.productName); }
            set { EditorPrefs.SetString(EDITOR_PREFS_DISCORD_WEBHOOK_URL + Application.productName, value); }
        }

        public static bool IsConfigured
        {
            get { return !string.IsNullOrEmpty(WebHookURL) && !string.IsNullOrEmpty(Username); }
        }

        public static void Reset()
        {
            EditorPrefs.DeleteKey(EDITOR_PREFS_DISCORD_USERNAME + Application.productName);
            EditorPrefs.DeleteKey(EDITOR_PREFS_DISCORD_WEBHOOK_URL + Application.productName);
            EditorPrefs.DeleteKey(EDITOR_PREFS_DISCORD_IS_CONFIGURED + Application.productName);
        }
    }
}
