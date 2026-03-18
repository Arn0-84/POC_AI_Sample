using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace POC_AI_Sample.ScriptEngine;

internal static class NewScriptCommand
{
    private static readonly Regex ScriptNamePattern = new("^[A-Za-z0-9_-]+$", RegexOptions.Compiled);

    public static int Handle(string[] args)
    {
        if (args.Length > 0)
        {
            Console.WriteLine("La commande 'new-script' ne prend pas d'arguments.");
            Console.WriteLine("Usage : app new-script");
            return 1;
        }

        var loader = new ScriptLoader();
        var scriptsRoot = loader.GetScriptsRoot();
        Directory.CreateDirectory(scriptsRoot);

        Console.WriteLine("Création interactive d'un nouveau script.");

        var scriptName = PromptForScriptName(scriptsRoot);
        var description = PromptNonEmpty("Description : ");
        var parameters = PromptParameters();

        var scriptDirectory = Path.Combine(scriptsRoot, scriptName);
        Directory.CreateDirectory(scriptDirectory);

        var configPath = Path.Combine(scriptDirectory, "config.json");
        var runPath = Path.Combine(scriptDirectory, "run.csx");

        var descriptor = new ScriptDescriptor
        {
            Name = scriptName,
            Description = description,
            Parameters = parameters,
            DirectoryPath = scriptDirectory,
            ScriptFilePath = runPath
        };

        File.WriteAllText(configPath, JsonSerializer.Serialize(descriptor, ScriptLoader.GetJsonOptions()));
        File.WriteAllText(runPath, BuildScriptTemplate(descriptor));

        Console.WriteLine("Script créé avec succès :");
        Console.WriteLine($"  {configPath}");
        Console.WriteLine($"  {runPath}");

        OpenInEditor(configPath, runPath);
        return 0;
    }

    private static string PromptForScriptName(string scriptsRoot)
    {
        while (true)
        {
            var name = PromptNonEmpty("Nom système du script (sans espaces) : ");
            if (!ScriptNamePattern.IsMatch(name))
            {
                Console.WriteLine("Le nom doit contenir uniquement des lettres, chiffres, tirets ou underscores.");
                continue;
            }

            var scriptDirectory = Path.Combine(scriptsRoot, name);
            if (Directory.Exists(scriptDirectory))
            {
                Console.WriteLine($"Le script '{name}' existe déjà.");
                continue;
            }

            return name;
        }
    }

    private static List<ScriptParameter> PromptParameters()
    {
        var parameters = new List<ScriptParameter>();

        while (PromptYesNo("Ajouter un paramètre ? (y/n) : "))
        {
            var parameter = new ScriptParameter
            {
                Name = PromptNonEmpty("  Nom du paramètre : "),
                Description = PromptNonEmpty("  Description du paramètre : "),
                Required = PromptYesNo("  Obligatoire ? (y/n) : ")
            };

            parameters.Add(parameter);
        }

        return parameters;
    }

    private static string PromptNonEmpty(string label)
    {
        while (true)
        {
            Console.Write(label);
            var value = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            Console.WriteLine("Une valeur est requise.");
        }
    }

    private static bool PromptYesNo(string label)
    {
        while (true)
        {
            Console.Write(label);
            var answer = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (answer is "y" or "yes" or "o" or "oui")
            {
                return true;
            }

            if (answer is "n" or "no" or "non")
            {
                return false;
            }

            Console.WriteLine("Répondez par y/n.");
        }
    }

    private static string BuildScriptTemplate(ScriptDescriptor descriptor)
    {
        return $$"""
// Script: {{descriptor.Name}}
// Décrivez ici la logique métier du script.
// Les paramètres sont décrits dans config.json.

Console.WriteLine("Hello from {{descriptor.Name}}.");
""";
    }

    private static void OpenInEditor(params string[] filePaths)
    {
        var editor = Environment.GetEnvironmentVariable("EDITOR");
        if (!string.IsNullOrWhiteSpace(editor))
        {
            if (TryStart(editor, filePaths))
            {
                Console.WriteLine($"Fichiers ouverts avec EDITOR={editor}.");
                return;
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (TryStart("cmd", ["/c", "start", string.Empty, ..filePaths]))
            {
                Console.WriteLine("Fichiers ouverts avec 'start'.");
                return;
            }
        }

        if (TryStart("code", filePaths))
        {
            Console.WriteLine("Fichiers ouverts avec VS Code.");
            return;
        }

        Console.WriteLine("Ouverture automatique impossible. Fichiers à éditer :");
        foreach (var filePath in filePaths)
        {
            Console.WriteLine($"  {filePath}");
        }
    }

    private static bool TryStart(string fileName, params string[] arguments)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false
            };

            foreach (var argument in arguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }

            return process.Start();
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            return false;
        }
    }
}
