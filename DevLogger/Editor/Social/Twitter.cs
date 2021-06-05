using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using WizardsCode.DevLogger;

namespace WizardsCode.Social
{
    [ExecuteInEditMode]
    public class Twitter
    {   
        private static float verifyCredentialsTime = 0;

        /// <summary>
        /// Check that twitter credentials in Settings are still valid. 
        /// If the credentials provided are invalid then clear them to force re-entry.
        /// </summary>
        /// <returns>trus if valid</returns>
        private static bool VerifyCredentials()
        {
#if UNITY_EDITOR
            float time = (float)EditorApplication.timeSinceStartup;
#else
                float time = Time.time;
#endif
            if (time > verifyCredentialsTime)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                OAuthHelper.AddDefaultOAuthParams(dict, TwitterSettings.EDITOR_PREFS_TWITTER_API_KEY, TwitterSettings.EDITOR_PREFS_TWITTER_API_SECRET);
                if (ApiGetRequest(TwitterSettings.VerifyCredentialsURL, dict, out string response))
                {
                    verifyCredentialsTime = time + 600; // validate every 10 minutes
                    return true;
                }
                else
                {
                    TwitterSettings.ClearAccessTokens();
                    Debug.LogError(response);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Publish a tweet from an existing DevLog entry.
        /// </summary>
        /// <param name="entry">The DevLog entry to tweet.</param>
        /// <param name="response">The response from Twitter.</param>
        /// <returns>True if succesfully published.</returns>
        internal static bool PublishTweet(DevLogEntry entry, out string response)
        {
            string tweet = entry.shortDescription;
            for (int i = 0; i < entry.metaData.Count; i++)
            {
                tweet += " " + entry.metaData[i];
            }

            if (entry.captures == null || entry.captures.Count <= 0)
            {
                if (PublishTweet(tweet, out response))
                {
                    entry.tweeted = true;
                    entry.lastTweetFileTime = DateTime.Now.ToFileTimeUtc();
                    return true;
                } else
                {
                    return false;
                }
            } 
            else 
            {
                List<string> files = new List<string>();
                for (int i = 0; i < entry.captures.Count; i++)
                {
                    files.Add(entry.captures[i].ImagePath);
                }

                if (PublishTweetWithMedia(tweet, files, out response))
                {
                    entry.tweeted = true;
                    entry.lastTweetFileTime = DateTime.Now.ToFileTimeUtc();
                    return true;
                } else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Publish a text only tweet. See the console for error details.
        /// </summary>
        /// <param name="status">The text of the tweet.</param>
        /// <returns>True if succesfully published.</returns>
        public static bool PublishTweet(string status, out string response)
        {
            if (string.IsNullOrEmpty(status) || status.Length > 280)
            {
                response = string.Format("Text of tweet too long or too short at {0} chars.", status.Length);
                Debug.LogError(response);

                return false;
            }

            if (!VerifyCredentials())
            {
                response = "Twitter credentials are invalid.";
                Debug.LogError(response);
                return false;
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("status", status);

            WWWForm form = new WWWForm();
            form.AddField("status", status);

            Hashtable headers = GetHeaders(TwitterSettings.PostTweetURL, parameters);
            
            return ApiPostRequest(TwitterSettings.PostTweetURL, form, headers, out response);
        }

        static string mediaIDs;
        public static bool PublishTweetWithMedia(string status, List<string> filePaths, out string response)
        {
            if (string.IsNullOrEmpty(status) || status.Length > 280)
            {
                response = string.Format("Text of tweet too long or too short at {0} chars.", status.Length);
                Debug.LogError(response);

                return false;
            }

            if (filePaths.Count > 4)
            {
                response = "Error sending Tweet: Can only attach four images to a tweet.";
                Debug.LogError(response);
                return false;
            }

            mediaIDs = "";
            for (int i = 0; i < filePaths.Count; i++)
            {
                if (!ValidateMediaFile(filePaths[i]))
                {
                    response = "Invalid media file : " + filePaths;
                    Debug.LogError(response);
                    return false;
                }

                string mediaID;
                if (filePaths[i].EndsWith(".png"))
                {
                    mediaID = UploadSmallMedia(filePaths[i], out response);
                } else
                {
                    mediaID = UploadLargeMedia(filePaths[i], out response);
                }
                if (string.IsNullOrEmpty(mediaID))
                {
                    response = "Failed to upload media: " + response;
                    Debug.Log(response);
                    return false;
                }

                if (string.IsNullOrEmpty(mediaIDs))
                {
                    mediaIDs = mediaID;
                }
                else
                {
                    mediaIDs += "," + mediaID;
                }
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("status", status);
            parameters.Add("media_ids", mediaIDs);

            WWWForm form = new WWWForm();
            AddParametersToForm(parameters, form);

            Hashtable headers = GetHeaders(TwitterSettings.PostTweetURL, parameters);

            return ApiPostRequest(TwitterSettings.PostTweetURL, form, headers, out response);
        }

        /// <summary>
        /// Checks to ensure a media file is valid for posting to Twitter.
        /// This will check that the file exists and that it is not currently being
        /// accessed by another process. If the file does not exist yet or if there 
        /// is another process acessing the file
        /// this method will wait for a period before returning false. The idea is that
        /// the process may still be writing the file.
        /// </summary>
        /// <param name="filename">The file to validate</param>
        /// <param name="timeout">The number of seconds to wait before returning false. Defaults to 10 seconds.</param>
        /// <returns>True if this is a valid file otherwise false.</returns>
        private static bool ValidateMediaFile(string filename, int timeout = 10)
        {
            bool isValid = false;
            long endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + timeout;
            while (!File.Exists(filename) && DateTimeOffset.UtcNow.ToUnixTimeSeconds() <= endTime)
            {
                // Waiting for file to be created
            }

            while (!isValid && DateTimeOffset.UtcNow.ToUnixTimeSeconds() <= endTime)
            {
                try
                {
                    Stream stream = new FileStream(filename, FileMode.Open);
                    isValid = true;
                    stream.Close();
                }
                catch
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Handle a POST request to the Twitter API.
        /// </summary>
        /// <param name="url">The URL to make the request to.</param>
        /// <param name="form">The form data to submit.</param>
        /// <param name="headers">The headers to submit.</param>
        /// <param name="response">The response from the request.</param>
        /// <returns>True if no error was returned.</returns>
        private static bool ApiPostRequest(string url, WWWForm form, Hashtable headers, out string response)
        {
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                foreach (DictionaryEntry header in headers)
                {
                    www.SetRequestHeader((string)header.Key, (string)header.Value);
                }
                AsyncOperation asyncOperation = www.SendWebRequest();

                while (!www.isDone)
                {
                    float progress = asyncOperation.progress;
                }

                if (www.isNetworkError || www.isHttpError)
                {
                    response = string.Format("Twitter API request failed: {0} {1}", www.error, www.downloadHandler.text);
                    // TODO: Handle errors more gracefully:
                    // 413 Payload Too Large (occurred when GIF upload was too large)
                    return false;
                }
                else
                {
                    response = www.downloadHandler.text;
                    return true;
                }
            }
        }

        /// <summary>
        /// Handle a GETT request to the Twitter API.
        /// </summary>
        /// <param name="url">The URL to make the request to.</param>
        /// <param name="headers">The headers to submit.</param>
        /// <param name="response">The response from the request.</param>
        /// <returns>True if no error was returned.</returns>
        private static bool ApiGetRequest(string url, Dictionary<string, string> parameters, out string response)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                KeyValuePair<string, string> param = parameters.ElementAt(i);
                if (!param.Key.StartsWith("oauth"))
                {
                    if (i == 0)
                    {
                        url += "?";
                    }
                    else
                    {
                        url += "&";
                    }
                    url += param.Key + "=" + param.Value;
                }
            }

            Hashtable headers = GetHeaders(url, parameters, "GET");

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                foreach (DictionaryEntry header in headers)
                {
                    www.SetRequestHeader((string)header.Key, (string)header.Value);
                }
                AsyncOperation asyncOperation = www.SendWebRequest();

                while (!www.isDone)
                {
                    float progress = asyncOperation.progress;
                }

                if (www.isNetworkError || www.isHttpError)
                {
                    response = string.Format("Twitter API request failed: {0} {1}", www.error, www.downloadHandler.text);
                    return false;
                }
                else
                {
                    response = www.downloadHandler.text;
                    return true;
                }
            }
        }

        /// <summary>
        /// Get the required headers for accesing the API.
        /// </summary>
        /// <param name="url">The URL we want to access</param>
        /// <param name="parameters">The parameters to go into the headers</param>
        /// <returns>A Hashtable of the headers.</returns>
        private static Hashtable GetHeaders(string url, Dictionary<string, string> parameters, string method = "POST")
        {
            var headers = new Hashtable();
            headers["Authorization"] = OAuthHelper.GetHeaderWithAccessToken(method,
                url,
                TwitterSettings.ApiKey,
                TwitterSettings.ApiSecret,
                TwitterSettings.AccessToken,
                TwitterSettings.AccessSecret,
                parameters);
            return headers;
        }

        /// <summary>
        /// Upload a large media file to twitter, e.g. a GIF or MPG.
        /// </summary>
        /// <param name="filePath">The fully qualified path to the media file to upload.</param>
        /// <param name="response">The response from Twitter.</param>
        /// <returns>A media ID for this upload, or if the upload fails null.</returns>
        private static string UploadLargeMedia(string filePath, out string response)
        {
            byte[] bytes = File.ReadAllBytes(filePath);

            // POST media/upload (INIT)
            Dictionary<string, string> mediaParameters = new Dictionary<string, string>();
            mediaParameters.Add("command", "INIT");
            mediaParameters.Add("media_type", "image/gif");
            mediaParameters.Add("total_bytes", bytes.Length.ToString());
            mediaParameters.Add("media_category", "tweet_gif");

            WWWForm mediaForm = new WWWForm();
            AddParametersToForm(mediaParameters, mediaForm);

            Hashtable mediaHeaders = GetHeaders(TwitterSettings.UploadMediaURL, mediaParameters);

            if (!ApiPostRequest(TwitterSettings.UploadMediaURL, mediaForm, mediaHeaders, out response))
            {
                Debug.LogError(response);
                return null;
            }
            string mediaID = Regex.Match(response, @"(\Dmedia_id\D\W)(\d*)").Groups[2].Value;

            // POST media/upload (APPEND)
            byte[] chunk;
            int chunkSize = 1024 * 512; // 512 KB
            int chunkIndex = 0;
            for (int i = 0; i < bytes.Length; i += chunkSize)
            {
                if (bytes.Length - i < chunkSize)
                {
                    chunkSize = bytes.Length - i;
                }
                chunk = new byte[chunkSize];
                Array.Copy(bytes, i, chunk, 0, chunkSize);
                string mediaString = System.Convert.ToBase64String(chunk);

                mediaParameters = new Dictionary<string, string>();
                mediaParameters.Add("command", "APPEND");
                mediaParameters.Add("media_id", mediaID);
                mediaParameters.Add("segment_index", chunkIndex.ToString());
                mediaParameters.Add("media_data", mediaString);
                //mediaParameters.Add("Content-Transfer-Encoding", "base64");
                chunkIndex++;

                mediaForm = new WWWForm();
                AddParametersToForm(mediaParameters, mediaForm);

                mediaHeaders = GetHeaders(TwitterSettings.UploadMediaURL, mediaParameters);

                if (!ApiPostRequest(TwitterSettings.UploadMediaURL, mediaForm, mediaHeaders, out response))
                {
                    Debug.LogError(response);
                    return null;
                }
            }

            // POST media/upload (FINALIZE)
            mediaParameters = new Dictionary<string, string>();
            mediaParameters.Add("command", "FINALIZE");
            mediaParameters.Add("media_id", mediaID);

            mediaForm = new WWWForm();
            AddParametersToForm(mediaParameters, mediaForm);

            mediaHeaders = GetHeaders(TwitterSettings.UploadMediaURL, mediaParameters);

            if (!ApiPostRequest(TwitterSettings.UploadMediaURL, mediaForm, mediaHeaders, out response))
            {
                Debug.LogError(response);
                return null;
            }

            // Wait for it to complete
            if (WaitForUploadToComplete(mediaID, out response).IsError)
            {
                return null;
            }

            return mediaID;
        }

        private static MediaStatusResponse WaitForUploadToComplete(string mediaId, out string response)
        {
            bool isFinished = false;
            MediaStatusResponse status = null;

            Dictionary<string, string> mediaParameters = new Dictionary<string, string>();
            mediaParameters.Add("command", "STATUS");
            mediaParameters.Add("media_id", mediaId);

            response = "";
            while (!isFinished)
            {
                if (!ApiGetRequest(TwitterSettings.UploadMediaURL, mediaParameters, out response))
                {
                    status = new MediaStatusResponse();
                    status.processing_info.error.code = 256;
                    status.processing_info.error.message = "API error: " + response;
                    return status;
                }

                status = JsonUtility.FromJson<MediaStatusResponse>(response);

                if (status.processing_info != null)
                {
                    if (status.processing_info.state == "failed")
                    {
                        response = "Failed to upload media: " + status.processing_info.error.message;
                        isFinished = true;
                    } else if (status.processing_info.state != "succeeded" 
                        && status.processing_info.check_after_secs > 0)
                    {
                        float checkTime = Time.realtimeSinceStartup + status.processing_info.check_after_secs;
                        while (Time.realtimeSinceStartup < checkTime) {
                            // waiting
                        }
                    } else
                    {
                        isFinished = true;
                    }
                }
            }
            return status;
        }

        /// <summary>
        /// Upload a small media (e.g. an image) file to twitter.
        /// </summary>
        /// <param name="filePath">The fully qualified path to the media file to upload.</param>
        /// <param name="response">The response from Twitter.</param>
        /// <returns>A media ID for this upload, or if the upload fails null.</returns>
        private static string UploadSmallMedia(string filePath, out string response)
        {
            //string status = "Testing media upload to twitter";
            Dictionary<string, string> mediaParameters = new Dictionary<string, string>();
            string mediaString = System.Convert.ToBase64String(File.ReadAllBytes(filePath));
            mediaParameters.Add("media_data", mediaString);

            WWWForm mediaForm = new WWWForm();
            mediaForm.AddField("media_data", mediaString);

            Hashtable mediaHeaders = GetHeaders(TwitterSettings.UploadMediaURL, mediaParameters);
            mediaHeaders.Add("Content-Transfer-Encoding", "base64");

            if (!ApiPostRequest(TwitterSettings.UploadMediaURL, mediaForm, mediaHeaders, out response))
            {
                Debug.LogError(response);
                return null;
            }
            return Regex.Match(response, @"(\Dmedia_id\D\W)(\d*)").Groups[2].Value;
        }

        private static void AddParametersToForm(Dictionary<string, string> parameters, WWWForm form)
        {
            foreach (KeyValuePair<string, string> param in parameters)
            {
                form.AddField(param.Key, param.Value);
            }
        }
    }

    [Serializable]
    internal class MediaStatusResponse
    {
        public ProcessingInfo processing_info;

        public MediaStatusResponse()
        {
            this.processing_info = new ProcessingInfo();
        }

        public bool IsError
        {
            get
            {
                if (processing_info.error != null && processing_info.error.code != 0)
                {
                    return true;
                } else
                {
                    return false;
                }
            }
        }
    }

    [Serializable]
    internal class ProcessingInfo { 
        public string state = null;
        public int check_after_secs = 2;
        public MediaError error;

        public ProcessingInfo()
        {
            error = new MediaError();
        }
    }

    [Serializable]
    internal class MediaError {
        public int code;
        public string name = "";
        public string message = "";
    }

}