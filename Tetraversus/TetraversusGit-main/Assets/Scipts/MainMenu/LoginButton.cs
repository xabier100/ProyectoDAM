using System;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using Mono.Cecil.Cil;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scipts.MainMenu
{
    public class ButtonsController : MonoBehaviour
    {
        [SerializeField] private Button loginButton;
        [SerializeField] public TextMeshProUGUI username;
        [SerializeField] public TextMeshProUGUI password;
        public TextMeshProUGUI errorText;
        public GameObject popUp;
        
        private void Start()
        {
            loginButton.onClick.AddListener(Login);
        }

        private void OnDestroy()
        {
            loginButton.onClick.RemoveListener(Login);
        }

        private void Login()
        {
            LoginController loginController = LoginController.Instance;
            string sanitizeUsername = SanitizeInput(username.text);
            string sanitizePassword = SanitizeInput(password.text);
            loginController.Login(
                sanitizeUsername,
                sanitizePassword,
                onSuccessToken: token =>
                {
                    PlayerPrefs.SetString("auth_token", token);
                    SceneManager.LoadScene("PostLogin");
                },
                onError: ex =>
                {
                    popUp.SetActive(true);
                    errorText.text = "Ha habido un error: "+ex.Message;
                });
            
            
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
}