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
    private Dictionary<char, EquationOperator> _operatorClass = new();
    public char[] OperatorSymbols => _operatorClass.Keys.ToArray();
    
    public void AddOperand(char symbol, int priority, Func<IOperatorNode> createOperatorNodeInstance)
    {
        var operation = new EquationOperator(symbol, priority, createOperatorNodeInstance);
        if (!_operatorClass.TryAdd(symbol, operation)) throw new ArgumentException("Symbol already exists");
    }
    
    private void AddBaseOperands()
    {
        AddOperand('+', 3, () => new AdditionNode());
        AddOperand('-', 3, () => new SubtractionNode());
        AddOperand('*', 5, () => new MultiplicationNode());
        AddOperand('/', 5, () => new DivisionNode());
    }

    public EquationTreeParser()
    {
        AddBaseOperands();
        AddOperand('d', 10, () => new DiceConstructionNode());
    }
    
    private static string PrettyFormatParsingError(string equation, int underlineStart, int underlineEnd)
    {
        var baseText = $"Error parsing equation: {equation}";
        var preEquationLength = baseText.Length - equation.Length;
        var underline = new string(' ', preEquationLength + underlineStart);
        underline += new string('^', underlineEnd - underlineStart);
        return $"{baseText}\n{underline}";
    }

    private string[] EquationToTokens(string equation)
    {
        List<string> tokens = new();
        var operators = OperatorSymbols;

        int lastIndex = 0;
        string token = "";
        for (int i = 0; i < equation.Length; i++)
        {
            if (char.IsWhiteSpace(equation[i]))
            {
                if (i > lastIndex) token += equation.Substring(lastIndex, i - lastIndex);
                while (char.IsWhiteSpace(equation[i])) i++;
                lastIndex = i + 1;
            }

            if (operators.Contains(equation[i]))
            {
                tokens.Add(token + equation.Substring(lastIndex, i - lastIndex));
                tokens.Add(equation[i].ToString());
            }
        }
        
        return tokens.ToArray();
    }

    private string InfixToPostfix(string infix)
    {
        var tokens = EquationToTokens(infix);
        var operators = OperatorSymbols;

        List<string> nextTokens = new();
        string leftOperand = "";
        while (true)
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                if (operators.Contains(tokens[i][^1]) || tokens[i] == "(" || tokens[i] == ")")
                {
                    if (leftOperand != "") nextTokens.Add(leftOperand + " " + tokens[i]);
                    else nextTokens.Add(tokens[i]);
                    continue;
                }
                else
                {
                    if (i == 0) leftOperand = tokens[i];
                    else if (i == tokens.Length - 1) nextTokens[^1] = nextTokens[^1].Insert(nextTokens[^1].Length - 2, tokens[i] + " ");
                    else
                    {
                        var leftPriority = _operatorClass.TryGetValue(tokens[i - 1][^1], out EquationOperator? opLeft) ? opLeft.Priority : 0;
                        var rightPriority = _operatorClass.TryGetValue(tokens[i + 1][^1], out EquationOperator? opRight) ? opRight.Priority : 0;
                        if (leftPriority >rightPriority) leftOperand = tokens[i];
                        else nextTokens[^1] = nextTokens[^1].Insert(nextTokens[^1].Length - 2, tokens[i] + " ");
                    }
                }
            }
            tokens = nextTokens.ToArray();
            nextTokens.Clear();
            if (tokens.Length == 1) break;
        }
        
        return tokens[0];
    }
    
    public Node ConstructTree(string equation)
    {
        var postFixEq = InfixToPostfix(equation);
        
    } 
}