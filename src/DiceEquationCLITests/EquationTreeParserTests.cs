using DiceEquationCLI;

namespace DiceEquationCLITests
{
	public class EquationTreeParserTests
	{
		// ========== Helper Functions ==========

		public string[] stringToCharStringArray_NoWhitespace(string input)
		{
			return input.ToCharArray().Where(c => !char.IsWhiteSpace(c)).Select(c => c.ToString()).ToArray();
		}

		// ========== Operator Registration Tests ==========

		[Fact]
		public void OperatorSymbols_IncludesInitialOperators()
		{
			var parser = new EquationTreeParser();
			var symbols = parser.OperatorSymbols;

			Assert.Contains('+', symbols);
			Assert.Contains('-', symbols);
			Assert.Contains('*', symbols);
			Assert.Contains('/', symbols);
			Assert.Contains('d', symbols);
		}

		[Fact]
		public void AddBinaryOperand_NewOperator()
		{
			var parser = new EquationTreeParser();
			parser.AddBinaryOperand('%', 3, () => new AdditionNode());
		}

		[Fact]
		public void AddBinaryOperand_ThrowWhenDuplicate()
		{
			var parser = new EquationTreeParser();
			Assert.Throws<ArgumentException>(() => parser.AddBinaryOperand('+', 3, () => new AdditionNode()));
		}

		[Fact]
		public void AddUnaryOperand_NewOperator()
		{
			var parser = new EquationTreeParser();
			parser.AddUnaryOperand('!', 10, () => new NegationNode());
		}

		[Fact]
		public void AddUnaryOperand_ThrowWhenDuplicate()
		{
			var parser = new EquationTreeParser();
			Assert.Throws<ArgumentException>(() => parser.AddUnaryOperand('-', 10, () => new NegationNode()));
		}

		[Fact]
		public void OperatorSymbols_IncludesRegisteredOperators()
		{
			var parser = new EquationTreeParser();
			parser.AddBinaryOperand('%', 3, () => new AdditionNode());
			parser.AddUnaryOperand('!', 10, () => new NegationNode());
			var symbols = parser.OperatorSymbols;

			Assert.Contains('%', symbols);
			Assert.Contains('!', symbols);
		}

		// ========== EquationToTokens Tests ==========

		[Theory]
		[InlineData("1")]
		[InlineData("1+2")]
		[InlineData("3*4")]
		[InlineData("5-6")]
		[InlineData("6/2")]
		[InlineData("1d6")]
		[InlineData("-8")]
		[InlineData("--8")]
		[InlineData("(9)")]
		[InlineData("1+2*3-4/5")]
		[InlineData("(1+2)*(3-4)/5")]
		public void EquationToTokens_SingleCharacterTokenEquations(string equation)
		{
			var parser = new EquationTreeParser();
			var tokens = parser.EquationToTokens(equation);
			var expectedTokens = stringToCharStringArray_NoWhitespace(equation);
			Assert.Equal(expectedTokens, tokens);
		}

		[Theory]
		[InlineData("1")]
		[InlineData("1 + 2")]
		[InlineData("3 * 4")]
		[InlineData("5 - 6")]
		[InlineData("6 / 2")]
		[InlineData("1 d 6")]
		[InlineData("- 8 ")]
		[InlineData("( 9 )")]
		[InlineData("1 + 2 * 3 - 4 / 5")]
		[InlineData("( 1 + 2 ) * ( 3 - 4 ) / 5")]
		public void EquationToTokens_SingleSpaceDelimitedTokenEquations(string equation)
		{
			var parser = new EquationTreeParser();
			var tokens = parser.EquationToTokens(equation);
			var expectedTokens = stringToCharStringArray_NoWhitespace(equation);
			Assert.Equal(expectedTokens, tokens);
		}

		[Theory]
		[InlineData("      1         ")]
		[InlineData("           1 \t +          2\t")]
		[InlineData(" \t\t3         *   4 \t\t ")]
		[InlineData("  5  -  6  ")]
		[InlineData(" 6 / 2 ")]
		[InlineData("\t1\td\t6\t")]
		[InlineData("  -     8   ")]
		[InlineData("   (    9      ) ")]
		[InlineData("     1     +      2     *       3      -      4       /       5       ")]
		[InlineData("    (        1     +     2     )     *       (       3        -        4      )      /     5     ")]
		public void EquationToTokens_ExcessiveWhitespaceTokenEquations(string equation)
		{
			var parser = new EquationTreeParser();
			var tokens = parser.EquationToTokens(equation);
			var expectedTokens = stringToCharStringArray_NoWhitespace(equation);
			Assert.Equal(expectedTokens, tokens);
		}

		[Theory]
		[InlineData("      1")]
		[InlineData("           1+          2\t")]
		[InlineData(" \t\t3         *4 \t ")]
		[InlineData("5-  6  ")]
		[InlineData(" 6/2 ")]
		[InlineData("\t1d\t6\t")]
		[InlineData("-     8")]
		[InlineData("   (    9) ")]
		[InlineData("     1+2     *3-      4/5       ")]
		[InlineData("    (1     +     2)     *(       3-4      )      /5")]
		public void EquationToTokens_MixedSpacesTokenEquations(string equation)
		{
			var parser = new EquationTreeParser();
			var tokens = parser.EquationToTokens(equation);
			var expectedTokens = stringToCharStringArray_NoWhitespace(equation);
			Assert.Equal(expectedTokens, tokens);
		}

		// ========== InfixToPostfix Tests ==========

		[Theory]
		[InlineData("1 + 2", "1 2 +")]
		[InlineData("3 * 4", "3 4 *")]
		[InlineData("5 - 6", "5 6 -")]
		[InlineData("6 / 2", "6 2 /")]
		[InlineData("1 d 6", "1 6 d")]
		public void InfixToPostfix_SimpleEquations(string infix, string postfix)
		{
			var infixArray = stringToCharStringArray_NoWhitespace(infix);
			var postfixArray = stringToCharStringArray_NoWhitespace(postfix);

			var parser = new EquationTreeParser();
			var result = parser.InfixToPostfix(infixArray);
			Assert.Equal(postfixArray, result);
		}


		[Theory]
		[InlineData("1 + 2 * 3 / 4", "1 2 3 * 4 / +")]
		[InlineData("3 * 4 + 1 - 3", "3 4 * 1 + 3 -")]
		[InlineData("5 - 6 * 8 + 4 / 2", "5 6 8 * - 4 2 / +")]
		[InlineData("1 d 6 d 5 * 2 d 5 / 4", "1 6 d 5 d 2 5 d * 4 /")]
		public void InfixToPostfix_ComplexEquations_NoParenthesis(string infix, string postfix)
		{
			var infixArray = stringToCharStringArray_NoWhitespace(infix);
			var postfixArray = stringToCharStringArray_NoWhitespace(postfix);

			var parser = new EquationTreeParser();
			var result = parser.InfixToPostfix(infixArray);
			Assert.Equal(postfixArray, result);
		}

		[Theory]
		[InlineData("(1 + ((2 * 3) / 4))", "1 2 3 * 4 / +")]
		[InlineData("(((3 * 4) + 1) - 3)", "3 4 * 1 + 3 -")]
		[InlineData("((5 - (6 * 8)) + (4 / 2))", "5 6 8 * - 4 2 / +")]
		[InlineData("(((1 d 6) d 5) * ((2 d 5) / 4))", "1 6 d 5 d 2 5 d 4 / *")]
		public void InfixToPostfix_ComplexEquations_UselessParenthesis(string infix, string postfix)
		{
			var infixArray = stringToCharStringArray_NoWhitespace(infix);
			var postfixArray = stringToCharStringArray_NoWhitespace(postfix);

			var parser = new EquationTreeParser();
			var result = parser.InfixToPostfix(infixArray);
			Assert.Equal(postfixArray, result);
		}

		[Theory]
		[InlineData("(1 + 2) * (3 / 4)", "1 2 + 3 4 / *")]
		[InlineData("3 * (4 + (1 - 3))", "3 4 1 3 - + *")]
		[InlineData("(5 - 6) * ((8 + 4) / 2)", "5 6 - 8 4 + 2 / *")]
		[InlineData("(1 d (6 d ((5 * 2) d 5))) / 4", "1 6 5 2 * 5 d d d 4 /")]
		public void InfixToPostfix_ComplexEquations_Parenthesis(string infix, string postfix)
		{
			var infixArray = stringToCharStringArray_NoWhitespace(infix);
			var postfixArray = stringToCharStringArray_NoWhitespace(postfix);

			var parser = new EquationTreeParser();
			var result = parser.InfixToPostfix(infixArray);
			Assert.Equal(postfixArray, result);
		}

		[Theory]
		[InlineData("-1", "1 u-")]
		[InlineData("--1", "1 u- u-")]
		[InlineData("-1 - 5", "1 u- 5 -")]
		[InlineData("5 - -1", "5 1 u- -")]
		[InlineData("-1 - -5 * 4", "1 u- 5 u- 4 * -")]
		[InlineData("6 * -5 + -4 * 2", "6 5 u- * 4 u- 2 * +")]
		public void InfixToPostfix_UnaryOperatorEquations(string infix, string postfix)
		{
			var infixArray = stringToCharStringArray_NoWhitespace(infix);
			// Split manually because unary operators are changed to two characters
			var postfixArray = postfix.Split(' ').ToArray();

			var parser = new EquationTreeParser();
			var result = parser.InfixToPostfix(infixArray);
			Assert.Equal(postfixArray, result);
		}

		// ========== Simple Expression Tests ==========

		[Fact]
		public void ConstructTree_SingleNumber()
		{
			var parser = new EquationTreeParser();
			var tree = parser.ConstructTree("42");

			Assert.NotNull(tree);
			Assert.Equal(42, tree.Evaluate());
		}

		[Fact]
		public void ConstructTree_DoubleNumber()
		{
			var parser = new EquationTreeParser();
			var tree = parser.ConstructTree("42.5");

			Assert.NotNull(tree);
			Assert.Equal(42.5, tree.Evaluate());
		}

		[Fact]
		public void ConstructTree_Addition()
		{
			var parser = new EquationTreeParser();
			var tree = parser.ConstructTree("1 + 2");

			Assert.NotNull(tree);
			Assert.Equal(3, tree.Evaluate());
		}

		[Fact]
		public void ConstructTree_Subtraction()
		{
			var parser = new EquationTreeParser();
			var tree = parser.ConstructTree("5 - 3");

			Assert.NotNull(tree);
			Assert.Equal(2, tree.Evaluate());
		}

		[Fact]
		public void ConstructTree_Multiplication()
		{
			var parser = new EquationTreeParser();
			var tree = parser.ConstructTree("3 * 4");

			Assert.NotNull(tree);
			Assert.Equal(12, tree.Evaluate());
		}

		[Fact]
		public void ConstructTree_Division()
		{
			var parser = new EquationTreeParser();
			var tree = parser.ConstructTree("10 / 2");

			Assert.NotNull(tree);
			Assert.Equal(5, tree.Evaluate());
		}

		[Fact]
		public void ConstructTree_DiceSimulation()
		{
			var parser = new EquationTreeParser();
			var tree = parser.ConstructTree("2d6");

			Assert.NotNull(tree);
			Assert.True(tree is DiceSimulationNode);
		}

		[Fact]
		public void ConstructTree_Negation()
		{
			var parser = new EquationTreeParser();
			var tree = parser.ConstructTree("-5");

			Assert.NotNull(tree);
			Assert.True(tree is NegationNode);
			Assert.Equal(-5, tree.Evaluate());
		}

		// ========== Operator Priority Tests ==========

		[Theory]
		[InlineData("2 + 3 * 4", 14)]
		[InlineData("10 - 2 * 3 / 2 + 1", 8)]
		[InlineData("12 / 2 + 3 - 2 * 5", -1)]
		[InlineData("12 / 2 * 4 / 2 + 1 * 2 * 3", 18)]
		// We can use Xd1 to test the dice operator whilst being deterministic
		[InlineData("10 - 5d1 + 4d1", 9)]
		[InlineData("2d1 * 10 / 5d1", 4)]
		public void ConstructTree_FollowsOperatorPriority(string expression, double expected)
		{
			var parser = new EquationTreeParser();
			var tree = parser.ConstructTree(expression);

			Assert.Equal(expected, tree.Evaluate());
		}

		[Theory]
		[InlineData("(2 + 3) * 4", 20)]
		[InlineData("(5 - 2) / 3", 1)]
		[InlineData("2 * (3 + 4)", 14)]
		[InlineData("((2 + 3) * 4) / (2 + 2) + 5", 10)]
		[InlineData("(5 + 3)d(5 - 4) - 4d1", 4)]
		[InlineData("((2 + 3) * 4)", 20)]
		[InlineData("((((1 + 1))))", 2)]
		[InlineData("()()1 + 1()()", 2)]
		public void ConstructTree_Parentheses(string expression, double expected)
		{
			var parser = new EquationTreeParser();
			var tree = parser.ConstructTree(expression);

			Assert.Equal(expected, tree.Evaluate());
		}

		// ========== Complex Expression Tests ==========

		[Theory]
		[InlineData("1d6")]
		public void ConstructTree_ComplexDiceConstruction(string expression)
		{
			var parser = new EquationTreeParser();
			var tree = parser.ConstructTree(expression);

			// Writing a probabilistic test will be fun...
			throw new NotImplementedException();

		}

		[Theory]
		[InlineData("-5", -5)]
		[InlineData("-0", 0)]
		[InlineData("-(3 + 2)", -5)]
		[InlineData("-(-5)", 5)]
		[InlineData("--5", 5)]
		public void ConstructTree_ComplexNegation(string expression, double expected)
		{
			var parser = new EquationTreeParser();
			var tree = parser.ConstructTree(expression);

			Assert.Equal(expected, tree.Evaluate());
		}

		// ========== Whitespace Tests ==========
		// Just a small test, since internally it uses EquationToTokens which has much better whitespace tests

		[Theory]
		[InlineData("1+2", 3)]
		[InlineData("1 + 2", 3)]
		[InlineData("1  +  2", 3)]
		[InlineData("  1 + 2  ", 3)]
		[InlineData("1\t+\t2", 3)]
		public void ConstructTree_HandlesWhitespace(string expression, double expected)
		{
			var parser = new EquationTreeParser();
			var tree = parser.ConstructTree(expression);

			Assert.Equal(expected, tree.Evaluate());
		}

		// ========== Error Handling Tests ==========

		[Theory]
		[InlineData("1 + ")]         // Missing operand
		[InlineData("+ 2")]          // Missing left operand
		[InlineData("1 + + 2")]      // Double operator
		[InlineData("")]             // Empty expression
		[InlineData("1 2 3")]        // Missing operator (not so much missign operator as just extra nodes left in the stack)
		public void ConstructTree_InvalidExpressions_ThrowException(string expression)
		{
			var parser = new EquationTreeParser();
			Assert.Throws<ArgumentException>(() => parser.ConstructTree(expression));
		}

		[Theory]
		[InlineData("(1 + 2")]
		[InlineData("1 + 2)")]
		[InlineData("(())1 + 2)")]
		public void ConstructTree_MismatchedParentheses_ThrowException(string expression)
		{
			var parser = new EquationTreeParser();
			Assert.Throws<ArgumentException>(() => parser.ConstructTree(expression));
		}

		[Theory]
		[InlineData("1 a 2")]
		[InlineData("1 test 2")]
		[InlineData("1 t+st 2")]
		[InlineData("1 & 2")]
		[InlineData("1 ^ 2")]
		public void ConstructTree_InvalidCharacters_ThrowException(string expression)
		{
			var parser = new EquationTreeParser();
			Assert.Throws<ArgumentException>(() => parser.ConstructTree(expression));
		}
	}
}
