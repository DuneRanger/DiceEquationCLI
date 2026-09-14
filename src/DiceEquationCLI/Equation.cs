using ProbabilisticNumberLibrary;

namespace DiceEquationCLI;

public class Equation
{
    public string Name { get; private set; } = "";
    private string _equation = "";
    private Node? _equationTree;
    private IDiscreteDistribution<double>? _distribution;
    /// <summary>
    /// If true, when calling Evaluate(), the distribution will be sampled, instead of evaluating the equation tree.
    /// </summary>
    private bool _evaluateDistribution = false;

	/// <summary>
	/// Returns the number of nodes in the equation tree.
	/// Can be used to roughly estimate the cost of calculating the equation tree.
	/// Throws an InvalidOperationException if the node type is unknown (i.e. not a ValueNode, UnaryNode, or BinaryNode).
	/// </summary>
	/// <param name="node">The root of the equation tree</param>
	/// <returns>The number of nodes in the equation tree</returns>
	/// <exception cref="InvalidOperationException"></exception>
	private int GetEquationTreeNodeCount(Node node) => node switch
    {
        null => 0,
        ValueNode _ => 1,
		UnaryNode unaryNode => 1 + GetEquationTreeNodeCount(unaryNode.Child!),
		BinaryNode binaryNode => 1 + GetEquationTreeNodeCount(binaryNode.Left!) + GetEquationTreeNodeCount(binaryNode.Right!),
		_ => throw new InvalidOperationException("Unknown node type")
	};

	public Equation(string equation)
    {
        _equation = equation;
    }
    
    /// <summary>
    /// Overwrites the name of the equation.
    /// There is no validation on the name.
    /// </summary>
    /// <param name="name">The new name for the equation</param>
    public void SetName(string name) => Name = name;
	/// <summary>
	/// Overwrites the equation tree of the equation.
    /// Warning: This does not change the string of the equation or the cached distribution.
	/// </summary>
	/// <param name="equationTreeRoot">The root of the new equation tree</param>
	public void SetEquationTree(Node equationTreeRoot) => _equationTree = equationTreeRoot;
	/// <summary>
	/// Overwrites the cached distribution of the equation.
	/// Note: This does not change the string of the equation or the equation tree.
	/// Even if the distribution is set, the equation tree will still be evaluated unless the corresponding flag is set to true.
	/// </summary>
	/// <param name="distribution">The new distribution for the equation</param>
	public void SetDistribution(IDiscreteDistribution<double> distribution) => _distribution = distribution;

	/// <summary>
	/// Evaluates the equation and returns a single sample from the distribution.
	/// If the distribution is set and the flag to evaluate the distribution is set, it will sample from the distribution.
	/// Otherwise, it will evaluate the equation tree.
	/// Otherwise, it will throw an InvalidOperationException.
	/// </summary>
	/// <returns>The result of the evaluated equation</returns>
	/// <exception cref="InvalidOperationException"></exception>
	public double Evaluate() {
        if (_distribution is not null && !_evaluateDistribution) return _distribution.Sample();
        if (_equationTree is not null) return _equationTree.Evaluate();
        throw new InvalidOperationException("No distribution available");
    }
	/// <summary>
	/// If the distribution is set, returns the distribution.
	/// Otherwise it will generate the distribution from the equation tree and return it.
	/// If the distribution has less values than the number of nodes in the equation tree, it will set the flag to evaluate the distribution instead of the equation tree.
	/// If neither the distribution nor the equation tree is set, it will throw an InvalidOperationException.
	/// </summary>
	/// <returns>The distribution of the equation (ProbabilisticNumber as IDiscreteDistribution)</returns>
	/// <exception cref="InvalidOperationException"></exception>
	public IDiscreteDistribution<double> GetDistribution() {
        if (_distribution is not null) return _distribution;
        if (_equationTree is not null)
        {
            var distribution = _equationTree.GetDistribution();
            _distribution = distribution;
            if (GetEquationTreeNodeCount(_equationTree) > distribution.Count / 2) _evaluateDistribution = true;
			return distribution;
        }
        throw new InvalidOperationException("No distribution available");
    }
    public override string ToString() => _equation;
}