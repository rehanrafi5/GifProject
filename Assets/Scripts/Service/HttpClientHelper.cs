using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeanSalazar.HttpClient;
using UnityEngine;

public class HttpClientHelper : MonoBehaviour
{
    #region Fields
    [SerializeField]
    private string _userAgent = "LitKit App";
    [SerializeField]
    private string _accept = "application/json";
    [SerializeField]
    private string _contentType = "application/json; charset=UTF-8";

    [Header("Access")]
    [SerializeField]
    private string _grantType = "client_credentials";

    private HttpClientSystem _httpClient;
    #endregion // Fields

    #region Init
    public void Awake()
    {
        _httpClient = GetComponent<HttpClientSystem>();
    }
    #endregion

    #region Implementation
    public async Task<bool> UseCode(string tokenUrl, string clientID, string clientSecret, string requestUrl, string code)
    {
        string codeUrl = string.Concat(requestUrl, code);
        string token;
        if (!string.IsNullOrEmpty(_httpClient.Token))
        {
            token = _httpClient.Token;
        }
        else
        {
            token = await _httpClient.GetAccessToken(tokenUrl, _grantType, clientID, clientSecret);
        }

        // double check if token is empty as GetAccessToken may return an empty string
        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("Null or Empty Token");
            return false;
        }

        return await _httpClient.Delete(codeUrl, token, _userAgent, _accept, _contentType);
    }

    public void Abort()
    {
        _httpClient.Abort();
    }
    #endregion // Implementation
}
