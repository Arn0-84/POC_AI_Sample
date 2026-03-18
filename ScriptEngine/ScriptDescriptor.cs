namespace POC_AI_Sample.ScriptEngine;

internal sealed class ScriptDescriptor
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<ScriptParameter> Parameters { get; set; } = [];

    public string DirectoryPath { get; set; } = string.Empty;

    public string ScriptFilePath { get; set; } = string.Empty;
}
