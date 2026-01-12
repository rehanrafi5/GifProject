using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DeanSalazar.HttpClient
{
    public class HttpClientSystem : MonoBehaviour
    {
        private UnityWebRequest www;
        private string token;

        public string Token => token;

        public async Task<string> GetAccessToken(string tokenUrl, string grantType, string clientID, string clientSecret)
        {
            Dictionary<string, string> content = new Dictionary<string, string>
            {
                //Fill key and value
                //{ "grant_type", "client_credentials" },
                { "grant_type", grantType },
                { "client_id", clientID },
                { "client_secret", clientSecret }
            };

            UnityWebRequest www = UnityWebRequest.Post(tokenUrl, content);

            //Send request
            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            // FAILED
            if (www.result != UnityWebRequest.Result.Success)
            {
                var message = $"{GetType().Name} Failed: {www.error}";
                Debug.Log(message);
                return "";
            }

            // SUCCESS
            var jsonResponse = www.downloadHandler.text;
            TokenInfo tokenInfo = JsonUtility.FromJson<TokenInfo>(jsonResponse);
            Debug.Log("Successfully retrieved token: " + tokenInfo.access_token);
            token = tokenInfo.access_token;
            return tokenInfo.access_token;
        }

        public async Task<TResultType> Get<TResultType>(string url, string userAgent, string accept, string accessToken, string contentType)
        {
            www = UnityWebRequest.Get(url);
            www.SetRequestHeader("User-Agent", userAgent);
            www.SetRequestHeader("Accept", accept);

            if (!string.IsNullOrEmpty(accessToken))
            {
                www.SetRequestHeader("Authorization", "Bearer " + accessToken);
            }
            if (!string.IsNullOrEmpty(contentType))
            {
                www.SetRequestHeader("Content-Type", contentType);
            }

            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            var jsonResponse = www.downloadHandler.text;

            if (www.result != UnityWebRequest.Result.Success)
            {
                var message = $"{GetType().Name} Failed: {www.error}";
                Debug.Log(message);
                return default;
            }

            try
            {
                var result = JsonUtility.FromJson<TResultType>(jsonResponse);
                var message = $"{GetType().Name} Success: {jsonResponse}";
                Debug.Log(message);
                return result;
            }
            catch (Exception ex)
            {
                var message = $"{GetType().Name}  Could not parse response {jsonResponse}. {ex.Message}";
                Debug.Log(message);
                return default;
            }
        }

        public async Task<bool> Delete(string url, string accessToken, string userAgent, string accept, string contentType)
        {
            www = UnityWebRequest.Delete(url);
            www.SetRequestHeader("Authorization", "Bearer " + accessToken);
            www.SetRequestHeader("User-Agent", userAgent);
            www.SetRequestHeader("Accept", accept);
            www.SetRequestHeader("Content-Type", contentType);

            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            // FAILED
            if (www.result != UnityWebRequest.Result.Success)
            {
                var message = $"{GetType().Name} Failed: {www.error}";
                Debug.Log(message);
                return false;
            }

            try
            {
                var message = $"{GetType().Name} Success";
                Debug.Log(message);
                return true;
            }
            catch (Exception ex)
            {
                var message = $"{GetType().Name}  Could not parse response. {ex.Message}";
                Debug.Log(message);
                return false;
            }
        }

        public void Abort()
        {
            if (www != null && !www.isDone)
            {
                www.Abort();
                var message = $"{GetType().Name} Aborted.";
                Debug.Log(message);
            }
        }
    }
}