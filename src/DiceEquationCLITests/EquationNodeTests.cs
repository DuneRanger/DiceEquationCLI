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

		// ========== Custom Operator Node Evaluation Tests ==========
	}
}
