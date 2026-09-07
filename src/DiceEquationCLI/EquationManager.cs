namespace DiceEquationCLI;

public class EquationManager
{
    private Dictionary<string, Equation> _equations = new();
    private EquationTreeParser parser = new();
    
    public void AddEquation(string name, string equation)
    {
        if (HasEquation(name)) throw new ArgumentException("Equation with this name already exists: " + name);
		var equationObj = new Equation(equation);
        var equationTreeRoot = parser.ConstructTree(equation);

        equationObj.SetEquationTree(equationTreeRoot);
        equationObj.SetName(name);
        _equations.Add(name, equationObj);
    }

    public bool HasEquation(string name)
    {
        return _equations.ContainsKey(name);
    }
    
    public Equation GetEquation(string name)
    {
        _equations.TryGetValue(name, out Equation? equationObj);
        if (equationObj == null) throw new ArgumentException("Equation \"" + name + "\" does not exist.");
        return equationObj;
    }
    
    public void DeleteEquation(string name)
    {
        if (!_equations.ContainsKey(name)) throw new ArgumentException("Equation \"" + name + "\" does not exist.");
        _equations.Remove(name);
    }

    public IEnumerable<KeyValuePair<string, Equation>> EnumerateEquations()
    {
        return _equations.AsEnumerable();
    }

    public void ClearEquations()
    {
        _equations.Clear();
    }
}