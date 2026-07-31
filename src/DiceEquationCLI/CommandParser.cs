namespace DiceEquationCLI;

public record struct Command(string Name, string[] Args) { }

public static class CommandParser
{
    public static Command Parse(string command)
    {
        var commandArgs = command.Split(' ');
        return new Command(commandArgs[0], commandArgs.Skip(1).ToArray());
    }
}