using DiceEquationCLI;
using ProbabilisticNumberLibrary;

namespace DiceEquationCLITests
{
	public class TestDistribution : IDiscreteDistribution<double>
	{
		private double _value;

		public TestDistribution(double value)
		{
			_value = value;
		}

		public double Sample()
		{
			return _value;
		}

		public Dictionary<double, double> ToDictionary()
		{
			return new Dictionary<double, double> { { _value, 1.0 } };
		}

		public string ToFormattedString(int indentLevel = 0)
		{
			return _value.ToString();
		}
	}

	public class TestIncrementingDistribution : IDiscreteDistribution<double>
	{
		private int _callCount = 0;

		public double Sample()
		{
			return ++_callCount;
		}

		public Dictionary<double, double> ToDictionary()
		{
			return new Dictionary<double, double>();
		}

		public string ToFormattedString(int indentLevel = 0)
		{
			return "TestIncrementingDistribution";
		}
	}

	public class EquationObjectTests
	{
		// ========== Constructor Tests ==========

		[Fact]
		public void Equation_Constructor_SetsEquationString()
		{
			var equation = new Equation("1 + 2");
			Assert.Equal("1 + 2", equation.ToString());
		}

		[Fact]
		public void Equation_Constructor_EmptyName()
		{
			var equation = new Equation("1 + 2");
			Assert.Equal("", equation.Name);
		}

		// ========== Name Property Tests ==========

		[Fact]
		public void Equation_SetName_UpdatesName()
		{
			var equation = new Equation("1 + 2");
			equation.SetName("myEquation");
			Assert.Equal("myEquation", equation.Name);
		}

		[Fact]
		public void Equation_SetName_CanUpdateMultipleTimes()
		{
			var equation = new Equation("1 + 2");
			equation.SetName("first");
			Assert.Equal("first", equation.Name);
			equation.SetName("second");
			Assert.Equal("second", equation.Name);
		}

		// ========== EquationTree Property Tests ==========

		[Fact]
		public void Equation_SetEquationTree_StoresNode()
		{
			var equation = new Equation("1 + 2");
			var node = new IntegerNode(42);
			equation.SetEquationTree(node);
		}

		[Fact]
		public void Equation_SetEquationTree_MultipleNodes()
		{
			var equation = new Equation("1 + 2");
			var node1 = new IntegerNode(1);
			var node2 = new IntegerNode(2);
			equation.SetEquationTree(node1);
			equation.SetEquationTree(node2);
		}

		// ========== Distribution Property Tests ==========

		[Fact]
		public void Equation_SetDistribution_StoresDistribution()
		{
			var equation = new Equation("1 + 2");
			var distribution = new TestDistribution(42.0);
			equation.SetDistribution(distribution);
		}

		[Fact]
		public void Equation_GetDistribution_ReturnsSetDistribution()
		{
			var equation = new Equation("1 + 2");
			var distribution = new TestDistribution(42.0);

			equation.SetDistribution(distribution);
			var result = equation.GetDistribution();

			Assert.Same(distribution, result);
		}

		[Fact]
		public void Equation_GetDistribution_ThrowsWhenNotSet()
		{
			var equation = new Equation("1 + 2");
			Assert.Throws<InvalidOperationException>(() => equation.GetDistribution());
		}

		// ========== Evaluate Tests ==========

		[Fact]
		public void Equation_Evaluate_CallsSampleOnDistribution()
		{
			var equation = new Equation("1 + 2");
			var distribution = new TestDistribution(123.0);

			equation.SetDistribution(distribution);
			var result = equation.Evaluate();

			Assert.Equal(123.0, result);
		}

		[Fact]
		public void Equation_Evaluate_ThrowsWhenDistributionNotSet()
		{
			var equation = new Equation("1 + 2");
			Assert.Throws<InvalidOperationException>(() => equation.Evaluate());
		}

		[Fact]
		public void Equation_Evaluate_MultipleCallsReturnSameValue()
		{
			var equation = new Equation("1 + 2");
			var distribution = new TestIncrementingDistribution();

			equation.SetDistribution(distribution);
			var result1 = equation.Evaluate();
			var result2 = equation.Evaluate();

			Assert.Equal(1.0, result1);
			Assert.Equal(2.0, result2);
		}

		[Fact]

		public void Equation_EvaluateEquationTree()
		{
			// Should test that the equation object is capable of evaluating its equation tree, if it doesn't have a distribution
			throw new NotImplementedException();
		}
	}
}
