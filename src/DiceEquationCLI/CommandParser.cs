namespace DiceEquationCLI;

/// <summary>
/// Constructs a Command instance with the name of the command and its arguments.
/// </summary>
/// <param name="Name">The name of the command should be a single keyword without any whitespace</param>
/// <param name="Args">Each argument should be a single word without any whitespace, since arguments are separated by spaces</param>
public record struct Command(string Name, string[] Args) { }

public static class CommandParser
{
    /// <summary>
    /// Parses a user-input command string into a Command object.
    /// The command name and its arguments must be whitespace delimited, the first word will be taken as the command name.
    /// </summary>
    /// <param name="command">A string containing user input</param>
    /// <returns>A Command object containing the command name and its arguments</returns>
    public static Command Parse(string command)
    {
        var commandArgs = command.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        commandArgs = commandArgs.Select(x => x.Trim()).ToArray();
        return new Command(commandArgs[0], commandArgs.Skip(1).ToArray());
    }
}