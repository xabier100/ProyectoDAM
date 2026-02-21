namespace Scipts.Configuration
{
    using System;

    [Serializable]
    public class AppConfig
    {
        public string apiBaseUrl;
        public string authUrl;
        public string registerUrl;
        public string emailVerficationStatusUrl;
        public string updateUsernameUrl;
        public string updatePasswordUrl;
        public string usernameExistenceUrl;
        public string getUsernameFromTokenUrl;
        public string saveGameUrl;
        public string verifyEmailUrl;
        public string verifyUsernameUrl;
        public string verifyPasswordUrl;
        public int timeoutSeconds = 15;

        // If by “ssl key” you mean certificate pin/fingerprint (recommended wording)
        public string certificatePinSha256;

        public string environment; // dev/prod/etc
    }
}