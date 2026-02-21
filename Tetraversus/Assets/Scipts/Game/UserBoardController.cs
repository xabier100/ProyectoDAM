using System;
using System.Collections;
using System.Threading.Tasks;
using Scipts.Configuration;
using Scipts.Models.DTOs;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class UserBoardController : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI username;
    public static UserBoardController Instance { get; private set; }
    private AppConfig config;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async Task  Start()
    {
        username.text = PlayerPrefs.GetString("username");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        config =await  AppConfigLoader.LoadAsync();
        SaveUsernameFromToken(
            updatedUsername => PlayerPrefs.SetString("username", updatedUsername),
            error => Debug.LogError($"Failed to get username from token: {error.Message}")
        );

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SaveUsernameFromToken(Action<string> onUpdated, Action<Exception> onError)
    {
        StartCoroutine(SaveUsernameFromTokenRoutine(onUpdated, onError));
    }
    private IEnumerator SaveUsernameFromTokenRoutine(Action<string> onUpdated, Action<Exception> onError)
    {
        string token = PlayerPrefs.GetString("auth_token", "");
        if (string.IsNullOrWhiteSpace(token))
        {
            onError?.Invoke(new Exception("No auth_token found in PlayerPrefs."));
            yield break;
        }

        // Ensure Bearer prefix
        token = token.Trim();
        if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            token = "Bearer " + token;

        if (string.IsNullOrWhiteSpace(config?.getUsernameFromTokenUrl))
        {
            onError?.Invoke(new Exception("Config meUrl is missing."));
            yield break;
        }

        using (var req = UnityWebRequest.Get(config.getUsernameFromTokenUrl))
        {
            req.SetRequestHeader("Accept", "application/json");
            req.SetRequestHeader("Authorization", token);

            // Optional: certificate pinning (only if you use it elsewhere)
            if (!string.IsNullOrWhiteSpace(config.certificatePinSha256))
            {
                req.certificateHandler = new PinnedCertHandler(config.certificatePinSha256);
                req.disposeCertificateHandlerOnDispose = true;
            }

            yield return req.SendWebRequest();

            long code = req.responseCode;
            string body = req.downloadHandler != null ? req.downloadHandler.text : "";

            // Transport-level error (DNS, TLS, etc.)
            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(new Exception($"HTTP {code} - {req.error}\n{body}"));
                yield break;
            }

            // Auth errors
            if (code == 401 || code == 403)
            {
                onError?.Invoke(new Exception("Token invalid/expired (401/403). Please login again."));
                yield break;
            }

            if (code != 200)
            {
                onError?.Invoke(new Exception($"Unexpected HTTP {code}\n{body}"));
                yield break;
            }

            UsernameResponse response = JsonUtility.FromJson<UsernameResponse>(body);

            if (string.IsNullOrWhiteSpace(response.username))
            {
                onError?.Invoke(new Exception($"200 OK but couldn't extract username from response:\n{body}"));
                yield break;
            }

            PlayerPrefs.SetString("username", response.username);
            PlayerPrefs.Save();

            onUpdated?.Invoke(response.username);
        }
    }
}
