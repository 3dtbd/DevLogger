using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.Git
{
    [InitializeOnLoad]
    public static class GitConstants
    {
        private const string EDITOR_PREFS_SCOPE = "WizardsCode.Git";

        public static string GIT_PATH => $"{EDITOR_PREFS_SCOPE}_GitExecutablePath";
        public static string REPOSITORY_PATH => $"{EDITOR_PREFS_SCOPE}_RepositoryPath_" + Application.productName;
    }
}