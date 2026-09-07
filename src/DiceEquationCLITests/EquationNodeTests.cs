using DiceEquationCLI;

namespace DiceEquationCLITests
{
	public class TestUnaryNode : UnaryNode
	{
		public override double Evaluate()
		{
			throw new NotImplementedException();
		}
	}

	public class TestBinaryNode : BinaryNode
	{
		public override double Evaluate()
		{
			throw new NotImplementedException();
		}
	}

	public class EquationNodeTests
	{
		// ========== Literal Node Tests ==========

		[Fact]
		public void IntegerLiteralNode_ReturnsCorrectValue()
		{
			var node = new IntegerNode(42);

			Assert.Equal(42, node.Evaluate());
		}

		[Fact]
		public void DoubleLiteralNode_ReturnsCorrectValue()
		{
			var node = new DoubleNode(42.0);

			Assert.Equal(42.0, node.Evaluate());
		}

		// ========== Operator Node Tests ==========

		[Fact]
		public void UnaryNode_AddChild()
		{
			var node = new TestUnaryNode();
			node.AddChild(new IntegerNode(1));
			Assert.Throws<InvalidOperationException>(() => node.AddChild(new IntegerNode(2)));
		}

		[Fact]
		public void BinaryNode_AddChild()
		{
			var node = new TestBinaryNode();
			node.AddLeftChild(new IntegerNode(1));
			node.AddRightChild(new IntegerNode(2));
			Assert.Throws<InvalidOperationException>(() => node.AddLeftChild(new IntegerNode(3)));
			Assert.Throws<InvalidOperationException>(() => node.AddRightChild(new IntegerNode(3)));
		}

		[Fact]
		public void BinaryNode_AddChildren()
		{
			var node = new TestBinaryNode();
			node.AddChildren(new IntegerNode(1), new IntegerNode(2));
			Assert.Throws<InvalidOperationException>(() => node.AddLeftChild(new IntegerNode(3)));
			Assert.Throws<InvalidOperationException>(() => node.AddRightChild(new IntegerNode(3)));
			Assert.Throws<InvalidOperationException>(() => node.AddChildren(new IntegerNode(3), new IntegerNode(4)));
		}

		[Fact]
		public void BinaryNode_AddChildren_HasChild()
		{
			var node = new TestBinaryNode();
			node.AddLeftChild(new IntegerNode(1));
			Assert.Throws<InvalidOperationException>(() => node.AddLeftChild(new IntegerNode(3)));
			Assert.Throws<InvalidOperationException>(() => node.AddChildren(new IntegerNode(3), new IntegerNode(4)));
		}

		// ========== Basic Operator Node Evaluation Tests ==========

		[Fact]
		public void AdditionNode_EvaluateNoChildren_ThrowsException()
		{
			var node = new AdditionNode();
			node.AddLeftChild(new IntegerNode(1));
			Assert.Throws<NullReferenceException>(() => node.Evaluate());
		}

		[Fact]
		public void SubstractionNode_EvaluateNoChildren_ThrowsException()
		{
			var node = new SubtractionNode();
			node.AddLeftChild(new IntegerNode(1));
			Assert.Throws<NullReferenceException>(() => node.Evaluate());
		}

		[Fact]
		public void MultiplicationNode_EvaluateNoChildren_ThrowsException()
		{
			var node = new MultiplicationNode();
			node.AddLeftChild(new IntegerNode(1));
			Assert.Throws<NullReferenceException>(() => node.Evaluate());
		}

		[Fact]
		public void DivisionNode_EvaluateNoChildren_ThrowsException()
		{
			var node = new DivisionNode();
			node.AddLeftChild(new IntegerNode(1));
			Assert.Throws<NullReferenceException>(() => node.Evaluate());
		}

		[Fact]
		public void NegationNode_EvaluateNoChildren_ThrowsException()
		{
			var node = new NegationNode();
			Assert.Throws<NullReferenceException>(() => node.Evaluate());
		}

		[Theory]
		[InlineData(1, 1, 2)]
		[InlineData(-1, 1, 0)]
		[InlineData(0, 0, 0)]
		[InlineData(0.5, 0.3, 0.8)]
		[InlineData(0.3, 0.5, 0.8)]
		[InlineData(1e10, 1e12, 1.01e12)]
		public void AdditionNode_Evaluate_ReturnsCorrectValue(double left, double right, double expected)
		{
			var node = new AdditionNode();
			node.AddChildren(new DoubleNode(left), new DoubleNode(right));
			Assert.Equal(expected, node.Evaluate());
		}

		[Theory]
		[InlineData(1, -1, 2)]
		[InlineData(0, 0, 0)]
		[InlineData(0.5, 0.3, 0.2)]
		[InlineData(0.3, 0.5, -0.2)]
		[InlineData(1e10, 1e12, -9.9e11)]
		public void SubtractionNode_Evaluate_ReturnsCorrectValue(double left, double right, double expected)
		{
			var node = new SubtractionNode();
			node.AddChildren(new DoubleNode(left), new DoubleNode(right));
			Assert.Equal(expected, node.Evaluate());
		}


		[Theory]
		[InlineData(1, -1, -1)]
		[InlineData(0, 0, 0)]
		[InlineData(0.5, 0.3, 0.15)]
		[InlineData(0.3, 0.5, 0.15)]
		[InlineData(1e10, 1e12, 1e22)]
		[InlineData(1e10, 1e-12, 0.01)]
		public void MultiplicationNode_Evaluate_ReturnsCorrectValue(double left, double right, double expected)
		{
			var node = new MultiplicationNode();
			node.AddChildren(new DoubleNode(left), new DoubleNode(right));
			Assert.Equal(expected, node.Evaluate());
		}


		[Theory]
		[InlineData(1, -1, -1)]
		[InlineData(0.5, 0.3, 0.5 / 0.3)]
		[InlineData(0.3, 0.5, 0.6)]
		[InlineData(1e10, 1e12, 0.01)]
		[InlineData(1e20, 1e-10, 1e30)]
		public void DivisionNode_Evaluate_ReturnsCorrectValue(double left, double right, double expected)
		{
			var node = new DivisionNode();
			node.AddChildren(new DoubleNode(left), new DoubleNode(right));
			Assert.Equal(expected, node.Evaluate());
		}

		[Fact]
		public void DivisionNode_Evaluate_DivideByZero_ThrowsException()
		{
			var node = new DivisionNode();
			node.AddChildren(new DoubleNode(1), new DoubleNode(0));
			//Assert.Equal(double.PositiveInfinity, node.Evaluate());
			Assert.Throws<DivideByZeroException>(() => node.Evaluate());
		}

		[Theory]
		[InlineData(1, -1)]
		[InlineData(-1, 1)]
		[InlineData(1e12, -1e12)]
		[InlineData(0, 0)]
		public void NegationNode_Evaluate_ReturnsCorrectValue(double value, double expected)
		{
			var node = new NegationNode();
			node.AddChild(new DoubleNode(value));
			Assert.Equal(expected, node.Evaluate());
		}

		// ========== Helper Functions for Custom Operator Node Tests ==========

		public bool DiceSimulationNode_CoversAllValues(DiceSimulationNode node, int min, int max)
		{
			if (max - min > 10000) throw new ArgumentException("Range too large for high-probability testing.");

			bool[] coveredValues = new bool[max - min + 1];
			int coveredCount = 0;
			var simulations = (max - min + 1) * 100;

			for (int i = 0; i < simulations; i++)
			{
				double result = node.Evaluate();
				if (result < min || result > max) throw new ArgumentException("Dice Simulation returned a value outside the expected range.");
				if (!coveredValues[(int)(result - min)]) coveredCount++;
				coveredValues[(int)(result - min)] = true;
			}

			// Allow a 0.1% allowed missed values
			return coveredCount >= (max - min + 1) * 0.999;
		}

		// ========== Custom Operator Node Evaluation Tests ==========

		[Fact]
		public void DiceSimulationNode_EvaluateNoChildren_ThrowsException()
		{
			var node = new DiceSimulationNode();
			node.AddLeftChild(new IntegerNode(1));
			Assert.Throws<NullReferenceException>(() => node.Evaluate());
		}

		[Theory]
		[InlineData(1.5, 6)]
		[InlineData(2, 6.5)]
		[InlineData(4.5, 6.5)]
		[InlineData(-1, 6)]
		public void DiceSimulationNode_InvalidValues_ThrowsException(double left, double right)
		{
			var node = new DiceSimulationNode();
			node.AddChildren(new DoubleNode(left), new DoubleNode(right));
			Assert.Throws<ArgumentException>(() => node.Evaluate());
		}

		[Theory]
		[InlineData(1, 6)]
		[InlineData(2, 6)]
		[InlineData(4, 6)]
		[InlineData(100, 6)]
		public void DiceSimulationNode_Evaluate_ReturnsCorrectValue(int rollCount, int sides)
		{
			var node = new DiceSimulationNode();
			node.AddChildren(new IntegerNode(rollCount), new IntegerNode(sides));
			DiceSimulationNode_CoversAllValues(node, rollCount, rollCount * sides);
		}

		[Theory]
		[InlineData(0, 6)]
		[InlineData(6, 0)]
		public void DiceSimulationNode_ZeroCausesZero(int rollCount, int sides)
		{
			var node = new DiceSimulationNode();
			node.AddChildren(new IntegerNode(rollCount), new IntegerNode(sides));
			for (int i = 0; i < 30; i++)
			{
				Assert.Equal(0, node.Evaluate());
			}
		}

		[Fact]
		public void DiceSimulationNode_CheckBaseUniformDistribution()
		{
			var node = new DiceSimulationNode();
			node.AddChildren(new IntegerNode(1), new IntegerNode(20));
			int[] values = new int[20];
			for (int i = 0; i < 100000; i++)
			{
				int result = (int)node.Evaluate();
				values[result - 1]++;
			}
			var expected = 100000 / 20;
			foreach (int count in values)
			{
				Assert.True(count >= expected * 0.8 && count <= expected * 1.2);
			}
		}
	}
}
