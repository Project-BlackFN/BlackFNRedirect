using System.Text.Json.Serialization;

namespace BlackFNRedirect;

public sealed class ConfigurationModel
{
    [JsonPropertyName("targetHost")]
    public string TargetHost { get; set; } = string.Empty;
    
    [JsonPropertyName("listenPort")]
    public int ListenPort { get; set; } = 8432;
}