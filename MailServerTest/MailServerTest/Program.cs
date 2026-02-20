// See https://aka.ms/new-console-template for more information

using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

public sealed class SmtpSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public string Mode { get; set; } = "STARTTLS"; // STARTTLS | SSL | PLAIN
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? FromEmail { get; set; }
    public string? ToEmail { get; set; }
    public bool SendTestEmail { get; set; } = false;
}

internal class Program
{
    private static async Task<int> Main()
    {
        // Load config from JSON + allow environment variable overrides
        // Example override:
        //   setx Smtp__Password "secret"
        // (double underscore maps to ':')
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var settings = config.GetSection("Smtp").Get<SmtpSettings>() ?? new SmtpSettings();

        // Basic validation
        if (string.IsNullOrWhiteSpace(settings.Host))
        {
            Console.WriteLine("Smtp:Host is required.");
            return 2;
        }
        if (settings.Port <= 0)
        {
            Console.WriteLine("Smtp:Port must be > 0.");
            return 2;
        }

        var mode = (settings.Mode ?? "").Trim().ToUpperInvariant();
        SecureSocketOptions socketOptions = mode switch
        {
            "STARTTLS" => SecureSocketOptions.StartTls,
            "SSL"      => SecureSocketOptions.SslOnConnect,
            "PLAIN"    => SecureSocketOptions.None,
            _ => throw new ArgumentException("Smtp:Mode must be STARTTLS, SSL, or PLAIN.")
        };

        const int timeoutMs = 15000;

        using var client = new SmtpClient
        {
            Timeout = timeoutMs,
            // WARNING: Protocol logs can include sensitive info depending on server behavior.
        };

        client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
        {
            Console.WriteLine();
            Console.WriteLine("=== TLS Certificate ===");
            Console.WriteLine($"Subject: {certificate?.Subject}");
            Console.WriteLine($"Issuer : {certificate?.Issuer}");
            Console.WriteLine($"Errors : {errors}");
            Console.WriteLine("=======================");
            Console.WriteLine();

            // Accept only valid certificates (recommended).
            return errors == System.Net.Security.SslPolicyErrors.None;
        };

        try
        {
            Console.WriteLine($"Connecting to {settings.Host}:{settings.Port} with {socketOptions} ...");
            await client.ConnectAsync(settings.Host, settings.Port, socketOptions);

            Console.WriteLine("Connected.");
            Console.WriteLine($"IsEncrypted: {client.IsEncrypted}");
            Console.WriteLine($"IsConnected: {client.IsConnected}");
            Console.WriteLine($"Auth mechanisms: {string.Join(", ", client.AuthenticationMechanisms)}");
            Console.WriteLine();

            var hasCreds = !string.IsNullOrWhiteSpace(settings.Username) && settings.Password != null;
            if (hasCreds)
            {
                Console.WriteLine($"Authenticating as {settings.Username} ...");
                await client.AuthenticateAsync(settings.Username, settings.Password);
                Console.WriteLine("Authenticated.");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Skipping authentication (Username/Password not provided).");
                Console.WriteLine();
            }

            if (settings.SendTestEmail)
            {
                if (string.IsNullOrWhiteSpace(settings.ToEmail))
                    throw new InvalidOperationException("Smtp:ToEmail is required when SendTestEmail=true.");

                var from = settings.FromEmail
                           ?? settings.Username
                           ?? "probe@localhost";

                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(from));
                message.To.Add(MailboxAddress.Parse(settings.ToEmail));
                message.Subject = "SMTP probe test (MailKit)";
                message.Body = new TextPart("plain")
                {
                    Text = $"SMTP probe successful.\nHost: {settings.Host}\nPort: {settings.Port}\nMode: {socketOptions}\nTime: {DateTimeOffset.Now}"
                };

                Console.WriteLine($"Sending test email to {settings.ToEmail} ...");
                await client.SendAsync(message);
                Console.WriteLine("Email sent.");
            }
            else
            {
                Console.WriteLine("SendTestEmail=false; connection/auth test completed without sending.");
            }

            await client.DisconnectAsync(true);
            Console.WriteLine("Disconnected cleanly.");
            return 0;
        }
        catch (AuthenticationException ex)
        {
            Console.WriteLine("AUTHENTICATION FAILED.");
            Console.WriteLine(ex);
            return 10;
        }
        catch (SmtpCommandException ex)
        {
            Console.WriteLine("SMTP COMMAND ERROR.");
            Console.WriteLine($"StatusCode: {ex.StatusCode}");
            Console.WriteLine($"Response  : {ex.Message}");
            return 11;
        }
        catch (SmtpProtocolException ex)
        {
            Console.WriteLine("SMTP PROTOCOL ERROR (often TLS/handshake/proxy issues).");
            Console.WriteLine(ex);
            return 12;
        }
        catch (SocketException ex)
        {
            Console.WriteLine("SOCKET ERROR (DNS/firewall/port blocked/server unreachable).");
            Console.WriteLine(ex);
            return 13;
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine("TIMEOUT.");
            Console.WriteLine(ex);
            return 14;
        }
        catch (Exception ex)
        {
            Console.WriteLine("UNEXPECTED ERROR.");
            Console.WriteLine(ex);
            return 99;
        }
    }
}
