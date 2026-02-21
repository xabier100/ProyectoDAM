using System;
using System.Security.Cryptography;
using UnityEngine.Networking;

public sealed class PinnedCertHandler : CertificateHandler
{
    private readonly string _expectedSha256Hex; // e.g. "A1B2...FF" (uppercase, no colons)

    public PinnedCertHandler(string expectedSha256Hex)
    {
        _expectedSha256Hex = expectedSha256Hex.Replace(":", "").ToUpperInvariant();
    }

    protected override bool ValidateCertificate(byte[] certificateData)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(certificateData);
            var hex = BitConverter.ToString(hash).Replace("-", "");
            return string.Equals(hex, _expectedSha256Hex, StringComparison.OrdinalIgnoreCase);
        }
    }
}