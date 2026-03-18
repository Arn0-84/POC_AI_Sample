using System.Text.Json.Serialization;

namespace POC_AI_Sample.ScriptEngine;

internal sealed class ScriptDescriptor
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public List<ScriptParameter> Parameters { get; set; } = [];

    public string DirectoryPath { get; set; } = string.Empty;

    public string ScriptFilePath { get; set; } = string.Empty;
}

internal sealed class ScriptParameter
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("required")]
    public bool Required { get; set; }
}
