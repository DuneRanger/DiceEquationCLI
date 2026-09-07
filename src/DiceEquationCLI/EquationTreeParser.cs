using System.ComponentModel;
using System.Runtime.Intrinsics.X86;
using static System.Runtime.InteropServices.JavaScript.JSType;

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DiceEquationCLITests")]

namespace DiceEquationCLI;

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
	public char[] OperatorSymbols => _binaryOperator.Keys.ToList().Concat(_unaryOperator.Keys.ToList()).ToArray();
    
    public void AddBinaryOperand(char symbol, int priority, Func<IOperatorNode> createOperatorNodeInstance)
    {
        var operation = new EquationOperator(symbol, priority, createOperatorNodeInstance);
        if (!_binaryOperator.TryAdd(symbol, operation)) throw new ArgumentException("Symbol already exists");
    }

	public void AddUnaryOperand(char symbol, int priority, Func<IOperatorNode> createOperatorNodeInstance)
	{
		var operation = new EquationOperator(symbol, priority, createOperatorNodeInstance);
		if (!_unaryOperator.TryAdd(symbol, operation)) throw new ArgumentException("Symbol already exists");
	}

	private void AddBaseOperands()
    {
        AddBinaryOperand('+', 3, () => new AdditionNode());
        AddBinaryOperand('-', 3, () => new SubtractionNode());
		AddBinaryOperand('*', 5, () => new MultiplicationNode());
        AddBinaryOperand('/', 5, () => new DivisionNode());
        AddUnaryOperand('-', 10, () => new NegationNode());
    }

    public EquationTreeParser()
    {
        AddBaseOperands();
        AddBinaryOperand('d', 10, () => new DiceSimulationNode());
    }
    
    private static string PrettyFormatParsingError(string equation, int underlineStart, int underlineEnd)
    {
        var baseText = $"Error parsing equation: {equation}";
        var preEquationLength = baseText.Length - equation.Length;
        var underline = new string(' ', preEquationLength + underlineStart);
        underline += new string('^', underlineEnd - underlineStart);
        return $"{baseText}\n{underline}";
    }

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
