using System;
using System.Collections;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Scipts.Configuration;
using UnityEngine;
using UnityEngine.Networking;

namespace Scipts.PostLogin
{
    public class VerifyRequirementsController:MonoBehaviour
    {
        //Singleton instance
        public static VerifyRequirementsController Instance { get; private set; }

        public AppConfig config;

        private async void Awake()
        {
            config = await AppConfigLoader.LoadAsync();
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void IsVerified(Action<int> onSuccess, Action<Exception> onError)
        {
            StartCoroutine(IsVerifiedRoutine(onSuccess, onError));
        }

        public IEnumerator IsVerifiedRoutine(Action<int> onSuccess, Action<Exception> onError)
        {
            string url = config.emailVerficationStatusUrl;

            // Use the same key you used elsewhere
            string token = PlayerPrefs.GetString("auth_token", "");
            if (string.IsNullOrWhiteSpace(token))
            {
                onError?.Invoke(new AuthenticationException("Missing auth token in PlayerPrefs (key: 'auth_token')."));
                yield break;
            }

            using (var req = UnityWebRequest.Get(url))
            {
                req.timeout = 15;

                // If your API expects Bearer, use:
                // req.SetRequestHeader("Authorization", $"Bearer {token}");
                // If it expects raw token (like your other calls), use:
                req.SetRequestHeader("Authorization", token);

                // Adjust depending on what your endpoint returns.
                req.SetRequestHeader("Accept", "application/json");

                // Keep your pinning (or swap for your real thumbprint)
                req.certificateHandler =
                    new PinnedCertHandler(config.certificatePinSha256);
                req.disposeCertificateHandlerOnDispose = true;

                // IMPORTANT: you must yield the request
                yield return req.SendWebRequest();

                bool isError = req.result != UnityWebRequest.Result.Success;
                if (isError)
                {
                    var headers = req.GetResponseHeaders();
                    var headersText = headers == null
                        ? "(none)"
                        : string.Join("\n", headers.Select(kv => $"{kv.Key}: {kv.Value}"));

                    var msg =
                        $"HTTP {req.responseCode} {req.error}\n" +
                        $"Response:\n{req.downloadHandler?.text}\n" +
                        $"Headers:\n{headersText}";

                    onError?.Invoke(new AuthenticationException(msg));
                    yield break;
                }

                string body = (req.downloadHandler?.text ?? "").Trim();

                try
                {
                    var j = JObject.Parse(body);

                    bool emailVerified = j.Value<bool>("emailVerified");

                    // int convention: 1 verified, 0 not verified
                    onSuccess?.Invoke(emailVerified ? 1 : 0);
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            }
        }
    }
}