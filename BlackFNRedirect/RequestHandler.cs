using System;
using System.Threading.Tasks;
using Titanium.Web.Proxy.EventArguments;

namespace BlackFNRedirect;

internal sealed class RequestHandler
{
    private readonly ProxyConfiguration _config;

    public RequestHandler(ProxyConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task OnBeforeTunnelConnectAsync(object sender, TunnelConnectSessionEventArgs e)
    {
        string hostname = e.HttpClient.Request.RequestUri.Host;

        if (ShouldInterceptHost(hostname))
        {
            if (_config.EnableVerboseLogging)
            {
                Console.WriteLine($"[TUNNEL] {hostname}");
            }
            e.DecryptSsl = true;
        }

        await Task.CompletedTask;
    }

    public async Task OnRequestAsync(object sender, SessionEventArgs e)
    {
        string host = e.HttpClient.Request.RequestUri.Host;

        if (ShouldInterceptHost(host))
        {
            if (_config.EnableVerboseLogging)
            {
                Console.WriteLine($"[REQ] {e.HttpClient.Request.Method} {e.HttpClient.Request.RequestUri.AbsoluteUri}");
            }

            var originalUri = e.HttpClient.Request.RequestUri;
            var newUri = new Uri(originalUri.AbsoluteUri.Replace(host, _config.TargetHost));

            e.HttpClient.Request.RequestUri = newUri;
            e.HttpClient.Request.Host = _config.TargetHost;

            if (_config.EnableVerboseLogging)
            {
                Console.WriteLine($"[REDIRECT] {newUri.AbsoluteUri}");
            }
        }

        await Task.CompletedTask;
    }

    public async Task OnResponseAsync(object sender, SessionEventArgs e)
    {
        if (_config.EnableVerboseLogging && e.HttpClient.Request.RequestUri.Host.Contains(_config.TargetHost))
        {
            Console.WriteLine($"[RESP] {e.HttpClient.Response.StatusCode}");
        }

        await Task.CompletedTask;
    }

    private bool ShouldInterceptHost(string hostname)
    {
        return hostname.Contains(_config.SourcePattern, StringComparison.OrdinalIgnoreCase);
    }
}