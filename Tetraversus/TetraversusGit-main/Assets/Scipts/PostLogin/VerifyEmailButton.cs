using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Scipts.Configuration;
using Scipts.Models.DTOs;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class VerifyEmailButton : MonoBehaviour
{
    public Button verifyEmailButton;

    public TextMeshProUGUI token;

    public AppConfig _config;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async Task Start()
    {
        _config = await AppConfigLoader.LoadAsync();
        verifyEmailButton.onClick.AddListener(Verify);
    }

    private void Verify()
    {
        string sanitizeToken = SanitizeInput(token.text);
        StartCoroutine(VerifyRoutine(sanitizeToken,onSuccessToken: token =>
            {
                Debug.Log("Email verificado correctamente");
            },
            onError: ex =>
            {
                Debug.Log("Error al verificar el email: "+ex.ToString());
            }));
    }

    private IEnumerator VerifyRoutine(string sanitizeToken, Action<object> onSuccessToken, Action<object> onError)
    {
        if (string.IsNullOrWhiteSpace(sanitizeToken))
        {
            onError?.Invoke(new Exception("Token is required."));
            yield break;
        }

        string url = _config.verifyEmailUrl;

        // ✅ Correct JSON body: { "token": "..." }
        var dto = new TokenVerificationDTO { Token = sanitizeToken };
        string json = JsonConvert.SerializeObject(dto);
        Debug.Log(json);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // ✅ Correct Authorization: Bearer <jwt>
        string jwt = PlayerPrefs.GetString("auth_token", "").Trim();
        if (string.IsNullOrWhiteSpace(jwt))
        {
            onError?.Invoke(new Exception("Missing auth_token in PlayerPrefs (Authorization required)."));
            yield break;
        }

        // If already stored with "Bearer ", avoid double prefix
        if (!jwt.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            jwt = "Bearer " + jwt;

        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPUT))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();

            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "application/json");
            req.SetRequestHeader("Authorization", jwt);

            req.certificateHandler = new PinnedCertHandler(_config.certificatePinSha256);
            req.disposeCertificateHandlerOnDispose = true;

            yield return req.SendWebRequest();

            long code = req.responseCode;
            string resp = req.downloadHandler != null ? req.downloadHandler.text : "";

            if (code >= 200 && code < 300)
            {
                onSuccessToken?.Invoke(resp);
                yield break;
            }

            onError?.Invoke(new Exception($"HTTP {code} - {req.error}\n{resp}\nRequestBody: {json}"));
        }
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
