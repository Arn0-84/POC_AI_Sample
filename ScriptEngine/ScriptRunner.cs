using System.ComponentModel;
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
            ".ps1" => RunExternalProcess("pwsh", BuildPowerShellArguments(script.ScriptFilePath, parameters), script.DirectoryPath),
            ".sh" => RunExternalProcess("bash", BuildShellArguments(script.ScriptFilePath, parameters), script.DirectoryPath),
            ".csx" => SimulateExecution(script, parameters),
            _ => SimulateExecution(script, parameters)
        };
    }

    private static int SimulateExecution(ScriptDescriptor script, IDictionary<string, string> parameters)
    {
        Console.WriteLine($"Simulation d'exécution du script : {script.Name}");
        Console.WriteLine($"Fichier : {script.ScriptFilePath}");
        Console.WriteLine("Paramètres reçus :");

        if (parameters.Count == 0)
        {
            Console.WriteLine("  (aucun)");
            return 0;
        }

        foreach (var parameter in parameters)
        {
            Console.WriteLine($"  --{parameter.Key} = {parameter.Value}");
        }

        return 0;
    }

    private static int RunExternalProcess(string fileName, IEnumerable<string> arguments, string workingDirectory)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false
            };

            foreach (var argument in arguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }

            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            Console.WriteLine($"Impossible d'exécuter '{fileName}' : {ex.Message}");
            return 1;
        }
    }

    private static IEnumerable<string> BuildPowerShellArguments(string scriptPath, IDictionary<string, string> parameters)
    {
        yield return "-File";
        yield return scriptPath;

        foreach (var parameter in parameters)
        {
            yield return $"-{parameter.Key}";
            yield return parameter.Value;
        }
    }

    private static IEnumerable<string> BuildShellArguments(string scriptPath, IDictionary<string, string> parameters)
    {
        yield return scriptPath;

        foreach (var parameter in parameters)
        {
            yield return $"--{parameter.Key}";
            yield return parameter.Value;
        }
    }
}
