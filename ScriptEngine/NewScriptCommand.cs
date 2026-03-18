using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace POC_AI_Sample.ScriptEngine;

internal static class NewScriptCommand
{
    private static readonly string ScriptsRootPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

    public static int Handle(string[] args)
    {
        if (args.Length > 0)
        {
            Console.WriteLine("La commande 'new-script' ne prend pas d'argument.");
            Console.WriteLine("Usage : app new-script");
            return 1;
        }

        Directory.CreateDirectory(ScriptsRootPath);

        Console.WriteLine("Création interactive d'un nouveau script.");

        var scriptName = PromptForScriptName();
        var scriptDirectory = Path.Combine(ScriptsRootPath, scriptName);
        if (Directory.Exists(scriptDirectory))
        {
            Console.WriteLine($"Le script '{scriptName}' existe déjà : {scriptDirectory}");
            return 1;
        }

        var description = PromptRequiredValue("Description : ");
        var parameters = PromptForParameters();

        Directory.CreateDirectory(scriptDirectory);

        var configPath = Path.Combine(scriptDirectory, "config.json");
        var runPath = Path.Combine(scriptDirectory, "run.csx");

        var configuration = new ScriptConfiguration
        {
            Name = scriptName,
            Description = description,
            Parameters = parameters
        };

        var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(configPath, json + Environment.NewLine);
        File.WriteAllText(runPath, BuildRunTemplate(scriptName, description));

        Console.WriteLine("Fichiers générés :");
        Console.WriteLine($"  {configPath}");
        Console.WriteLine($"  {runPath}");

        OpenInEditor(configPath, runPath);
        return 0;
    }

    private static string PromptForScriptName()
    {
        while (true)
        {
            var scriptName = PromptRequiredValue("Nom système du script (sans espaces) : ");
            if (scriptName.Contains(' '))
            {
                Console.WriteLine("Le nom système ne doit pas contenir d'espaces.");
                continue;
            }

            if (scriptName.Any(char.IsWhiteSpace))
            {
                Console.WriteLine("Le nom système ne doit pas contenir d'espaces ou de tabulations.");
                continue;
            }

            return scriptName;
        }
    }

    private static List<ScriptParameter> PromptForParameters()
    {
        var parameters = new List<ScriptParameter>();

        while (AskYesNo("Ajouter un paramètre ? (y/n) : "))
        {
            var name = PromptRequiredValue("  Nom du paramètre : ");
            var description = PromptRequiredValue("  Description du paramètre : ");
            var required = AskYesNo("  Paramètre obligatoire ? (y/n) : ");

            parameters.Add(new ScriptParameter
            {
                Name = name,
                Description = description,
                Required = required
            });
        }

        return parameters;
    }

    private static string PromptRequiredValue(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            Console.WriteLine("Une valeur est requise.");
        }
    }

    private static bool AskYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (value is "y" or "yes" or "o" or "oui")
            {
                return true;
            }

            if (value is "n" or "no" or "non")
            {
                return false;
            }

            Console.WriteLine("Réponse attendue : y/n.");
        }
    }

    private static string BuildRunTemplate(string scriptName, string description)
    {
        return $"// Script: {scriptName}{Environment.NewLine}"
            + $"// Description: {description}{Environment.NewLine}"
            + "// Ajoutez ici la logique métier du script." + Environment.NewLine
            + Environment.NewLine
            + "Console.WriteLine(\"Hello from script skeleton.\");" + Environment.NewLine;
    }

    private static void OpenInEditor(params string[] filePaths)
    {
        var editor = Environment.GetEnvironmentVariable("EDITOR");
        if (!string.IsNullOrWhiteSpace(editor))
        {
            if (TryStartProcess(editor, filePaths))
            {
                return;
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            foreach (var filePath in filePaths)
            {
                if (!TryStartProcess("cmd", "/c", "start", string.Empty, filePath))
                {
                    Console.WriteLine($"Impossible d'ouvrir automatiquement : {filePath}");
                }
            }

            return;
        }

        if (IsCommandAvailable("code") && TryStartProcess("code", filePaths))
        {
            return;
        }

        Console.WriteLine("Ouverture automatique impossible. Fichiers à éditer :");
        foreach (var filePath in filePaths)
        {
            Console.WriteLine($"  {filePath}");
        }
    }

    private static bool IsCommandAvailable(string command)
    {
        var lookupCommand = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? $"where {command}"
            : $"command -v {command}";

        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "/bin/sh",
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"/c {lookupCommand}" : $"-c \"{lookupCommand}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            process?.WaitForExit();
            return process is { ExitCode: 0 };
        }
        catch
        {
            return false;
        }
    }

    private static bool TryStartProcess(string fileName, params string[] arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false
            };

            foreach (var argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            using var process = Process.Start(startInfo);
            return process is not null;
        }
        catch
        {
            return false;
        }
    }
}
