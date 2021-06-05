using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace WizardsCode.DevLogger
{
    public static class Discord
    {

        static UnityWebRequest www;

        public static void PostEntry(DevLogEntry entry)
        {
            string username = DiscordSettings.Username;
            
            PostMessage(new Message(username, entry));
        }

        private static void PostMessage(Message message)
        {
            string url = DiscordSettings.WebHookURL;

            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("username", message.username));
            formData.Add(new MultipartFormDataSection("content", message.introText + "\n" + message.bodyText));
            for (int i = 0; i < message.files.Count; i++)
            {
                formData.Add(new MultipartFormFileSection(message.files[i].name, message.files[i].fileData, message.files[i].filename, message.files[i].contentType));
            }
            www = UnityWebRequest.Post(url, formData);

            AsyncOperation asyncOperation = www.SendWebRequest();
            
            message.entry.discordPost = true;
            message.entry.lastDiscordPostFileTime = DateTime.Now.ToFileTimeUtc();
            EditorApplication.update += PostRequestUpdate;
        }

        static void PostRequestUpdate()
        {
            if (!www.isDone)
                return;

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            www.Dispose();

            EditorApplication.update -= PostRequestUpdate;
        }
    }

    [Serializable]
    public class Message
    {
        public string introText;
        public string bodyText;
        public string username;
        public List<ImageContent> files = new List<ImageContent>();

        internal DevLogEntry entry;

        internal Message(string username, DevLogEntry entry)
        {
            this.entry = entry;

            this.username = username;

            this.introText = entry.shortDescription;
            for (int i = 0; i < entry.metaData.Count; i++)
            {
                if (!entry.metaData[i].StartsWith("#"))
                {
                    introText += " " + entry.metaData[i];
                }
            }

            this.bodyText = entry.longDescription;

            for (int i = 0; i < entry.captures.Count; i++)
            {
                DevLogScreenCapture capture = entry.captures[i];
                ImageContent image = new ImageContent("File" + i, capture);
                files.Add(image);
            }
        }

        /// <summary>
        /// Create a new Discord message.
        /// </summary>
        /// <param name="username">The unsername to use when posting.</param>
        /// <param name="introText">The intro text that will appear before the first image, if there is one.</param>
        /// <param name="bodyText">The main body text.</param>
        /// <param name="screenCaptures">The set of available and selected screen captures that may be shared as part of the message.</param>
        internal Message(string username, string introText, string bodyText, DevLogScreenCaptureCollection screenCaptures) : this(username, introText, screenCaptures)
        {
            this.bodyText = bodyText;
        }

        /// <summary>
        /// Create a new Discord message.
        /// </summary>
        /// <param name="username">The unsername to use when posting.</param>
        /// <param name="introText">The intro text that will appear before the first image, if there is one.</param>
        /// <param name="screenCaptures">The set of available and selected screen captures that may be shared as part of the message.</param>
        internal Message(string username, string introText, DevLogScreenCaptureCollection screenCaptures)
        {
            this.username = username;
            this.introText = introText;

            for (int i = 0; i < screenCaptures.Count; i++)
            {
                DevLogScreenCapture capture = screenCaptures.captures[i];
                if (!capture.IsSelected)
                {
                    continue;
                }

                ImageContent image = new ImageContent("File" + i, capture);
                files.Add(image);
            }
        }
    }
    
    [Serializable]
    public class ImageContent
    {
        public string name;
        public string filepath;
        public string filename;
        public byte[] fileData;
        public string contentType;

        internal ImageContent(string name, DevLogScreenCapture capture)
        {
            this.name = name;
            this.filepath = capture.ImagePath;
            this.filename = Path.GetFileName(capture.ImagePath);
            this.fileData = File.ReadAllBytes(this.filepath);
            if (this.filepath.EndsWith("jpg"))
            {
                this.contentType = "image/jpg";
            }
            else if (this.filepath.EndsWith("png"))
            {
                this.contentType = "image/png";
            }
            else if (this.filepath.EndsWith("gif"))
            {
                this.contentType = "image/gif";
            }
        }
    }
}