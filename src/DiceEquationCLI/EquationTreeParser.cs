using System.ComponentModel;
using System.Runtime.Intrinsics.X86;
using static System.Runtime.InteropServices.JavaScript.JSType;

using System.Runtime.CompilerServices;

// So that the tests can access internal methods, for more divisive testing of ConstructTree
[assembly: InternalsVisibleTo("DiceEquationCLITests")]

namespace DiceEquationCLI;

/// <summary>
/// Contains the symbol, priority, and a function to create an instance of the operator node for a given operator.
/// A higher priority value indicates that the operator should be evaluated before operators with lower priority values.
/// </summary>
public class EquationOperator
{
    public char Symbol { get; private set; }
    public int Priority { get; private set; }
    public Func<IOperatorNode> CreateNodeInstance { get; private set; }
    
    public EquationOperator(char symbol, int priority, Func<IOperatorNode> createNodeInstance)
    {
        Symbol = symbol;
        Priority = priority;
        CreateNodeInstance = createNodeInstance;
    }
}

public class EquationTreeParser
{
    private Dictionary<char, EquationOperator> _binaryOperator = new();
    private Dictionary<char, EquationOperator> _unaryOperator = new();
	/// <summary>
	/// Returns an array of all operator symbols (both binary and unary) that are currently registered in the parser.
	/// Every symbol in the array is unique, and the order of symbols is not guaranteed.
	/// </summary>
	public char[] OperatorSymbols => _binaryOperator.Keys.ToList().Concat(_unaryOperator.Keys.ToList()).Distinct().ToArray();

	/// <summary>
	/// Adds a new binary operator to the parser with the specified symbol, priority, and a function to create an instance of the corresponding operator node.
	/// If the symbol already exists, an ArgumentException is thrown.
	/// </summary>
	/// <param name="symbol">The symbol representing the operator</param>
	/// <param name="priority">The priority of the operator (higher values are evaluated first)</param>
	/// <param name="createOperatorNodeInstance">A function to create an instance of the corresponding operator node</param>
	/// <exception cref="ArgumentException"></exception>
	public void AddBinaryOperand(char symbol, int priority, Func<IOperatorNode> createOperatorNodeInstance)
    {
        var operation = new EquationOperator(symbol, priority, createOperatorNodeInstance);
        if (!_binaryOperator.TryAdd(symbol, operation)) throw new ArgumentException("Symbol already exists");
    }

	/// <summary>
	/// Adds a new unary operator to the parser with the specified symbol, priority, and a function to create an instance of the corresponding operator node.
	/// If the symbol already exists, an ArgumentException is thrown.
	/// </summary>
	/// <param name="symbol">The symbol representing the operator</param>
	/// <param name="priority">The priority of the operator (higher values are evaluated first)</param>
	/// <param name="createOperatorNodeInstance">A function to create an instance of the corresponding operator node</param>
	/// <exception cref="ArgumentException"></exception>
	public void AddUnaryOperand(char symbol, int priority, Func<IOperatorNode> createOperatorNodeInstance)
	{
		var operation = new EquationOperator(symbol, priority, createOperatorNodeInstance);
		if (!_unaryOperator.TryAdd(symbol, operation)) throw new ArgumentException("Symbol already exists");
	}

	/// <summary>
	/// Adds the base set of operands (addition, subtraction, multiplication, division, and negation (unary minus)) to the parser.
	/// </summary>
	private void AddBaseOperands()
    {
        AddBinaryOperand('+', 3, () => new AdditionNode());
        AddBinaryOperand('-', 3, () => new SubtractionNode());
		AddBinaryOperand('*', 5, () => new MultiplicationNode());
        AddBinaryOperand('/', 5, () => new DivisionNode());
        AddUnaryOperand('-', 10, () => new NegationNode());
    }

	/// <summary>
	/// Constructs a new instance of the EquationTreeParser class and initializes it with the base operands and the dice simulation operator ('d').
	/// </summary>
	public EquationTreeParser()
    {
        AddBaseOperands();
        AddBinaryOperand('d', 10, () => new DiceSimulationNode());
    }

	/// <summary>
	/// Converts an equation string into an array of string tokens.
	/// A token is defined as a sequence of characters that represents a number, or a symbol representing an operator or a parenthesis.
	/// <br></br>
	/// All whitespace characters are ignored.
	/// Only ordinary parentheses are supported, though their pairing isn't checked.
	/// </summary>
	/// <param name="equation">The equation string to convert into tokens</param>
	/// <returns>An array of string tokens</returns>
	internal string[] EquationToTokens(string equation)
    {
        List<string> tokens = new();
        var operators = OperatorSymbols.ToList();
        operators.Add('(');
        operators.Add(')');

        string token = "";
        for (int i = 0; i < equation.Length; i++)
        {
            if (char.IsWhiteSpace(equation[i]))
            {
				if (token != "")
				{
					tokens.Add(token);
					token = "";
				}
                while (i < equation.Length && char.IsWhiteSpace(equation[i])) i++;
				if (i >= equation.Length) break;
            }
            if (operators.Contains(equation[i]))
            {
				if (token != "")
				{
					tokens.Add(token);
					token = "";
				}
				tokens.Add(equation[i].ToString());
			}
			else
			{
				token += equation[i];
			}
		}
		if (token != "") tokens.Add(token);

        return tokens.ToArray();
    }

	/// <summary>
	/// Converts an array of string tokens in infix notation to postfix notation.
	/// If the input tokens contain invalid characters or mismatched parentheses, an ArgumentException is thrown.
	/// The validity and parity of the operators and operands is not checked.
	/// <br></br>
	/// Unary operators will be prefixed with 'u' in the output tokens to distinguish them from binary operators.
	/// </summary>
	/// <param name="tokens">An array of string tokens in infix notation</param>
	/// <returns>An array of string tokens in postfix notation</returns>
	/// <exception cref="ArgumentException"></exception>
	internal string[] InfixToPostfix(string[] tokens)
	{
		Stack<string> operatorStack = new Stack<string>();
		List<string> postfix = new List<string>();

		bool expectUnary = true;

		foreach (var token in tokens)
		{
			if (string.IsNullOrWhiteSpace(token)) continue;

			// Matches both doubles and ints
			if (double.TryParse(token, out _))
			{
				postfix.Add(token);
				expectUnary = false;
			}
			else if (token == "(")
			{
				operatorStack.Push(token);
				expectUnary = true;
			}
			else if (token == ")")
			{
				while (operatorStack.Count > 0 && operatorStack.Peek() != "(")
				{
					postfix.Add(operatorStack.Pop());
				}
				if (operatorStack.Count == 0 || operatorStack.Pop() != "(")
					throw new ArgumentException("Mismatched parentheses: missing opening parenthesis.");
				expectUnary = false;
			}
			else if (token.Length == 1 && OperatorSymbols.Contains(token[0]))
			{
				char op = token[0];
				bool isUnary = expectUnary && _unaryOperator.ContainsKey(op);

				if (!isUnary && !_binaryOperator.ContainsKey(op))
				{
					throw new ArgumentException($"Invalid or unsupported operator: {op}");
				}

				EquationOperator eqOp = isUnary ? _unaryOperator[op] : _binaryOperator[op];

				// Add a prefix, so that it's easier to tell unary and binary operators apart
				// Instead of having to analyze the context
				string opToken = isUnary ? "u" + op : op.ToString();

				while (operatorStack.Count > 0 && operatorStack.Peek() != "(")
				{
					string top = operatorStack.Peek();
					bool topIsUnary = top.StartsWith("u");
					char topOp = topIsUnary ? top[1] : top[0];
					EquationOperator topEqOp = topIsUnary ? _unaryOperator[topOp] : _binaryOperator[topOp];

					if (topEqOp.Priority > eqOp.Priority || (topEqOp.Priority == eqOp.Priority && !isUnary))
					{
						postfix.Add(operatorStack.Pop());
					}
					else
					{
						break;
					}
				}
				operatorStack.Push(opToken);
				expectUnary = true;
			}
			else
			{
				throw new ArgumentException($"Invalid character or token in equation: {token}");
			}
		}

		while (operatorStack.Count > 0)
		{
			string op = operatorStack.Pop();
			if (op == "(" || op == ")")
				throw new ArgumentException("Mismatched parentheses: missing closing parenthesis.");
			postfix.Add(op);
		}

		return postfix.ToArray();
	}

	/// <summary>
	/// Constructs an equation tree from a given equation string.
	/// If there are any invalid characters, mismatched parentheses, or missing operands for operators, an ArgumentException is thrown.
	/// If there is a problem with the construction of the operator nodes for a given operator, an InvalidOperationException is thrown.
	/// </summary>
	/// <param name="equation">The equation string to parse. Whitespace is ignored.</param>
	/// <returns>The root node of the constructed equation tree.</returns>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
	public Node ConstructTree(string equation)
	{
		string[] tokens = EquationToTokens(equation);
		if (tokens.Length == 0) throw new ArgumentException("Equation is empty or contains only whitespace.");
		string[] postfix = InfixToPostfix(tokens);

		Stack<Node> nodeStack = new Stack<Node>();

		foreach (var token in postfix)
		{
			if (double.TryParse(token, out double val))
			{
				nodeStack.Push(new DoubleNode(val));
			}
			else
			{
				bool isUnary = token.StartsWith("u");
				char op = isUnary ? token[1] : token[0];

				EquationOperator eqOp = isUnary ? _unaryOperator[op] : _binaryOperator[op];
				IOperatorNode opNode = eqOp.CreateNodeInstance();

				if (opNode == null) throw new InvalidOperationException($"Failed to create operator node for operator '{op}'");
				Node actualOpNode = (Node)opNode;

				if (isUnary)
				{
					if (nodeStack.Count < 1) throw new ArgumentException($"Missing child node for unary operator '{op}'");
					Node child = nodeStack.Pop();

					UnaryOperatorNode unaryNode = actualOpNode as UnaryOperatorNode;
					unaryNode.AddChild(child);
				}
				else
				{
					if (nodeStack.Count < 2) throw new ArgumentException($"Missing child nodes for binary operator '{op}'");
					Node right = nodeStack.Pop();
					Node left = nodeStack.Pop();

					BinaryOperatorNode operatorNode = actualOpNode as BinaryOperatorNode;
					operatorNode.AddChildren(left, right);
				}

				nodeStack.Push(actualOpNode);
			}
		}

		if (nodeStack.Count != 1) throw new ArgumentException("Invalid equation format: Too many operands or missing operators.");

		return nodeStack.Pop();
	}
}
