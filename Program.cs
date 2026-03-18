using System;
using System.Collections.Generic;

/// <summary>
/// Point d'entrée du POC AI.
/// Les actions disponibles sont centralisées avec leur description
/// pour pouvoir être réutilisées plus tard par une fonction HELP.
/// </summary>
internal static class Program
{
    private static readonly Dictionary<string, ActionDefinition> Actions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Hello"] = new ActionDefinition(
            "Répond 'World !' quand l'argument 'Hello' est fourni",
            static () => "World !")
    };

    private static void Main(string[] args)
    {
        // Si aucun argument n'est fourni, on ne déclenche aucune action.
        if (args.Length == 0)
        {
            return;
        }

        var command = args[0];

        if (Actions.TryGetValue(command, out var action))
        {
            Console.WriteLine(action.Execute());
        }
    }

    /// <summary>
    /// Structure simple représentant une action et sa description.
    /// </summary>
    private sealed record ActionDefinition(string Description, Func<string> Execute);
}
