using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.Git
{
    [InitializeOnLoad]
    public static class GitSettings
    {
        private static string DefaultGitExecutablePath => "C:\\Program Files\\Git\\bin\\git.exe";
        private static string m_GitPath;
        private static string m_RepositoryPath;
        public static string ExecutablePath
        {
            get
            {
                if (string.IsNullOrEmpty(m_GitPath))
                {
                    ExecutablePath = EditorPrefs.GetString(GitConstants.GIT_PATH, DefaultGitExecutablePath);
                }
                return m_GitPath; 
            }
            set
            {
                m_GitPath = value;
                EditorPrefs.SetString(GitConstants.GIT_PATH, m_GitPath);
            }
        }

        public static string RepositoryPath
        {
            get
            {
                if (string.IsNullOrEmpty(m_RepositoryPath))
                {
                    RepositoryPath = EditorPrefs.GetString(GitConstants.REPOSITORY_PATH, Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length));
                }
                return m_RepositoryPath;
            }
            set
            {
                m_RepositoryPath = value;
                EditorPrefs.SetString(GitConstants.REPOSITORY_PATH, m_RepositoryPath);
            }
        }

        public static void Reset()
        {
            EditorPrefs.DeleteKey(GitConstants.GIT_PATH);
            EditorPrefs.DeleteKey(GitConstants.REPOSITORY_PATH);
        }
    }
}
