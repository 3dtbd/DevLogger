using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.EditorUtils {
    public static class Skin
    {
        public static GUIStyle infoLabelStyle;
        public static GUIStyle errorLabelStyle;

        static Skin()
        {
            infoLabelStyle = new GUIStyle();
            infoLabelStyle.alignment = TextAnchor.UpperLeft;

            GUIStyleState errorNormalState = new GUIStyleState();
            errorNormalState.textColor = Color.red;

            errorLabelStyle = new GUIStyle();
            errorLabelStyle.alignment = TextAnchor.UpperLeft;
            errorLabelStyle.normal = errorNormalState;
        }

        public static void StartSection(string title, bool withSpace = true)
        {
            if (withSpace)
            {
                EditorGUILayout.Space();
            }
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        public static void EndSection()
        {
            EditorGUILayout.EndVertical();
        }

        public static void StartHelpBox(bool withSpace = true)
        {
            if (withSpace)
            {
                EditorGUILayout.Space();
            }
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        }

        public static void EndHelpBox()
        {
            EditorGUILayout.EndVertical();
        }
    }
}