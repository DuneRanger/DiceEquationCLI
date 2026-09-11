using System.Globalization;

namespace DiceEquationCLI;

public class DiceEquationAPI
{
    /// <summary>
    /// Maps command names to their implemented callable function
    /// </summary>
    private Dictionary<string, Action<string[]>> _commands = new();
    /// <summary>
    /// Maps command names to their help messages
    /// </summary>
    private Dictionary<string, string> _commandHelpMessages = new();
    private EquationManager _equationManager = new();

    private TextReader _inputReader;
    private TextWriter _outputWriter;
    /// <summary>
    /// A flag marking whether the API should stop ruuning (i.e. exit the Run function)
    /// </summary>
    private bool _exit = false;

    /// <summary>
    /// Initializes a new instance of the DiceEquationAPI class.
    /// It contains its own EquationManager and command mappings
    /// </summary>
    /// <param name="input">The input from which the API will read user commands, when Run is called</param>
    /// <param name="output">The output to which the API will write its responses, from any function</param>
    public DiceEquationAPI(TextReader input, TextWriter output)
    {
        _inputReader = input;
        _outputWriter = output;
        AddBaseCommands();
    }

	/// <summary>
	/// Adds what is considered the base set of commands to the API, which are always available
	/// Both the command Action and its help message are added for:
	/// 'help', 'exit', 'save', 'del', 'eval', 'show', 'prob', 'export', and 'import'
	/// </summary>
	private void AddBaseCommands()
    {
        _commands.Add("help", PrintHelp);
        _commands.Add("exit", ExitProgram);
        _commands.Add("save", SaveEquation);
        _commands.Add("del", DeleteEquation);
        _commands.Add("eval", EvaluateEquation);
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
        _commandHelpMessages.Add("del", """
                                           del all|[identifier]
                                               Deletes a saved expression with the given identifier
                                           """);
        _commandHelpMessages.Add("eval", """
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

	/// <summary>
	/// Checks if the number of arguments matches the expected count
	/// </summary>
	/// <param name="args">An array of arguments to check</param>
	/// <param name="expectedCount">The expected number of arguments. Should be a non-negative integer.</param>
	/// <returns>Aan ArgumentException with an error message is thrown if false.</returns>
    /// <exception cref="ArgumentException"></exception>
	private void AssertArgCount(string[] args, int expectedCount)
    {
        if (args.Length != expectedCount)
        {
            throw new ArgumentException($"Invalid argument count (expected {expectedCount}): " + String.Join(" ", args));
        }
    }

	/// <summary>
	/// Checks if the identifier is valid.
	/// A valid identifier may contain letters, numbers and underscores, but cannot start with a number.
	/// </summary>
	/// <param name="identifier">The identifier to check</param>
	/// <returns>An ArgumentException with an error message is thrown if false.</returns>
    /// <exception cref="ArgumentException"></exception>
	public void AssertValidIdentifier(string identifier)
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
        if (!isValid) throw new ArgumentException($"Invalid identifier: '{identifier}'\nIdentifiers may only contain letters, numbers and underscores.");
        if (char.IsDigit(identifier[0])) throw new ArgumentException($"Invalid identifier: '{identifier}'\nIdentifiers cannot start with a number.");
	}

	/// <summary>
	/// Prompts the user for a yes/no confirmation.
	/// The message given will be printed to outputWriter with (y/n): appended to the end.
	/// </summary>
	/// <param name="message">The message to display requesting confirmation</param>
	/// <returns>True, if the user responds with 'y' or 'yes'</returns>
	private bool PromptConfirmation(string message)
	{
		_outputWriter.Write(message + " (y/n): ");
		string? input = _inputReader.ReadLine();
		if (input == null) return false;
		input = input.Trim().ToLower();
		return input == "y" || input == "yes";
	}

	/// <summary>
	/// Runs the main loop of the application.
	/// The input is continuously read from inputReader until the exit flag is set to true.
	/// </summary>
	/// <returns>Nothing should be thrown, unless a problem occurs with the inputReader</returns>
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
                _outputWriter.WriteLine("Error: " + e.Message);
            }
        }
    }

	/// <summary>
	/// Writes the whole help message to the outputWriter, which includes all available commands and their usage.
	/// </summary>
	/// <param name="args">Expected to be an empty array, but it is not validated</param>
	/// <returns>Nothing should be thrown, unless a problem occurs with the outputWriter</returns>
	public void PrintHelp(string[] args)
    {
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

	/// <summary>
	/// Sets the exit flag to true, which will cause the Run function to exit its loop and return.
	/// </summary>
	/// <param name="args">Expected to be an empty array.</param>
	/// <returns>An ArgumentException is thrown if the args array is not empty.</returns>
	/// <exception cref="ArgumentException"></exception>
	public void ExitProgram(string[] args)
    {
        AssertArgCount(args, 0);
        _exit = true;
    }

	/// <summary>
	/// Saves an equation to the equation manager.
	/// The equation is expected to be in the form of "identifier = expression".
	/// Everything before the first '=' is taken as the identifier, and everything after is taken as the expression.
	/// </summary>
	/// <param name="args">The arguments containing the equation to save. The equation and identifier may be split into any number of arguments.</param>
	/// <returns>An ArgumentException is thrown if the arguments do not match the expected format.</returns>
    /// <exception cref="ArgumentException"></exception>
	public void SaveEquation(string[] args)
    {
        string fullString = String.Join("", args);
        var nameExpression = fullString.Split('=');
        if (nameExpression.Length != 2) throw new ArgumentException("Equation should contain a single '=' between the identifier and equation");

        string name = nameExpression[0];
        string expression = nameExpression[1];
        AssertValidIdentifier(name);

        _equationManager.AddEquation(name, expression);
        _outputWriter.WriteLine($"Equation '{name}' saved.");
	}

	/// <summary>
	/// Deletes an equation from the equation manager.
	/// Accepts either a single identifier to delete, or the keyword "all" to delete all equations.
	/// </summary>
	/// <param name="args">Expects only a single argument.</param>
	/// <returns>An ArgumentException is thrown if the arguments do not match the expected format.</returns>
	/// <exception cref="ArgumentException"></exception>
	public void DeleteEquation(string[] args)
	{
		AssertArgCount(args, 1);
        if (args[0] == "all")
        {
            if (!PromptConfirmation("Are you sure you want to delete all equations?"))
			{
				_outputWriter.WriteLine("Deletion cancelled.");
				return;
			}
            _equationManager.ClearEquations();
            _outputWriter.WriteLine("All equations deleted.");
		}
        else
        {
            AssertValidIdentifier(args[0]);
            _equationManager.DeleteEquation(args[0]);
			_outputWriter.WriteLine($"Equation '{args[0]}' deleted.");
		}
    }

	/// <summary>
	/// Evaluates an equation from the equation manager and prints the result to the outputWriter.
	/// Accepts either a single identifier to evaluate, or the keyword "all" to evaluate all equations.
	/// </summary>
	/// <param name="args">Expects only a single argument.</param>
	/// <returns>An ArgumentException is thrown if the arguments do not match the expected format.</returns>
	/// <exception cref="ArgumentException"></exception>
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

	/// <summary>
	/// Writes the equation string that was saved for the given identifier, to the outputWriter.
	/// Accepts either a single identifier to show, or the keyword "all" to show all equations.
	/// </summary>
	/// <param name="args">Expects only a single argument.</param>
	/// <returns>An ArgumentException is thrown if the arguments do not match the expected format.</returns>
	/// <exception cref="ArgumentException"></exception>
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

	/// <summary>
	/// Writes the probability distribution of the equation with the given identifier, to the outputWriter.
	/// Accepts <b>only</b> a single identifier to show.
	/// <br></br>
	/// Note: The distribution is computed on the first call, and cached for subsequent calls.
	/// The calculation may take a long time and a lot of memory for equations with 1000+ possible outcomes.
	/// The memory used by the distribution won't be cleared until the equation is deleted or the program exits.
	/// </summary>
	/// <param name="args">Expects only a single argument.</param>
	/// <returns>An ArgumentException is thrown if the arguments do not match the expected format.</returns>
	/// <exception cref="ArgumentException"></exception>
	public void ShowEquationDistribution(string[] args)
    {
        AssertArgCount(args, 1);
        AssertValidIdentifier(args[0]);
        var equation = _equationManager.GetEquation(args[0]);
        _outputWriter.Write(equation.GetDistribution().ToFormattedString(indentLevel: 0));
    }

	/// <summary>
	/// Exports all saved equations to a file with the given filename.
	/// Equations are saved in the format "identifier=expression", one per line.
	/// If the file already exists, its contents will be overwritten, otherwise it will be created.
	/// If an error occurs while creating or opening the file, an error message will be printed and the export will be cancelled.
	/// </summary>
	/// <param name="args">Expects only a single argument, the filepath/filename to export to.</param>
	/// <returns>An ArgumentException is thrown if the arguments do not match the expected format.</returns>
	/// <exception cref="ArgumentException"></exception>
	public void ExportEquations(string[] args)
    {
        AssertArgCount(args, 1);
        if (File.Exists(args[0]))
        {
            if (!PromptConfirmation($"File {args[0]} already exists. Overwrite?"))
            {
                _outputWriter.WriteLine("Export cancelled.");
                return;
            }
            _outputWriter.WriteLine($"Overwriting Contents...");
        }
        else
        {
            try
            {
                File.Create(args[0]).Close();
            } catch (Exception ex)
			{
				_outputWriter.WriteLine("Error creating file: " + ex.Message);
				return;
			}
        }

        StreamWriter fileHandler;
        try
        {
            fileHandler = new StreamWriter(args[0]);
        }
        catch (Exception ex)
        {
            _outputWriter.WriteLine("Error opening file: " + ex.Message);
            return;
        }

        _outputWriter.Write($"Exported ");
		foreach (var kvp in _equationManager.EnumerateEquations())
        {
            var eq = kvp.Value;
            fileHandler.WriteLine(eq.Name + "=" + eq.ToString());
            _outputWriter.Write($"'{eq.Name}', ");
		}
        fileHandler.Close();
        _outputWriter.WriteLine("to " + args[0]);
	}

	/// <summary>
	/// Imports equations from a file with the given filename.
	/// Empty lines and lines starting with '#' are ignored.
	/// All equations are expected to be in the format "identifier=expression", one per line.
	/// If an identifier already exists, the equation will be skipped and a warning will be printed.
	/// If an error occurs while opening the file, an error message will be printed and the import will be cancelled.
	/// </summary>
	/// <param name="args">Expects only a single argument, the filepath/filename to import from.</param>
	/// <returns>An ArgumentException is thrown if the arguments do not match the expected format, or if the file does not exist.</returns>
	/// <exception cref="ArgumentException"></exception>
	public void ImportEquations(string[] args)
    {
        AssertArgCount(args, 1);
        if (!File.Exists(args[0])) throw new ArgumentException("File does not exist: " + args[0]);
        StreamReader fileHandler;
        try
        {
            fileHandler = new StreamReader(args[0]);
        }
        catch (Exception ex)
		{
			_outputWriter.WriteLine("Error opening file: " + ex.Message);
			return;
		}
		string? line;
        while ((line = fileHandler.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.Length == 0 || line[0] == '#') continue;
            try
            {
                var name = line.Split('=')[0].Trim();
                if (_equationManager.HasEquation(name))
                {
                    _outputWriter.WriteLine($"Warning: '{name}' already exists. Skipping");
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