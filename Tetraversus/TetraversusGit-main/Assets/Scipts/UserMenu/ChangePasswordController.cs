using System;
using System.Collections;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Scipts.Configuration;
using Scipts.Models.DTOs;
using UnityEngine;
using UnityEngine.Networking;

public class ChangePasswordController : MonoBehaviour
{
    public AppConfig _config;
    
    //Singleton instance
    public static ChangePasswordController Instance { get; private set; }

    private async Task Awake()
    {
        _config = await AppConfigLoader.LoadAsync();
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

    public void Change(string oldPassword, string newPassword, Action<string> onSuccess, Action<Exception> onError)
    {
        StartCoroutine(ChangeRoutine(oldPassword, newPassword, onSuccess, onError));
    }
    
    public IEnumerator ChangeRoutine(string oldPassword, string newPassword,Action<string> onSuccess, Action<Exception> onError)
    {
        var dto = new { oldPassword, newPassword}; // ensures camelCase keys

        var json = JsonConvert.SerializeObject(dto);
  
        using (var req = new UnityWebRequest(_config.updatePasswordUrl, UnityWebRequest.kHttpVerbPUT))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = 15;

            // Match cURL exactly:
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "text/plain");
            req.SetRequestHeader("Authorization",PlayerPrefs.GetString("auth_token"));
            

            // Only keep pinning if the thumbprint matches the *localhost* cert you’re actually hitting:
            // (If unsure, comment these two lines to rule TLS pinning out.)
            req.certificateHandler =
                new PinnedCertHandler(_config.certificatePinSha256);
            req.disposeCertificateHandlerOnDispose = true;
            yield return req.SendWebRequest();
            
            bool isError = req.result != UnityWebRequest.Result.Success;

            if (isError)
            {
                var headers = req.GetResponseHeaders();
                var headersText = headers == null
                    ? "(none)"
                    : string.Join("\n", System.Linq.Enumerable.Select(headers, kv => $"{kv.Key}: {kv.Value}"));

                var msg =
                    $"HTTP {req.responseCode} {req.error}\n";

                onError?.Invoke(new AuthenticationException(msg));
                yield break;
            }
            var responseText = req.downloadHandler.text;

            onSuccess?.Invoke(responseText);
        }
    }
    
    public void ExistsPassword(string password, Action<bool> onSuccess, Action<Exception> onError)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            onError?.Invoke(new ArgumentException("Password is empty."));
            return;
        }
    
        StartCoroutine(ExistsPasswordRoutine(password.Trim(), onSuccess, onError));
    }

    private IEnumerator ExistsPasswordRoutine(string password, Action<bool> onSuccess, Action<Exception> onError)
    {
        string url = $"{_config.verifyPasswordUrl}?password={UnityWebRequest.EscapeURL(password)}";

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
