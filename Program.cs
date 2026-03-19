using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Point d'entrée du POC AI.
/// Les actions disponibles sont centralisées avec leur description
/// pour pouvoir être réutilisées par la commande /help.
/// </summary>
internal static class Program
{
    private const string HelpCommand = "/help";

    private static readonly Dictionary<string, ActionDefinition> Actions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Hello"] = new ActionDefinition(
            "Hello",
            "Affiche 'World !'.",
            "Hello",
            static () => "World !")
    };

    private static void Main(string[] args)
    {
        // Si aucun argument n'est fourni, on ne déclenche aucune action.
        if (args.Length == 0)
        {
            return;
        }

        if (IsHelpCommand(args[0]))
        {
            if (args.Length == 1)
            {
                Console.WriteLine(BuildGlobalHelp());
                return;
            }

            Console.WriteLine(BuildActionHelp(args[1]));
            return;
        }

        var command = args[0];

        if (args.Length > 1 && IsHelpCommand(args[1]))
        {
            Console.WriteLine(BuildActionHelp(command));
            return;
        }

        if (Actions.TryGetValue(command, out var action))
        {
            Console.WriteLine(action.Execute());
        }
    }

    private static bool IsHelpCommand(string value) =>
        string.Equals(value, HelpCommand, StringComparison.OrdinalIgnoreCase);

    private static string BuildGlobalHelp()
    {
        var lines = new List<string>
        {
            "Aide disponible :",
            $"- {HelpCommand} : affiche toutes les actions disponibles.",
            $"- {HelpCommand} <action> : affiche l'aide d'une action précise.",
            "- <action> /help : affiche l'aide de l'action placée avant /help.",
            string.Empty,
            "Actions :"
        };

        lines.AddRange(
            Actions.Values
                .OrderBy(action => action.Name, StringComparer.OrdinalIgnoreCase)
                .Select(action => $"- {action.Syntax} : {action.Description}"));

        return string.Join(Environment.NewLine, lines);
    }

    private static string BuildActionHelp(string actionName)
    {
        if (!Actions.TryGetValue(actionName, out var action))
        {
            return $"Action inconnue : {actionName}";
        }

        return string.Join(
            Environment.NewLine,
            $"Aide pour l'action '{action.Name}' :",
            $"- Description : {action.Description}",
            $"- Syntaxe : {action.Syntax}",
            $"- Aide dédiée : {HelpCommand} {action.Name} ou {action.Name} {HelpCommand}");
    }

    /// <summary>
    /// Structure simple représentant une action et sa description.
    /// </summary>
    private sealed record ActionDefinition(string Name, string Description, string Syntax, Func<string> Execute);
}
