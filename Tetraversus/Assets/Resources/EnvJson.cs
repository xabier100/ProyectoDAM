using UnityEngine;

namespace Resources
{
    [System.Serializable]
    public class EnvConfig
    {
        public string apiBaseUrl;
        public string loginPath;
    }

    public static class EnvJson
    {
        private static EnvConfig _cache;

        public static EnvConfig Load(string name = "env")
        {
            if (_cache != null) return _cache;
            var ta = UnityEngine.Resources.Load<TextAsset>(name);
            _cache = ta ? JsonUtility.FromJson<EnvConfig>(ta.text) : new EnvConfig();
            return _cache;
        }
    }
}
