namespace DiceEquationCLI;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to the Dice Equation CLI!");
        Console.WriteLine("Type 'help' for a list of available commands.");
		var api = new DiceEquationAPI(Console.In, Console.Out);
        api.Run();
    }
}