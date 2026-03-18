namespace POC_AI_Sample.ScriptEngine;

internal static class ScriptCommand
{
    private static readonly string ScriptsRootPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

    public static int Handle(string[] args)
    {
        if (args.Length == 0 || IsHelpToken(args[0]))
        {
            PrintHelp();
            return 0;
        }

        var loader = new ScriptLoader(ScriptsRootPath);
        var action = args[0].ToLowerInvariant();

        return action switch
        {
            "list" => ListScripts(loader),
            "help" => ShowScriptHelp(loader, args),
            "run" => RunScript(loader, args),
            _ => UnknownSubCommand(action)
        };
    }

    private static bool IsHelpToken(string value) =>
        value.Equals("help", StringComparison.OrdinalIgnoreCase)
        || value.Equals("/help", StringComparison.OrdinalIgnoreCase)
        || value.Equals("--help", StringComparison.OrdinalIgnoreCase)
        || value.Equals("-h", StringComparison.OrdinalIgnoreCase);

    private static int ListScripts(ScriptLoader loader)
    {
        var scripts = loader.LoadAll();
        if (scripts.Count == 0)
        {
            Console.WriteLine("Aucun script disponible.");
            return 0;
        }

        foreach (var script in scripts)
        {
            Console.WriteLine($"{script.Name} - {script.Description}");
        }

        return 0;
    }

    private static int ShowScriptHelp(ScriptLoader loader, string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage : app script help <nomScript>");
            return 1;
        }

        var script = loader.LoadByName(args[1]);
        if (script is null)
        {
            Console.WriteLine($"Script introuvable : {args[1]}");
            return 1;
        }

        Console.WriteLine($"Nom : {script.Name}");
        Console.WriteLine($"Description : {script.Description}");
        Console.WriteLine($"Répertoire : {script.DirectoryPath}");
        Console.WriteLine($"Fichier : {script.ScriptFilePath}");
        Console.WriteLine();
        Console.WriteLine("Paramètres :");

        if (script.Parameters.Count == 0)
        {
            Console.WriteLine("  Aucun paramètre.");
            return 0;
        }

        foreach (var parameter in script.Parameters)
        {
            var requiredLabel = parameter.Required ? "oui" : "non";
            Console.WriteLine($"  {parameter.Name} - {parameter.Description} (obligatoire : {requiredLabel})");
        }

        return 0;
    }

    private static int RunScript(ScriptLoader loader, string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage : app script run <nomScript> [--param value ...]");
            return 1;
        }

        var script = loader.LoadByName(args[1]);
        if (script is null)
        {
            Console.WriteLine($"Script introuvable : {args[1]}");
            return 1;
        }

        if (!TryParseNamedParameters(args.Skip(2).ToArray(), out var parameters, out var errorMessage))
        {
            Console.WriteLine(errorMessage);
            return 1;
        }

        var missingRequired = script.Parameters
            .Where(parameter => parameter.Required)
            .Where(parameter => !parameters.ContainsKey(parameter.Name))
            .Select(parameter => parameter.Name)
            .ToList();

        if (missingRequired.Count != 0)
        {
            Console.WriteLine($"Paramètres obligatoires manquants : {string.Join(", ", missingRequired)}");
            return 1;
        }

        return ScriptRunner.Run(script, parameters);
    }

    private static bool TryParseNamedParameters(
        string[] args,
        out Dictionary<string, string> parameters,
        out string errorMessage)
    {
        parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        errorMessage = string.Empty;

        for (var i = 0; i < args.Length; i += 2)
        {
            if (!args[i].StartsWith("--", StringComparison.Ordinal))
            {
                errorMessage = $"Paramètre invalide : {args[i]}. Les paramètres doivent utiliser la forme --nom valeur.";
                return false;
            }

            if (i + 1 >= args.Length)
            {
                errorMessage = $"Valeur manquante pour le paramètre {args[i]}";
                return false;
            }

            var parameterName = args[i][2..];
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                errorMessage = "Nom de paramètre vide après le préfixe '--'.";
                return false;
            }

            parameters[parameterName] = args[i + 1];
        }

        return true;
    }

    private static int UnknownSubCommand(string action)
    {
        Console.WriteLine($"Sous-commande inconnue pour 'script' : {action}");
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
