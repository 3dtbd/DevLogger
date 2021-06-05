using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WizardsCode.DevLogger
{
    public class DevLogScreenCapture : ScriptableObject
    {
        public enum ImageEncoding { gif, png }

        public ImageEncoding encoding;
        public string windowName;
        public string productName;
        public string version;
        public string sceneName;
        public int _width;
        public int _height;
        [SerializeField] private string _timestampAsString;

        public bool IsImageSaved = false;
        public bool IsSelected = false;
        public string AbsoluteSaveFolder;

        public DateTime timestamp
        {
            get { return DateTime.Parse(_timestampAsString); }
            set { _timestampAsString = value.ToString(); }
        }

        private Texture2D _texture;

        public Texture2D Texture
        {
            get {
                if (_texture == null && IsImageSaved)
                {
                    LoadPreviewTexture();
                }
                return _texture; 
            }
            set {
                _texture = value;
            }
        }

        private void LoadPreviewTexture()
        {
            Uri uri = new Uri(PreviewImagePath);
            string absoluteUri = uri.AbsoluteUri;
            WWW www = new WWW(absoluteUri);
            do { } while (!www.isDone && string.IsNullOrEmpty(www.error));

            if (string.IsNullOrEmpty(www.error))
            {
                _texture = new Texture2D(_width, _height);
                www.LoadImageIntoTexture(_texture);
            } else
            {
                Debug.LogError("Unable to read image from " + uri + " into texture: " + www.error);
            }
        }

        public string PreviewImagePath
        {
            get { return AbsoluteSaveFolder + Filename.Replace(".gif", ".png"); } 
        }

        /// <summary>
        /// The full image path, including filename.
        /// </summary>
        public string ImagePath
        {
            get {
                string path = (AbsoluteSaveFolder + Filename).Replace("/", "\\");
                return path; 
            }
        }

        public string Filename
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(windowName);
                sb.Append("_");
                sb.Append(productName);
                sb.Append("_");
                sb.Append(sceneName);
                sb.Append("_v");
                sb.Append(version);
                sb.Append("_");
                sb.Append(timestamp.ToFileTime());
                sb.Append(".");
                sb.Append(encoding);
                return sb.ToString();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Filename: " + Filename);
            sb.AppendLine("Product Name: " + productName);
            sb.AppendLine("Scene Name: " + sceneName);
            sb.AppendLine("Window Name: " + windowName);
            sb.AppendLine("Width: " + _width);
            sb.AppendLine("Height: " + _height);
            sb.AppendLine("Encoding: " + encoding);
            sb.AppendLine("AbsoluteSaveFolder: " + AbsoluteSaveFolder);
            sb.AppendLine("Image Path: " + ImagePath);
            sb.AppendLine("Preview Image Path: " + PreviewImagePath);
            sb.AppendLine("IsImageSaved: " + IsImageSaved);
            sb.AppendLine("IsSelected: " + IsSelected);
            sb.AppendLine("Version: " + version);
            sb.AppendLine("TimeStamp: " + _timestampAsString);
            return sb.ToString();
        }
    }
}
