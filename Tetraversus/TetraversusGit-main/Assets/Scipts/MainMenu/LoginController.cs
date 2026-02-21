using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Resources;
using Scipts.Configuration;
using Scipts.Models.DTOs;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


public class LoginController : MonoBehaviour
{
    public static LoginController Instance { get; private set; }

    private string authUrl;
    private AppConfig config;

    private async void Awake()
    {
        config = await AppConfigLoader.LoadAsync();
        authUrl = config.authUrl;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    } 
    
    public void Login(
        string username,
        string password,
        Action<string> onSuccessToken,
        Action<Exception> onError)
    {
        StartCoroutine(LoginRoutine(username, password, onSuccessToken, onError));
    }

    private IEnumerator LoginRoutine(
        string username,
        string password,
        Action<string> onSuccessToken,
        Action<Exception> onError)
    {
        var dto = new { username, password };
        var json = JsonConvert.SerializeObject(dto);

        using (var req = new UnityWebRequest(authUrl, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = 15;

            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "text/plain");

            // Pinning as you had it (optional)
            req.certificateHandler =
                new PinnedCertHandler(config.certificatePinSha256);
            req.disposeCertificateHandlerOnDispose = true;

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
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

            var body = req.downloadHandler.text;
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponseDto>(body);

            onSuccessToken?.Invoke(tokenResponse.Token);
        }
    }
}