namespace DiceEquationCLI;

public record struct Command(string Name, string[] Args) { }

public static class CommandParser
{
    public static Command Parse(string command)
    {
        var commandArgs = command.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        commandArgs = commandArgs.Select(x => x.Trim()).ToArray();
        return new Command(commandArgs[0], commandArgs.Skip(1).ToArray());
    }
}