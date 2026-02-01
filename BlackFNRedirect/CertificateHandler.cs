using System;
using System.Net.Security;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;

namespace BlackFNRedirect;

internal sealed class CertificateHandler
{
    private readonly ProxyConfiguration _config;
    private readonly ProxyServer _proxyServer;

    public CertificateHandler(ProxyConfiguration config, ProxyServer proxyServer)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _proxyServer = proxyServer ?? throw new ArgumentNullException(nameof(proxyServer));
    }

    public void ConfigureCertificates()
    {
        _proxyServer.CertificateManager.CertificateEngine = 
            Titanium.Web.Proxy.Network.CertificateEngine.DefaultWindows;
        _proxyServer.CertificateManager.SaveFakeCertificates = true;
        _proxyServer.CertificateManager.RootCertificateIssuerName = _config.CertificateIssuerName;
    }

    public void TrustRootCertificate()
    {
        _proxyServer.CertificateManager.EnsureRootCertificate();
        
        if (_proxyServer.CertificateManager.RootCertificate != null &&
            !string.IsNullOrEmpty(_proxyServer.CertificateManager.RootCertificate.GetCertHashString()))
        {
            _proxyServer.CertificateManager.TrustRootCertificate(true);
        }
        else
        {
            throw new InvalidOperationException("Failed to create or retrieve root certificate.");
        }
    }

    public Task OnCertificateValidationAsync(object sender, CertificateValidationEventArgs e)
    {
        if (e.SslPolicyErrors == SslPolicyErrors.None)
        {
            e.IsValid = true;
        }

        return Task.CompletedTask;
    }
}