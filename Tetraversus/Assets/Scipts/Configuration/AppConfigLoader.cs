using System;
using System.IO;
using System.Threading.Tasks;
using Scipts.Configuration;
using UnityEngine;
using UnityEngine.Networking;

public static class AppConfigLoader
{
    private static AppConfig _cached;

    public static async Task<AppConfig> LoadAsync()
    {
        if (_cached != null) return _cached;

        string path = Path.Combine(Application.streamingAssetsPath, "configuration.json");

        string json = await ReadTextAsync(path);

        // JsonUtility requires fields (not properties) and [Serializable]
        _cached = JsonUtility.FromJson<AppConfig>(json);

        if (_cached == null)
            throw new Exception("Failed to parse config.json");

        return _cached;
    }

    private static async Task<string> ReadTextAsync(string path)
    {
        // For Android/WebGL this will be a URL-like path; UnityWebRequest is safest
        using var req = UnityWebRequest.Get(path);
        req.timeout = 15;

        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
            throw new Exception($"Failed to read config at '{path}': {req.error}");

        return req.downloadHandler.text;
    }
}