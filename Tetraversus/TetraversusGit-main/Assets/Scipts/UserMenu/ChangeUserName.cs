using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeUserName : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Button changeUserButton;
    public TextMeshProUGUI newUsername1;
    public TextMeshProUGUI newUsername2;
    public TextMeshProUGUI currentUsername;
    public GameObject popUp;
    
    public TextMeshProUGUI errorText;
    void Start()
    {
        changeUserButton.onClick.AddListener(Change);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Change()
    {
        var instance = ChangeUsernameController.Instance;

        if(currentUsername.text.Length<=0)
        {
            popUp.SetActive(true);
            errorText.text = "El nombre de usuario actual no puede estar vacio";
            return;
        }
        instance.ExistsUsername(SanitizeInput(currentUsername.text),
            onSuccess: exists =>
            {
                if (!exists)
                {
                    popUp.SetActive(true);
                    errorText.text = "El nombre de usuario actual no existe";
                    return;
                }
            },
            onError: ex =>
            {
                popUp.SetActive(true);
                errorText.text = "Ha habido un error: "+ex.Message;
                return;
            }
        );
        if (newUsername1.text != newUsername2.text)
        {
            popUp.SetActive(true);
            errorText.text = "Los nombres de usuario no coinciden";
            return;
        }

        if (newUsername1.text.Length < 6)
        {
            popUp.SetActive(true);
            errorText.text = "El nombre de usuario debe tener al menos 6 caracteres";
            return;
        }
        
        instance.Change(
            SanitizeInput(newUsername1.text),
            onSuccess: token =>
            {
                PlayerPrefs.SetString("username", SanitizeInput(newUsername1.text));
                popUp.SetActive(true);
                errorText.text = "Nombre de usuario cambiado con exito";
                errorText.color = Color.green;
            },
            onError: ex =>
            {
                errorText.text = "Ha habido un error: "+ex.Message;
                popUp.SetActive(true);
                errorText.color = Color.red;
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

