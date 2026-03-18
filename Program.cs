using POC_AI_Sample.ScriptEngine;

namespace POC_AI_Sample;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0 || IsHelpToken(args[0]))
        {
            PrintGlobalHelp();
            return 0;
        }

        var command = args[0].ToLowerInvariant();
        var commandArgs = args.Skip(1).ToArray();

        return command switch
        {
            "script" => ScriptCommand.Handle(commandArgs),
            "new-script" => NewScriptCommand.Handle(commandArgs),
            _ => HandleUnknownCommand(command)
        };
    }

    private static bool IsHelpToken(string value) =>
        value.Equals("help", StringComparison.OrdinalIgnoreCase)
        || value.Equals("/help", StringComparison.OrdinalIgnoreCase)
        || value.Equals("--help", StringComparison.OrdinalIgnoreCase)
        || value.Equals("-h", StringComparison.OrdinalIgnoreCase);

    private static int HandleUnknownCommand(string command)
    {
        Console.WriteLine($"Commande inconnue : {command}");
        Console.WriteLine();
        PrintGlobalHelp();
        return 1;
    }

    internal static void PrintGlobalHelp()
    {
        Console.WriteLine("Utilisation :");
        Console.WriteLine("  app help");
        Console.WriteLine("  app script <list|help|run> [...]");
        Console.WriteLine("  app new-script");
        Console.WriteLine();
        Console.WriteLine("Commandes principales :");
        Console.WriteLine("  script      Lister, afficher l'aide et exécuter les scripts déclarés.");
        Console.WriteLine("  new-script  Créer un nouveau script en mode interactif.");
        Console.WriteLine("  help        Afficher cette aide globale.");
        Console.WriteLine();
        Console.WriteLine("Exemples :");
        Console.WriteLine("  app script list");
        Console.WriteLine("  app script help MyScript");
        Console.WriteLine("  app script run MyScript --input fichier.txt --mode fast");
        Console.WriteLine("  app new-script");
        Console.WriteLine();
        Console.WriteLine("Astuce : utilisez 'app script list' pour voir les scripts disponibles.");
    }
}
