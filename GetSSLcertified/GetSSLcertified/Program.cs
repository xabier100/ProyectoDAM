using System;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

Console.Write("The REST API server must be running to get the certificate. ");
Console.Write("Enter https URL (default: https://localhost:7291): ");
string? inputUrl = Console.ReadLine();
string url = string.IsNullOrWhiteSpace(inputUrl) ? "https://localhost:7291" : inputUrl.Trim();

if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != "https")
{
    Console.WriteLine("Invalid URL. Example: https://localhost:7291");
    return;
}

Console.Write("Output format [1=hex, 2=base64] (default: 1): ");
string? fmt = Console.ReadLine();
bool outputBase64 = fmt?.Trim() == "2";

string host = uri.Host;
int port = uri.Port > 0 ? uri.Port : 443;

X509Certificate2? serverCert = null;

using var tcp = new TcpClient();
tcp.Connect(host, port);

using var ssl = new SslStream(
    tcp.GetStream(),
    leaveInnerStreamOpen: false,
    userCertificateValidationCallback: (_, cert, __, ___) =>
    {
        if (cert != null) serverCert = new X509Certificate2(cert);
        return true; // allow self-signed so handshake completes
    });

ssl.AuthenticateAsClient(new SslClientAuthenticationOptions
{
    TargetHost = host,
    EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
});

if (serverCert is null)
{
    Console.WriteLine("No server certificate received.");
    return;
}

// certificatePinSha256 = SHA-256 over certificate DER (RawData)
byte[] hash = SHA256.HashData(serverCert.RawData);

// Print ONLY the pin (one line)
Console.WriteLine(outputBase64
    ? Convert.ToBase64String(hash)
    : Convert.ToHexString(hash).ToLowerInvariant());
    