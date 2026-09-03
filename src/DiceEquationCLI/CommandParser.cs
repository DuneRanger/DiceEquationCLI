namespace DiceEquationCLI;

public record struct Command(string Name, string[] Args) { }

public static class CommandParser
{
    public static Command Parse(string command)
    {
        var commandArgs = command.Split(' ');
        commandArgs = commandArgs.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
        return new Command(commandArgs[0], commandArgs.Skip(1).ToArray());
    }
}