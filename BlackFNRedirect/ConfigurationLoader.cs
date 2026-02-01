using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlackFNRedirect;

public static class ConfigurationLoader
{
    private const string ConfigFileName = "config.json";

    public static async Task<ConfigurationModel> LoadAsync()
    {
        if (!File.Exists(ConfigFileName))
        {
            throw new FileNotFoundException($"Configuration file '{ConfigFileName}' not found.");
        }

        try
        {
            string jsonContent = await File.ReadAllTextAsync(ConfigFileName);
            var config = JsonSerializer.Deserialize<ConfigurationModel>(jsonContent);

            if (config == null)
            {
                throw new InvalidOperationException("Failed to deserialize configuration.");
            }

            ValidateConfiguration(config);
            return config;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid JSON in configuration file: {ex.Message}", ex);
        }
    }

    private static void ValidateConfiguration(ConfigurationModel config)
    {
        if (string.IsNullOrWhiteSpace(config.TargetHost))
        {
            throw new InvalidOperationException("TargetHost must be specified in configuration.");
        }

        if (config.ListenPort <= 0 || config.ListenPort > 65535)
        {
            throw new InvalidOperationException("ListenPort must be between 1 and 65535.");
        }
    }

    public static async Task CreateDefaultConfigAsync()
    {
        var defaultConfig = new ConfigurationModel
        {
            TargetHost = "ols.blackfn.ghost143.de",
            ListenPort = 8432
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string jsonContent = JsonSerializer.Serialize(defaultConfig, options);
        await File.WriteAllTextAsync(ConfigFileName, jsonContent);
    }
}