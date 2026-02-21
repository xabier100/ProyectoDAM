using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RegisterButton : MonoBehaviour
{
    [SerializeField] private Button registerButton;
    [SerializeField] public TextMeshProUGUI username;
    [SerializeField] public TextMeshProUGUI email;
    [SerializeField] public TextMeshProUGUI password;
    public GameObject popUp;
    public TextMeshProUGUI errorText;

    private void Start()
    {
        registerButton.onClick.AddListener(Register);
    }

    // private void OnDestroy()
    // {
    //     registerButton.onClick.RemoveListener(Register);
    // }

    private void Register()
    {
        RegisterController registerController = RegisterController.Instance;
        string sanitizeUsername = SanitizeInput(username.text);
        string sanitizeEmail = SanitizeInput(email.text);
        string sanitizePassword = SanitizeInput(password.text);
        Debug.Log(registerController);
        Debug.Log(popUp);
        registerController.Register(
            sanitizeUsername,
            sanitizeEmail,
            sanitizePassword,
            onSuccess: res =>
            {
                // Save token + username in both success cases
                PlayerPrefs.SetString("auth_token", res.token);
                PlayerPrefs.SetString("username", sanitizeUsername);
                
                // put your “registered and verified” destination here
                    SceneManager.LoadScene("PostLogin"); // <-- change to your real scene
            },
            onError: ex =>
            {
                popUp.SetActive(true);
                errorText.text = "Ha habido un error: " + ex.Message;
            }
        );
    }

    private string SanitizeInput(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;

        // If pasted from a URL, decode any %xx first (won't throw on most inputs)
        try
        {
            s = Uri.UnescapeDataString(s);
        }
        catch
        {
            /* ignore malformed */
        }

        // Shared cleaners
        const string
            ZeroWidthAndFormat =
                @"[\u200B\u200C\u200D\u2060\uFEFF\u200E\u200F\u202A-\u202E]"; // ZW*, BOM, bidi marks
        const string LineBreaks = @"[\r\n\u0085\u2028\u2029]+"; // CR, LF, NEL, LS, PS

        s = Regex.Replace(s, ZeroWidthAndFormat, "");
        s = Regex.Replace(s, LineBreaks, "");
        s = s.Replace("\u200B", "");

        return s;
    }
}
