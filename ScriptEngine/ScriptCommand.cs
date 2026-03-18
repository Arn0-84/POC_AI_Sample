namespace POC_AI_Sample.ScriptEngine;

internal static class ScriptCommand
{
    public static int Handle(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
            return 1;
        }

        var loader = new ScriptLoader();
        var subCommand = args[0].ToLowerInvariant();

        return subCommand switch
        {
            "list" => HandleList(loader),
            "help" => HandleHelp(loader, args.Skip(1).ToArray()),
            "run" => HandleRun(loader, args.Skip(1).ToArray()),
            _ => HandleUnknownSubCommand(subCommand)
        };
    }

    private static int HandleList(ScriptLoader loader)
    {
        var scripts = loader.LoadAll();
        if (scripts.Count == 0)
        {
            Console.WriteLine($"Aucun script trouvé dans '{loader.GetScriptsRoot()}'.");
            return 0;
        }

        foreach (var script in scripts)
        {
            Console.WriteLine($"{script.Name} - {script.Description}");
        }

        return 0;
    }

    private static int HandleHelp(ScriptLoader loader, string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage : app script help <nomScript>");
            return 1;
        }

        var script = loader.FindByName(args[0]);
        if (script is null)
        {
            Console.WriteLine($"Script introuvable : {args[0]}");
            return 1;
        }

        Console.WriteLine($"Nom : {script.Name}");
        Console.WriteLine($"Description : {script.Description}");
        Console.WriteLine($"Répertoire : {script.DirectoryPath}");
        Console.WriteLine($"Fichier script : {script.ScriptFilePath}");
        Console.WriteLine();
        Console.WriteLine("Paramètres :");

        if (script.Parameters.Count == 0)
        {
            Console.WriteLine("  (aucun paramètre)");
            return 0;
        }

        foreach (var parameter in script.Parameters)
        {
            var requiredLabel = parameter.Required ? "obligatoire" : "optionnel";
            Console.WriteLine($"  --{parameter.Name} : {parameter.Description} [{requiredLabel}]");
        }

        return 0;
    }

    private static int HandleRun(ScriptLoader loader, string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage : app script run <nomScript> [--param value ...]");
            return 1;
        }

        var scriptName = args[0];
        var script = loader.FindByName(scriptName);
        if (script is null)
        {
            Console.WriteLine($"Script introuvable : {scriptName}");
            return 1;
        }

        if (!TryParseNamedParameters(args.Skip(1).ToArray(), out var parameters, out var errorMessage))
        {
            Console.WriteLine(errorMessage);
            return 1;
        }

        var requiredParameters = script.Parameters
            .Where(parameter => parameter.Required)
            .Select(parameter => parameter.Name)
            .ToList();

        var missingParameters = requiredParameters
            .Where(required => !parameters.ContainsKey(required))
            .ToList();

        if (missingParameters.Count > 0)
        {
            Console.WriteLine($"Paramètres obligatoires manquants : {string.Join(", ", missingParameters.Select(name => $"--{name}"))}");
            return 1;
        }

        return ScriptRunner.Run(script, parameters);
    }

    private static bool TryParseNamedParameters(string[] args, out Dictionary<string, string> parameters, out string errorMessage)
    {
        parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        errorMessage = string.Empty;

        for (var index = 0; index < args.Length; index++)
        {
            var token = args[index];
            if (!token.StartsWith("--", StringComparison.Ordinal))
            {
                errorMessage = $"Argument inattendu : {token}. Les paramètres doivent être fournis sous la forme --nom valeur.";
                return false;
            }

            var parameterName = token[2..];
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                errorMessage = "Nom de paramètre vide après '--'.";
                return false;
            }

            if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                errorMessage = $"Valeur manquante pour le paramètre --{parameterName}.";
                return false;
            }

            parameters[parameterName] = args[index + 1];
            index++;
        }

        return true;
    }

    private static int HandleUnknownSubCommand(string subCommand)
    {
        Console.WriteLine($"Sous-commande script inconnue : {subCommand}");
        PrintHelp();
        return 1;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Usage :");
        Console.WriteLine("  app script list");
        Console.WriteLine("  app script help <nomScript>");
        Console.WriteLine("  app script run <nomScript> [--param value ...]");
    }
}
