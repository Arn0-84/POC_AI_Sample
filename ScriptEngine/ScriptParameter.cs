namespace POC_AI_Sample.ScriptEngine;

internal sealed class ScriptParameter
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool Required { get; set; }
}
