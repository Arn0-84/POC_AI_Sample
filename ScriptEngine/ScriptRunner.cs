using System.Diagnostics;
using System.Runtime.InteropServices;

namespace POC_AI_Sample.ScriptEngine;

internal static class ScriptRunner
{
    public static int Run(ScriptDescriptor script, IDictionary<string, string> parameters)
    {
        var extension = Path.GetExtension(script.ScriptFilePath).ToLowerInvariant();

        return extension switch
        {
            ".ps1" => RunExternal(script, parameters, "pwsh", BuildPowerShellArguments(script, parameters)),
            ".sh" when !RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                => RunExternal(script, parameters, "/bin/sh", BuildShellArguments(script, parameters)),
            ".csx" => SimulateRun(script, parameters),
            _ => UnsupportedExtension(script)
        };
    }

    private static int SimulateRun(ScriptDescriptor script, IDictionary<string, string> parameters)
    {
        Console.WriteLine($"Simulation de l'exécution du script : {script.Name}");
        Console.WriteLine($"Fichier : {script.ScriptFilePath}");

        if (parameters.Count == 0)
        {
            Console.WriteLine("Paramètres : aucun");
            return 0;
        }

        Console.WriteLine("Paramètres :");
        foreach (var parameter in parameters.OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"  --{parameter.Key} = {parameter.Value}");
        }

        return 0;
    }

    private static int UnsupportedExtension(ScriptDescriptor script)
    {
        Console.WriteLine($"Extension de script non supportée : {Path.GetExtension(script.ScriptFilePath)}");
        return 1;
    }

    private static int RunExternal(ScriptDescriptor script, IDictionary<string, string> parameters, string fileName, IEnumerable<string> arguments)
    {
        try
        {
            var argumentList = arguments.ToList();
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false
            };

            foreach (var argument in argumentList)
            {
                startInfo.ArgumentList.Add(argument);
            }

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                Console.WriteLine($"Impossible de démarrer le processus '{fileName}'.");
                return 1;
            }

            process.WaitForExit();
            return process.ExitCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'exécution du script '{script.Name}' : {ex.Message}");
            return 1;
        }
    }

    private static IEnumerable<string> BuildPowerShellArguments(ScriptDescriptor script, IDictionary<string, string> parameters)
    {
        yield return "-File";
        yield return script.ScriptFilePath;

        foreach (var parameter in parameters)
        {
            yield return $"-{parameter.Key}";
            yield return parameter.Value;
        }
    }

    private static IEnumerable<string> BuildShellArguments(ScriptDescriptor script, IDictionary<string, string> parameters)
    {
        yield return script.ScriptFilePath;

        foreach (var parameter in parameters)
        {
            yield return $"--{parameter.Key}";
            yield return parameter.Value;
        }
    }
}
