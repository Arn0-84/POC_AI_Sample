using POC_AI_Sample.ScriptEngine;

namespace POC_AI_Sample;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0 || IsHelpCommand(args[0]))
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

    private static bool IsHelpCommand(string command)
        => command.Equals("help", StringComparison.OrdinalIgnoreCase)
           || command.Equals("/help", StringComparison.OrdinalIgnoreCase)
           || command.Equals("--help", StringComparison.OrdinalIgnoreCase)
           || command.Equals("-h", StringComparison.OrdinalIgnoreCase);

    private static int HandleUnknownCommand(string command)
    {
        Console.WriteLine($"Commande inconnue : {command}");
        Console.WriteLine();
        PrintGlobalHelp();
        return 1;
    }

    internal static void PrintGlobalHelp()
    {
        Console.WriteLine("Usage :");
        Console.WriteLine("  app help");
        Console.WriteLine("  app script <list|help|run> [...]");
        Console.WriteLine("  app new-script");
        Console.WriteLine();
        Console.WriteLine("Commandes principales :");
        Console.WriteLine("  script      Énumère, décrit et exécute les scripts disponibles.");
        Console.WriteLine("  new-script  Crée interactivement la structure d'un nouveau script.");
        Console.WriteLine("  help        Affiche cette aide globale.");
        Console.WriteLine();
        Console.WriteLine("Exemples :");
        Console.WriteLine("  app script list");
        Console.WriteLine("  app script help MyScript");
        Console.WriteLine("  app script run MyScript --input demo.txt --mode fast");
        Console.WriteLine("  app new-script");
        Console.WriteLine();
        Console.WriteLine("Utilisez 'app script list' pour voir tous les scripts disponibles.");
    }
}
