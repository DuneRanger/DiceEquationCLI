namespace DiceEquationCLI;

public class EquationManager
{
    private Dictionary<string, Equation> _equations = new();
    private EquationTreeParser parser = new();

	/// <summary>
	/// Adds a new equation to the manager with the specified name and equation string.
	/// If an equation with the same name already exists, an ArgumentException is thrown.
	/// If an error occurs while parsing the equation or constructing the object, the error will be propagated to the caller.
	/// </summary>
	/// <param name="name">The name of the equation, expected to be a valid identifier</param>
	/// <param name="equation">The string representation of the equation</param>
	/// <exception cref="ArgumentException"></exception>
	public void AddEquation(string name, string equation)
    {
        if (HasEquation(name)) throw new ArgumentException($"Equation '{name}' name already exists.");
		var equationObj = new Equation(equation);
        var equationTreeRoot = parser.ConstructTree(equation);

        equationObj.SetEquationTree(equationTreeRoot);
        equationObj.SetName(name);
        _equations.Add(name, equationObj);
    }

	/// <summary>
	/// Checks if an equation with the specified name exists in the manager.
	/// </summary>
	/// <param name="name">The name of the equation to check</param>
	/// <returns>true if the equation exists, false otherwise</returns>
    public bool HasEquation(string name)
    {
        return _equations.ContainsKey(name);
    }

	/// <summary>
	/// Returns the equation object associated with the specified name.
    /// If the equation does not exist, an ArgumentException is thrown.
	/// </summary>
	/// <param name="name">The name of the equation to retrieve</param>
	/// <returns>The equation object</returns>
	/// <exception cref="ArgumentException"></exception>
	public Equation GetEquation(string name)
    {
        _equations.TryGetValue(name, out Equation? equationObj);
        if (equationObj == null) throw new ArgumentException($"Equation '{name}' does not exist.");
        return equationObj;
    }

	/// <summary>
	/// Deletes the equation with the specified name from the manager.
	/// If the equation does not exist, an ArgumentException is thrown.
	/// </summary>
	/// <param name="name">The name of the equation to delete</param>
	/// <exception cref="ArgumentException"></exception>
	public void DeleteEquation(string name)
    {
        if (!_equations.ContainsKey(name)) throw new ArgumentException($"Equation '{name}' does not exist.");
        _equations.Remove(name);
    }

	/// <summary>
	/// Returns the dictionary of all equations managed by this EquationManager as an enumerable of key-value pairs,
	/// where the key is the equation name and the value is the corresponding Equation object.
	/// </summary>
	/// <returns>An enumerable of key-value pairs representing the equations</returns>
	public IEnumerable<KeyValuePair<string, Equation>> EnumerateEquations()
    {
        return _equations.AsEnumerable();
    }

	/// <summary>
	/// Deletes all equations saved in the manager, emptying the internal dictionary.
	/// </summary>
	public void ClearEquations()
    {
        _equations.Clear();
    }
}