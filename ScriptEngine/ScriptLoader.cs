using System.Text.Json;

namespace POC_AI_Sample.ScriptEngine;

internal sealed class ScriptLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly string _scriptsRoot;

    public ScriptLoader(string? scriptsRoot = null)
    {
        _scriptsRoot = scriptsRoot ?? ResolveDefaultScriptsRoot();
    }

    public IReadOnlyList<ScriptDescriptor> LoadAll()
    {
        if (!Directory.Exists(_scriptsRoot))
        {
            return [];
        }

        var scripts = new List<ScriptDescriptor>();

        foreach (var directory in Directory.GetDirectories(_scriptsRoot))
        {
            var directoryName = Path.GetFileName(directory);

            try
            {
                scripts.Add(LoadFromDirectory(directory));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Impossible de charger le script '{directoryName}' : {ex.Message}");
            }
        }

        return scripts.OrderBy(script => script.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public ScriptDescriptor? FindByName(string name)
        => LoadAll().FirstOrDefault(script => script.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public ScriptDescriptor LoadFromDirectory(string directory)
    {
        var configPath = Path.Combine(directory, "config.json");
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"config.json introuvable dans '{directory}'.", configPath);
        }

        ScriptDescriptor? descriptor;
        try
        {
            descriptor = JsonSerializer.Deserialize<ScriptDescriptor>(File.ReadAllText(configPath), JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"JSON invalide dans '{configPath}' : {ex.Message}", ex);
        }

        if (descriptor is null)
        {
            throw new InvalidOperationException($"Le fichier '{configPath}' ne contient pas de configuration exploitable.");
        }

        if (string.IsNullOrWhiteSpace(descriptor.Name))
        {
            throw new InvalidOperationException($"Le champ 'name' est obligatoire dans '{configPath}'.");
        }

        descriptor.Description = descriptor.Description?.Trim() ?? string.Empty;
        descriptor.Parameters ??= [];
        descriptor.DirectoryPath = directory;
        descriptor.ScriptFilePath = ResolveScriptFilePath(directory);

        foreach (var parameter in descriptor.Parameters.Where(parameter => string.IsNullOrWhiteSpace(parameter.Name)))
        {
            throw new InvalidOperationException($"Chaque paramètre doit avoir un champ 'name' dans '{configPath}'.");
        }

        return descriptor;
    }

    public string GetScriptsRoot() => _scriptsRoot;

    public static JsonSerializerOptions GetJsonOptions() => JsonOptions;

    private static string ResolveDefaultScriptsRoot()
    {
        var currentDirectoryRoot = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");
        if (Directory.Exists(currentDirectoryRoot))
        {
            return currentDirectoryRoot;
        }

        return Path.Combine(AppContext.BaseDirectory, "Scripts");
    }

    private static string ResolveScriptFilePath(string directory)
    {
        var supportedNames = new[] { "run.csx", "run.ps1", "run.sh", "run.cmd", "run.bat" };

        foreach (var fileName in supportedNames)
        {
            var candidate = Path.Combine(directory, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException($"Aucun fichier script pris en charge trouvé dans '{directory}'. Attendu : {string.Join(", ", supportedNames)}.");
    }
}
