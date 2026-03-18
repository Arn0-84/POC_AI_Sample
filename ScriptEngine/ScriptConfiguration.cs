using System.Text.Json.Serialization;

namespace POC_AI_Sample.ScriptEngine;

internal sealed class ScriptConfiguration
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public List<ScriptParameter> Parameters { get; set; } = [];
}
