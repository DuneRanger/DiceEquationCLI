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

	public class EquationObjectTests
	{
		#region Constructor Tests

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

		#endregion Constructor Tests
		#region Name Property Tests

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

		#endregion Name Property Tests
		#region EquationTree Property Tests

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

		#endregion EquationTree Property Tests
		#region Distribution Property Tests

		[Fact]
		public void Equation_SetDistribution()
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
		public void Equation_GetDistribution_NoDistributionSet_ValidEquation()
		{
			var equation = new Equation("1 + 2");
			var add = new AdditionNode();
			add.AddChildren(new IntegerNode(1), new IntegerNode(2));

			equation.SetEquationTree(add);
			var result = equation.GetDistribution();
			var expected = ProbabilisticNumber<double>.FromUniform(new double[] { 3.0 });

			Assert.Equal(expected.ToDictionary(), result.ToDictionary());
		}

		[Fact]
		public void Equation_GetDistribution_ThrowWhenNoEquation()
		{
			var equation = new Equation("1 + 2");
			Assert.Throws<InvalidOperationException>(() => equation.GetDistribution());
		}

		#endregion Distribution Property Tests
		#region Evaluate Tests


		[Fact]
		public void Equation_Evaluate_ThrowWhenNoEquation()
		{
			var equation = new Equation("1 + 2");
			Assert.Throws<InvalidOperationException>(() => equation.Evaluate());
		}

		[Fact]
		public void Equation_Evaluate_ValidEquation()
		{
			var equation = new Equation("1 + 2");
			var add = new AdditionNode();
			add.AddChildren(new IntegerNode(1), new IntegerNode(2));

			equation.SetEquationTree(add);
			var result = equation.Evaluate();

			Assert.Equal(3.0, result);
		}

		[Fact]
		public void Equation_Evaluate_ValidDistributionAndEquation()
		{
			var equation = new Equation("1 + 2");

			var add = new AdditionNode();
			add.AddChildren(new IntegerNode(1), new IntegerNode(2));

			var distribution = new TestDistribution(123.0);

			equation.SetEquationTree(add);
			equation.SetDistribution(distribution);
			var result = equation.Evaluate();

			Assert.Equal(123.0, result);
		}

		[Fact]
		public void Equation_Evaluate_OnlyValidDistribution()
		{
			var equation = new Equation("1 + 2");
			var distribution = new TestDistribution(123.0);

			equation.SetDistribution(distribution);
			var result = equation.Evaluate();

			Assert.Equal(123.0, result);
		}

		#endregion Evaluate Tests
	}
}
