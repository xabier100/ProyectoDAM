using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Security.Authentication;
using System.Text;
using Newtonsoft.Json;
using Scipts.Configuration;
using Scipts.Models.DTOs;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


public class RegisterController : MonoBehaviour
{
    public static RegisterController Instance { get; private set; }

    public string registerUrl ;

    private AppConfig config;

    private async void Awake()
    {
        config = await AppConfigLoader.LoadAsync();
        registerUrl = config.registerUrl;
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

    public void Register(
        string username,
        string email,
        string password,
        Action<RegisterResponse> onSuccess,
        Action<Exception> onError)
    {
        StartCoroutine(RegisterCoroutine(username, email, password, onSuccess, onError));
    }

    private System.Collections.IEnumerator RegisterCoroutine(
        string username,
        string email,
        string password,
        Action<RegisterResponse> onSuccess,
        Action<Exception> onError)
    {
        var dto = new { username, email, password };
        var json = JsonConvert.SerializeObject(dto);

        using var req = new UnityWebRequest(registerUrl, UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        // Treat any 2xx as success
        bool is2xx = req.responseCode >= 200 && req.responseCode < 300;

        if (is2xx)
        {
            try
            {
                var body = req.downloadHandler.text;
                var res = JsonConvert.DeserializeObject<RegisterResponse>(body);

                if (res == null)
                    throw new Exception("Empty/invalid register response.");

                onSuccess?.Invoke(res);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        }
        else
        {
            // Optional: include server body for debugging
            var body = req.downloadHandler?.text;
            onError?.Invoke(new Exception($"HTTP {(int)req.responseCode}. {body}"));
        }
    }
    
}