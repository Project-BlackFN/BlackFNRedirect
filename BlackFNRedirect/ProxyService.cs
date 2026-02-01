using System;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace BlackFNRedirect;

public sealed class ProxyService : IDisposable
{
    private readonly ProxyConfiguration _config;
    private readonly ProxyServer _proxyServer;
    private readonly RequestHandler _requestHandler;
    private readonly CertificateHandler _certificateHandler;
    private bool _disposed;

    public ProxyService(ProxyConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _proxyServer = new ProxyServer();
        _requestHandler = new RequestHandler(_config);
        _certificateHandler = new CertificateHandler(_config, _proxyServer);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _certificateHandler.ConfigureCertificates();

        _proxyServer.BeforeRequest += _requestHandler.OnRequestAsync;
        _proxyServer.BeforeResponse += _requestHandler.OnResponseAsync;
        _proxyServer.ServerCertificateValidationCallback += _certificateHandler.OnCertificateValidationAsync;

        var endpoint = new ExplicitProxyEndPoint(_config.ListenAddress, _config.Port, true);
        endpoint.BeforeTunnelConnectRequest += _requestHandler.OnBeforeTunnelConnectAsync;

        _proxyServer.AddEndPoint(endpoint);
        _proxyServer.Start();

        _certificateHandler.TrustRootCertificate();

        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (!_disposed && _proxyServer.ProxyRunning)
        {
            _proxyServer.Stop();
        }
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _proxyServer?.Dispose();
        _disposed = true;
    }
}