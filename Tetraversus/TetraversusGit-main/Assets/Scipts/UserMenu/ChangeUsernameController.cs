using System;
using System.Collections;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using Newtonsoft.Json;
using Scipts.Configuration;
using Scipts.Models.DTOs;
using UnityEngine;
using UnityEngine.Networking;

public class ChangeUsernameController : MonoBehaviour
{
    private AppConfig _config;
    
    //Singleton instance
    public static ChangeUsernameController Instance { get; private set; }

    private async void Awake()
    {
        _config =await AppConfigLoader.LoadAsync();
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Change(string newUsername, Action<string> onSuccess, Action<Exception> onError)
    {
        StartCoroutine(ChangeRoutine(newUsername, onSuccess, onError));
    }

    public IEnumerator ChangeRoutine(string newUsername, Action<string> onSuccess, Action<Exception> onError)
    {
        var dto = new { newUsername };
        var json = JsonConvert.SerializeObject(dto);
        var body = Encoding.UTF8.GetBytes(json);

        using (var req = new UnityWebRequest(_config.updateUsernameUrl, UnityWebRequest.kHttpVerbPUT))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.uploadHandler.contentType = "application/json";
            req.downloadHandler = new DownloadHandlerBuffer();

            req.timeout = 30;
            req.useHttpContinue = false;

            req.SetRequestHeader("Accept", "text/plain");
            req.certificateHandler =
                new PinnedCertHandler(_config.certificatePinSha256);
            req.disposeCertificateHandlerOnDispose = true;

            var token = PlayerPrefs.GetString("auth_token");

            // Most APIs require a scheme. If Postman is using "Bearer", Unity must match it exactly.
            if (!string.IsNullOrWhiteSpace(token) && !token.StartsWith("Bearer "))
                token = "Bearer " + token;

            req.SetRequestHeader("Authorization", token);

            // TEMPORARILY disable pinning to rule TLS out, then re-enable once confirmed.
            // req.certificateHandler = new PinnedCertHandler("...");
            // req.disposeCertificateHandlerOnDispose = true;

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                var msg = $"HTTP {req.responseCode} {req.result} {req.error}\nBody: {req.downloadHandler?.text}";
                onError?.Invoke(new AuthenticationException(msg));
                yield break;
            }

            var responseText = req.downloadHandler.text;

            // If server returns plain text token, don't JSON-deserialize it.
            // If it returns JSON, keep your existing deserialization.
            onSuccess?.Invoke(responseText);
        }
    }

    public void ExistsUsername(string username, Action<bool> onSuccess, Action<Exception> onError)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            onError?.Invoke(new ArgumentException("Username is empty."));
            return;
        }
    
        StartCoroutine(ExistsUsernameRoutine(username.Trim(), onSuccess, onError));
    }

    private IEnumerator ExistsUsernameRoutine(string username, Action<bool> onSuccess, Action<Exception> onError)
    {
        string url = $"{_config.verifyUsernameUrl}?username={UnityWebRequest.EscapeURL(username)}";

        using (var req = UnityWebRequest.Get(url))
        {
            req.SetRequestHeader("Accept", "application/json");
            
            req.certificateHandler = new PinnedCertHandler(_config.certificatePinSha256);
            req.disposeCertificateHandlerOnDispose = true;
            req.SetRequestHeader("Authorization", PlayerPrefs.GetString("auth_token"));

            yield return req.SendWebRequest();

            long code = req.responseCode;

            // Expected outcomes
            if (code == 200)
            {
                onSuccess?.Invoke(true);
                yield break;
            }

            if (code == 404)
            {
                onSuccess?.Invoke(false);
                yield break;
            }
            if (req.result == UnityWebRequest.Result.ConnectionError)
            {
                onError?.Invoke(new Exception(
                    $"TLS/Connection error: {req.error}\nURL: {req.url}\n" +
                    $"Pin set? {!string.IsNullOrEmpty(_config.certificatePinSha256)}"));
                yield break;
            }

            // Anything else is a real error
            string body = req.downloadHandler != null ? req.downloadHandler.text : "";
            onError?.Invoke(new Exception($"HTTP {code} - {req.error}\n{body}"));
        }
    }

    
}
