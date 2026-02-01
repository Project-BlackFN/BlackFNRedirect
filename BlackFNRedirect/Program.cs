using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFNRedirect;

public static class Program
{
    private static ProxyService? _proxyService;
    private static readonly CancellationTokenSource _cts = new();

    public static async Task<int> Main(string[] args)
    {
        Console.CancelKeyPress += OnCancelKeyPress;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        try
        {
            var config = new ProxyConfiguration
            {
                ListenAddress = IPAddress.Any,
                Port = 8432,
                TargetHost = "ols.blackfn.ghost143.de",
                SourcePattern = ".ol.epicgames.com",
                EnableVerboseLogging = false
            };

            _proxyService = new ProxyService(config);
            await _proxyService.StartAsync(_cts.Token);

            Console.WriteLine($"Proxy listening on {config.ListenAddress}:{config.Port}");
            
            await Task.Delay(Timeout.Infinite, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Shutting down...");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            return 1;
        }
        finally
        {
            await ShutdownAsync();
        }

        return 0;
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        _cts.Cancel();
    }

    private static void OnProcessExit(object? sender, EventArgs e)
    {
        _cts.Cancel();
    }

    private static async Task ShutdownAsync()
    {
        if (_proxyService != null)
        {
            await _proxyService.StopAsync();
        }
        _cts.Dispose();
    }
}