using System.Text.Json;

namespace POC_AI_Sample.ScriptEngine;

internal sealed class ScriptLoader
{
    private const string ConfigFileName = "config.json";
    private static readonly string[] SupportedScriptFiles = ["run.csx", "run.ps1", "run.sh"];
    private readonly string _scriptsRootPath;

    public ScriptLoader(string scriptsRootPath)
    {
        _scriptsRootPath = scriptsRootPath;
    }

    public IReadOnlyList<ScriptDescriptor> LoadAll()
    {
        if (!Directory.Exists(_scriptsRootPath))
        {
            Console.WriteLine($"Le dossier des scripts est introuvable : {_scriptsRootPath}");
            return [];
        }

        var scripts = new List<ScriptDescriptor>();

        foreach (var directory in Directory.GetDirectories(_scriptsRootPath))
        {
            if (TryLoadFromDirectory(directory, out var descriptor))
            {
                scripts.Add(descriptor);
            }
        }

        return scripts
            .OrderBy(script => script.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public ScriptDescriptor? LoadByName(string name)
    {
        return LoadAll().FirstOrDefault(script =>
            script.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryLoadFromDirectory(string directoryPath, out ScriptDescriptor descriptor)
    {
        descriptor = null!;

        var configPath = Path.Combine(directoryPath, ConfigFileName);
        if (!File.Exists(configPath))
        {
            Console.WriteLine($"Configuration absente pour le script '{Path.GetFileName(directoryPath)}' : {configPath}");
            return false;
        }

        ScriptConfiguration? configuration;
        try
        {
            var json = File.ReadAllText(configPath);
            configuration = JsonSerializer.Deserialize<ScriptConfiguration>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON invalide pour le script '{Path.GetFileName(directoryPath)}' : {ex.Message}");
            return false;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Impossible de lire la configuration du script '{Path.GetFileName(directoryPath)}' : {ex.Message}");
            return false;
        }

        if (configuration is null || string.IsNullOrWhiteSpace(configuration.Name))
        {
            Console.WriteLine($"Configuration invalide dans {configPath} : le champ 'name' est obligatoire.");
            return false;
        }

        var scriptFilePath = ResolveScriptFile(directoryPath);
        if (scriptFilePath is null)
        {
            Console.WriteLine($"Fichier d'exécution introuvable pour le script '{configuration.Name}' dans {directoryPath}.");
            return false;
        }

        descriptor = new ScriptDescriptor
        {
            Name = configuration.Name.Trim(),
            Description = configuration.Description?.Trim() ?? string.Empty,
            Parameters = configuration.Parameters ?? [],
            DirectoryPath = directoryPath,
            ScriptFilePath = scriptFilePath
        };

        return true;
    }

    private static string? ResolveScriptFile(string directoryPath)
    {
        return SupportedScriptFiles
            .Select(fileName => Path.Combine(directoryPath, fileName))
            .FirstOrDefault(File.Exists);
    }
}
