using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CodeVerifier : MonoBehaviour
{
    #region Inspector Fields - Form
    [SerializeField] private TMP_InputField _inputCode;
    [SerializeField] private Button _submitCode;
    [SerializeField] private TextMeshProUGUI _txtMessage;

    [SerializeField] private Color colorSuccess;
    [SerializeField] private Color colorFail;
    #endregion // Inspector Fields - Form

    #region HTTP Fields
    [SerializeField] private HttpClientHelper _httpHelper;

    [Header("Token")]
    [SerializeField]
    private string _tokenUrl;
    [SerializeField]
    private string _clientID;
    [SerializeField]
    private string _clientSecret;

    [Header("Request")]
    [SerializeField]
    private string _requestUrl;
    #endregion // HTTP Fields

    #region Private Fields
    private string _msgEmpty = "Please input an access code.";
    private string _msgProcessing = "Verifying...";
    private string _msgSucceeded = "Successful!";
    private string _msgFailed = "Failed to verify the code. The code may be invalid or you do not have internet connection.";
    #endregion // Private Fields

    #region Unity Callbacks
    private void Awake()
    {
        var code = PlayerPrefs.GetString(PlayerPrefKeys.SAccessCode);
        if (!string.IsNullOrEmpty(code))
        {
            LoadNextScene();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        _txtMessage.text = "";
        _txtMessage.color = colorFail;

        _submitCode.onClick.AddListener(CheckCode);

        _inputCode.onSubmit.AddListener(OnInputSubmit);
    }
    private void OnInputSubmit(string value)
    {
        CheckCode();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            CheckCode();
        }
    }

    #endregion // Unity Callbacks

    #region Implementation
    private void CheckCode()
    {
        _submitCode.interactable = false;

        string code = _inputCode.text.ToUpper();
        if (string.IsNullOrEmpty(code))
        {
            _txtMessage.color = colorFail;
            _txtMessage.text = _msgEmpty;
            _submitCode.interactable = true;
            return;
        }

        _txtMessage.color = colorSuccess;
        _txtMessage.text = _msgProcessing;

        VerifyCode(code);
    }


    private async void VerifyCode(string code)
    {
        bool result = await _httpHelper.UseCode(
            _tokenUrl,
            _clientID,
            _clientSecret,
            _requestUrl,
            code);

        if (!result)
        {
            _txtMessage.color = colorFail;
            _txtMessage.text = _msgFailed;
            _submitCode.interactable = true;
            return;
        }


        // SUCCESS
        PlayerPrefs.SetString(PlayerPrefKeys.SAccessCode, code);
        _txtMessage.color = colorSuccess;
        _txtMessage.text = _msgSucceeded;
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (SceneManager.sceneCountInBuildSettings > nextSceneIndex)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }

    #endregion // Implementation

    #region Context Menu
    [ContextMenu("Reset Code")]
    private void ResetCode()
    {
        PlayerPrefs.DeleteKey(PlayerPrefKeys.SAccessCode);
    }
    #endregion // Context Menu

}
