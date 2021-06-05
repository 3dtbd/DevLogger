using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;

namespace WizardsCode.DevLogger
{
    public static class Settings
    {
        public static string CaptureFileFolderPath
        {
            get { 
                string path = EditorPrefs.GetString(CapturesFolderPathKey); 
                if (string.IsNullOrEmpty(path))
                {
                    path = Application.dataPath;
                    path = path.Substring(0, path.Length - "Assets/".Length);
                    path += Path.DirectorySeparatorChar + "DevLog" + Path.DirectorySeparatorChar + "ScreenCaptures";
                    CaptureFileFolderPath = path;
                }

                return path;
            }
            set {
                EditorPrefs.SetString(CapturesFolderPathKey, value); 
            }
        }

        public static bool OrganizeCapturesByProject
        {
            get { return EditorPrefs.GetBool(OrganizeCapturesByProjectKey); }
            set { EditorPrefs.SetBool(OrganizeCapturesByProjectKey, value); }
        }

        public static bool OrganizeCapturesByScene { 
            get { return EditorPrefs.GetBool(OrganizeCapturesBySceneKey); }
            set { EditorPrefs.SetBool(OrganizeCapturesBySceneKey, value); }
        }

        public static string DevLogScriptableObjectPath { 
            get { return EditorPrefs.GetString(DevLogScriptableObjectPathKey); }
            set { EditorPrefs.SetString(DevLogScriptableObjectPathKey, value); }
        }

        public static string ScreenCaptureScriptableObjectPath
        {
            get { return EditorPrefs.GetString(ScreenCaptureScriptableObjectPathKey); }
            set { EditorPrefs.SetString(ScreenCaptureScriptableObjectPathKey, value); }
        }

        public static bool TrimSceneViewToolbar
        {
            get { return EditorPrefs.GetBool(TrimSceneViewToolbarPathKey, true); }
            set { EditorPrefs.SetBool(TrimSceneViewToolbarPathKey, value); }
        }

        public static bool TrimGameViewToolbar
        {
            get { return EditorPrefs.GetBool(TrimGameViewToolbarPathKey, true); }
            set { EditorPrefs.SetBool(TrimGameViewToolbarPathKey, value); }
        }

        public static bool TrimTabsWhenMaximized
        {
            get { return EditorPrefs.GetBool(TrimTabsWhenMaximizedPathKey, true); }
            set { EditorPrefs.SetBool(TrimTabsWhenMaximizedPathKey, value); }
        }

        public static void Reset()
        {
            EditorPrefs.DeleteKey(CapturesFolderPathKey);
            EditorPrefs.DeleteKey(OrganizeCapturesByProjectKey);
            EditorPrefs.DeleteKey(OrganizeCapturesBySceneKey);
            EditorPrefs.DeleteKey(DevLogScriptableObjectPathKey);
            EditorPrefs.DeleteKey(ScreenCaptureScriptableObjectPathKey);
        }

        #region Keys
        static string CapturesFolderPathKey
        { get { return "DevLogCapturesFolderPath_" + Application.productName; } }

        static string OrganizeCapturesByProjectKey
        { get { return "DevLogOrganizeByProject_" + Application.productName; } }

        static string OrganizeCapturesBySceneKey
        {
            get { return "DevLogOrganizeByScene_" + Application.productName; }
        }

        static string DevLogScriptableObjectPathKey
        { get { return "DevLogScriptableObjectPath_" + Application.productName; } }

        static string ScreenCaptureScriptableObjectPathKey
        {
            get { return "DevLogScreenCapturesObjectPath_" + Application.productName; }
        }

        static string TrimSceneViewToolbarPathKey
        {
            get { return "DevLogTrimSceneViewToolbar_" + Application.productName; }
        }

        static string TrimGameViewToolbarPathKey
        {
            get { return "DevLogTrimGameViewToolbar_" + Application.productName; }
        }

        static string TrimTabsWhenMaximizedPathKey
        {
            get { return "DevLogTrimTabsWhenMaximized_" + Application.productName; }
        }
        #endregion
    }
}
