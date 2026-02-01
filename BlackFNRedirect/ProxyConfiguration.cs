using System.Net;

namespace BlackFNRedirect;

public sealed class ProxyConfiguration
{
    public IPAddress ListenAddress { get; init; } = IPAddress.Any;
    public int Port { get; init; } = 8432;
    public string TargetHost { get; init; } = string.Empty;
    public string SourcePattern { get; init; } = string.Empty;
    public bool EnableVerboseLogging { get; init; } = false;
    public string CertificateIssuerName { get; init; } = "5XProxy Systems CA";
}