using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using WizardsCode.Social;

namespace WizardsCode.DevLogger
{
    public static class TwitterSettings
    {
        internal const string EDITOR_PREFS_TWITTER_USER_ID = "TwitterUserID";
        internal const string EDITOR_PREFS_TWITTER_USER_SCREEN_NAME = "TwitterUserScreenName";
        internal const string EDITOR_PREFS_TWITTER_API_KEY = "TwitterAPIKey";
        internal const string EDITOR_PREFS_TWITTER_API_SECRET = "TwitterAPISecret";
        internal const string EDITOR_PREFS_TWITTER_ACCESS_TOKEN = "TwitterAccessToken";
        internal const string EDITOR_PREFS_TWITTER_ACCESS_SECRET = "TwitterAccessSecret";

        internal const string PostTweetURL = "https://api.twitter.com/1.1/statuses/update.json";
        internal const string UploadMediaURL = "https://upload.twitter.com/1.1/media/upload.json";
        internal const string VerifyCredentialsURL = "https://api.twitter.com/1.1/account/verify_credentials.json";

        public static string ApiKey
        {
            get { return EditorPrefs.GetString(EDITOR_PREFS_TWITTER_API_KEY); }
            set { EditorPrefs.SetString(EDITOR_PREFS_TWITTER_API_KEY, value); }
        }

        public static string ApiSecret
        {
            get { return EditorPrefs.GetString(EDITOR_PREFS_TWITTER_API_SECRET); }
            set { EditorPrefs.SetString(EDITOR_PREFS_TWITTER_API_SECRET, value); }
        }

        public static string AccessToken
        {
            get { return EditorPrefs.GetString(EDITOR_PREFS_TWITTER_ACCESS_TOKEN); }
            set { EditorPrefs.SetString(EDITOR_PREFS_TWITTER_ACCESS_TOKEN, value); }
        }
        public static string AccessSecret
        {
            get { return EditorPrefs.GetString(EDITOR_PREFS_TWITTER_ACCESS_SECRET); }
            set { EditorPrefs.SetString(EDITOR_PREFS_TWITTER_ACCESS_SECRET, value); }
        }

        public static bool IsConfigured {
            get
            {
                if (string.IsNullOrEmpty(ApiKey))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(ApiSecret))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(AccessToken))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(AccessSecret))
                {
                    return false;
                }

                return true;
            }
        }

        public static void ClearAccessTokens()
        {
            AccessToken = null;
            AccessSecret = null;
        }

        public static void Reset()
        {
            EditorPrefs.DeleteKey(EDITOR_PREFS_TWITTER_USER_ID);
            EditorPrefs.DeleteKey(EDITOR_PREFS_TWITTER_USER_SCREEN_NAME);
            EditorPrefs.DeleteKey(EDITOR_PREFS_TWITTER_API_KEY);
            EditorPrefs.DeleteKey(EDITOR_PREFS_TWITTER_API_SECRET);
            EditorPrefs.DeleteKey(EDITOR_PREFS_TWITTER_ACCESS_TOKEN);
            EditorPrefs.DeleteKey(EDITOR_PREFS_TWITTER_ACCESS_SECRET);
        }
    }
}
