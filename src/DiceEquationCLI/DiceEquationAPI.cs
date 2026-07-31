using System.Globalization;

namespace DiceEquationCLI;

public class DiceEquationAPI
{
    private Dictionary<string, Action<string[]>> _commands = new();
    private Dictionary<string, string> _commandHelpMessages = new();
    private EquationManager _equationManager = new();
    
    private TextReader _inputReader;
    private TextWriter _outputWriter;
    private bool _exit = false;

    public DiceEquationAPI(TextReader input, TextWriter output)
    {
        _inputReader = input;
        _outputWriter = output;
        AddBaseCommands();
    }
    
    private void AddBaseCommands()
    {
        _commands.Add("help", PrintHelp);
        _commands.Add("exit", ExitProgram);
        _commands.Add("save", SaveEquation);
        _commands.Add("delete", DeleteEquation);
        _commands.Add("evaluate", EvaluateEquation);
        _commands.Add("show", ShowEquationString);
        _commands.Add("prob", ShowEquationDistribution);
        _commands.Add("export", ExportEquations);
        _commands.Add("import", ImportEquations);
        
        _commandHelpMessages.Add("help", """
                                         help
                                            Prints this help message
                                         """);
        _commandHelpMessages.Add("exit", """
                                         exit
                                            Exits the program
                                         """);
        _commandHelpMessages.Add("save", """
                                         save [identifier] = [expression] 
                                             Saves an expression under the given identifier
                                             If the identifier already exists, the expression is overwritten
                                             Spaces in the expression and around the '=' will be ignored
                                         """);
        _commandHelpMessages.Add("delete", """
                                           del all|[identifier]
                                               Deletes a saved expression with the given identifier
                                           """);
        _commandHelpMessages.Add("evaluate", """
                                             eval all|[identifier]
                                                 Evaluates and prints the result of the expression with the given identifier
                                                 The first evaluation may take longer, due to optimizing the expression
                                             """);
        _commandHelpMessages.Add("show", """
                                         show all|[identifier]
                                             Prints the string representation of the expression with the given identifier
                                         """);
        _commandHelpMessages.Add("prob", """
                                         prob [identifier]
                                             Prints the probability distribution of the expression with the given identifier
                                         """);
        _commandHelpMessages.Add("export", """
                                           export [filename]
                                               Prints all saved expressions in string form to the named file
                                               Existing file contents are overwritten
                                           """);
        _commandHelpMessages.Add("import", """
                                           import [filename]
                                               Imports all valid expressions from the named file
                                               Existing identifiers are NOT overwritten and an error message will be printed
                                               An error message will be printed for each invalid line in the file
                                               The whole file will be attempted to be imported, even if an error occurs 
                                           """);
    }

    private void AssertArgCount(string[] args, int expectedCount)
    {
        if (args.Length != expectedCount)
        {
            throw new ArgumentException("Invalid argument count: " + String.Join(" ", args));
        }
    }

    private void AssertValidIdentifier(string identifier)
    {
        bool isValid = true;
        foreach (char c in identifier)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
            {
                isValid = false;
                break;
            }
        }
        if (!isValid || char.IsDigit(identifier[0])) throw new ArgumentException("Invalid identifier: " + identifier);
    }

    public void Run()
    {
        while (true)
        {
            if (_exit) break;
            _outputWriter.Write("> ");
            string? input = _inputReader.ReadLine();
            if (input == null) continue;
            var command = CommandParser.Parse(input);
            _commands.TryGetValue(command.Name, out var commandAction);
            if (commandAction == null)
            {
                _outputWriter.WriteLine("Unknown command: " + command.Name);
                commandAction = PrintHelp;
            }

            try
            {
                commandAction(command.Args);
            }
            catch (ArgumentException e)
            {
                _outputWriter.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                _outputWriter.WriteLine("Unexpected Error: " + e.Message);
            }
        }
    }
    
    public void PrintHelp(string[] args)
    {
        AssertArgCount(args, 0);
        _outputWriter.WriteLine("Available commands:");
        foreach (var command in _commands.Keys)
        {
            _outputWriter.WriteLine(_commandHelpMessages[command]);
        }

        _outputWriter.WriteLine("""
                                Note: Commands are case-insensitive, and arguments are separated by spaces.
                                      Valid identifiers may contain letters, numbers and underscores. Though they cannot start with a number.
                                """);
    }

    public void ExitProgram(string[] args)
    {
        AssertArgCount(args, 0);
        _exit = true;
    }
    
    public void SaveEquation(string[] args)
    {
        string fullString = String.Join("", args);
        var nameExpression = fullString.Split('=');
        string name = nameExpression[0].Trim();
        string expression = nameExpression[1].Trim();
        AssertValidIdentifier(name);
        throw new NotImplementedException();
    }

    public void DeleteEquation(string[] args)
    {
        AssertArgCount(args, 1);
        if (args[0] == "all")
        {
            _equationManager.ClearEquations();
        }
        else
        {
            AssertValidIdentifier(args[0]);
            _equationManager.DeleteEquation(args[0]);
        }
    }

    public void EvaluateEquation(string[] args)
    {
        AssertArgCount(args, 1);
        if (args[0] == "all")
        {
            var enumerator = _equationManager.EnumerateEquations();
            foreach ((string name, Equation expression) in enumerator)
            {
                _outputWriter.WriteLine(name + ": " + expression.Evaluate().ToString(CultureInfo.CurrentCulture));
            }
        }
        else
        {
            AssertValidIdentifier(args[0]);
            var equation = _equationManager.GetEquation(args[0]);
            _outputWriter.WriteLine(equation.Evaluate().ToString(CultureInfo.CurrentCulture));
        }
    }
    
    public void ShowEquationString(string[] args)
    {
        AssertArgCount(args, 1);
        if (args[0] == "all")
        {
            var enumerator = _equationManager.EnumerateEquations();
            foreach ((string name, Equation expression) in enumerator)
            {
                _outputWriter.WriteLine(name + ": " + expression.ToString());
            }
        }
        else
        {
            AssertValidIdentifier(args[0]);
            var equation = _equationManager.GetEquation(args[0]);
            _outputWriter.WriteLine(equation.ToString());
        }
    }
    
    public void ShowEquationDistribution(string[] args)
    {
        AssertArgCount(args, 1);
        AssertValidIdentifier(args[0]);
        var equation = _equationManager.GetEquation(args[0]);
        _outputWriter.WriteLine(equation.GetDistribution().ToFormattedString(indentLevel: 1));
    }
    
    public void ExportEquations(string[] args)
    {
        AssertArgCount(args, 1);
        if (File.Exists(args[0])) _outputWriter.WriteLine($"Warning: {args[0]} already exists. Overwriting Contents.");
        else File.Create(args[0]);
        var fileHandler = new StreamWriter(args[0]);
        foreach (var equation in _equationManager.EnumerateEquations())
        {
            fileHandler.WriteLine(equation.Value.ToString());
        }
        fileHandler.Close();
    }

    public void ImportEquations(string[] args)
    {
        AssertArgCount(args, 1);
        if (!File.Exists(args[0])) throw new ArgumentException("File does not exist: " + args[0]);
        var fileHandler = new StreamReader(args[0]);
        string line;
        while ((line = fileHandler.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.Length == 0 || line[0] == '#') continue;
            try
            {
                var name = line.Split('=')[0].Trim();
                if (_equationManager.HasEquation(name))
                {
                    _outputWriter.WriteLine($"Warning: {name} already exists. Skipping");
                    continue;
                }
                SaveEquation(line.Split(' '));
            }
            catch (Exception e)
            {
                _outputWriter.WriteLine($"Error parsing line: {line}: {e.Message}");
            }
        }
    }
}