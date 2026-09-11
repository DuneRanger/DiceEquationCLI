using ProbabilisticNumberLibrary;

namespace DiceEquationCLI;

public class Equation
{
    public string Name { get; private set; } = "";
    private string _equation = "";
    private Node? _equationTree;
    private IDiscreteDistribution<double>? _distribution;
    private bool _evaluateDistribution = false;

	private int GetEquationTreeNodeCount(Node node) => node switch
    {
        null => 0,
        ValueNode _ => 1,
		UnaryNode unaryNode => 1 + GetEquationTreeNodeCount(unaryNode.Child),
		BinaryNode binaryNode => 1 + GetEquationTreeNodeCount(binaryNode.Left) + GetEquationTreeNodeCount(binaryNode.Right),
		_ => throw new InvalidOperationException("Unknown node type")
	};

	public Equation(string equation)
    {
        _equation = equation;
    }
    
    public void SetName(string name) => Name = name;
    public void SetEquationTree(Node equationTreeRoot) => _equationTree = equationTreeRoot;
    public void SetDistribution(IDiscreteDistribution<double> distribution) => _distribution = distribution;
    
    public double Evaluate() {
        if (_distribution is not null && !_evaluateDistribution) return _distribution.Sample();
        if (_equationTree is not null) return _equationTree.Evaluate();
        throw new InvalidOperationException("No distribution available");
    }
    public IDiscreteDistribution<double> GetDistribution() {
        if (_distribution is not null) return _distribution;
        if (_equationTree is not null)
        {
            var distribution = _equationTree.GetDistribution();
            _distribution = distribution;
            if (GetEquationTreeNodeCount(_equationTree) < 1000) _evaluateDistribution = true;
			return distribution;
        }
        throw new InvalidOperationException("No distribution available");
    }
    public override string ToString() => _equation;
}