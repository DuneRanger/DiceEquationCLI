using DiceEquationCLI;

namespace DiceEquationCLITests
{
	public class EquationManagerTests
	{
		// ========== AddEquation Tests ==========

		[Fact]
		public void AddEquation_AddSingle()
		{
			var manager = new EquationManager();
			manager.AddEquation("simple", "1 + 2");
			Assert.True(manager.HasEquation("simple"));
		}

		[Fact]
		public void AddEquation_AddMultiple()
		{
			var manager = new EquationManager();
			manager.AddEquation("first", "1 + 2");
			manager.AddEquation("second", "3 * 4");
			manager.AddEquation("third", "5 - 1");

			Assert.True(manager.HasEquation("first"));
			Assert.True(manager.HasEquation("second"));
			Assert.True(manager.HasEquation("third"));
		}

		[Fact]
		public void AddEquation_ThrowWhenDuplicateIdentifier()
		{
			var manager = new EquationManager();
			manager.AddEquation("dup", "1 + 2");

			Assert.Throws<ArgumentException>(() => manager.AddEquation("dup", "3 + 4"));
		}

		[Fact]
		public void AddEquation_CaseSensitive()
		{
			var manager = new EquationManager();
			manager.AddEquation("simple", "1 + 2");
			manager.AddEquation("Simple", "1 + 2");
		}

		[Fact]
		public void AddEquation_ComplexEquation()
		{
			var manager = new EquationManager();
			manager.AddEquation("complex", "2d6 + 3 * (1d4 - 1)");

			Assert.True(manager.HasEquation("complex"));
		}

		// ========== HasEquation Tests ==========

		[Fact]
		public void HasEquation_ReturnFalseForNonExistentEquation()
		{
			var manager = new EquationManager();
			Assert.False(manager.HasEquation("nonexistent"));
		}

		[Fact]
		public void HasEquation_ReturnTrueForExistingEquation()
		{
			var manager = new EquationManager();
			manager.AddEquation("exists", "1 + 2");
			Assert.True(manager.HasEquation("exists"));
		}

		[Fact]
		public void HasEquation_CaseSensitive()
		{
			var manager = new EquationManager();
			manager.AddEquation("MyEquation", "1 + 2");
			Assert.True(manager.HasEquation("MyEquation"));
			Assert.False(manager.HasEquation("myequation"));
		}

		// ========== GetEquation Tests ==========

		[Fact]
		public void GetEquation_ReturnsCorrectEquation()
		{
			var manager = new EquationManager();
			manager.AddEquation("test", "1 + 2");
			var equation = manager.GetEquation("test");

			Assert.NotNull(equation);
			Assert.Equal("1 + 2", equation.ToString());
			Assert.Equal("test", equation.Name);
		}

		[Fact]
		public void GetEquation_ThrowsWhenEquationNotExists()
		{
			var manager = new EquationManager();
			Assert.Throws<ArgumentException>(() => manager.GetEquation("nonexistent"));
		}

		[Fact]
		public void GetEquation_MultipleEquations()
		{
			var manager = new EquationManager();
			manager.AddEquation("first", "1 + 1");
			manager.AddEquation("second", "2 + 2");
			manager.AddEquation("third", "3 + 3");

			var eq1 = manager.GetEquation("first");
			var eq2 = manager.GetEquation("second");
			var eq3 = manager.GetEquation("third");

			Assert.Equal("1 + 1", eq1.ToString());
			Assert.Equal("2 + 2", eq2.ToString());
			Assert.Equal("3 + 3", eq3.ToString());
		}

		[Fact]
		public void GetEquation_CaseSensitive()
		{
			var manager = new EquationManager();
			manager.AddEquation("simple", "5 * 3");
			manager.AddEquation("Simple", "5 * 5");

			var equation = manager.GetEquation("simple");

			Assert.Equal("5 * 3", equation.ToString());
			Assert.Equal("simple", equation.Name);

			equation = manager.GetEquation("Simple");

			Assert.Equal("5 * 5", equation.ToString());
			Assert.Equal("Simple", equation.Name);
		}

		// ========== DeleteEquation Tests ==========

		[Fact]
		public void DeleteEquation_RemovesEquation()
		{
			var manager = new EquationManager();
			manager.AddEquation("todelete", "1 + 2");

			manager.DeleteEquation("todelete");
			Assert.False(manager.HasEquation("todelete"));
		}

		[Fact]
		public void DeleteEquation_ThrowsWhenEquationNotExists()
		{
			var manager = new EquationManager();
			Assert.Throws<ArgumentException>(() => manager.DeleteEquation("nonexistent"));
		}

		[Fact]
		public void DeleteEquation_DeletesOnlySpecifiedEquation()
		{
			var manager = new EquationManager();
			manager.AddEquation("keep1", "1 + 2");
			manager.AddEquation("delete", "3 + 4");
			manager.AddEquation("keep2", "5 + 6");

			manager.DeleteEquation("delete");

			Assert.True(manager.HasEquation("keep1"));
			Assert.False(manager.HasEquation("delete"));
			Assert.True(manager.HasEquation("keep2"));
		}

		[Fact]
		public void DeleteEquation_CanReAddDeletedEquation()
		{
			var manager = new EquationManager();
			manager.AddEquation("equation", "1 + 2");
			manager.DeleteEquation("equation");

			manager.AddEquation("equation", "3 + 4");
			Assert.True(manager.HasEquation("equation"));
		}

		[Fact]
		public void DeleteEquation_CaseSensitive()
		{
			var manager = new EquationManager();
			manager.AddEquation("todelete", "1 + 2");
			manager.AddEquation("ToDelete", "3 + 4");

			manager.DeleteEquation("todelete");
			manager.DeleteEquation("ToDelete");
			Assert.False(manager.HasEquation("todelete"));
			Assert.False(manager.HasEquation("ToDelete"));
		}

		// ========== EnumerateEquations Tests ==========

		[Fact]
		public void EnumerateEquations_EmptyManager()
		{
			var manager = new EquationManager();
			var enumerable = manager.EnumerateEquations();
			var list = enumerable.ToList();

			Assert.Empty(list);
		}

		[Fact]
		public void EnumerateEquations_SingleEquation()
		{
			var manager = new EquationManager();
			manager.AddEquation("eq1", "1 + 1");

			var enumerable = manager.EnumerateEquations();
			var list = enumerable.ToList();

			Assert.Single(list);
			Assert.Equal("eq1", list[0].Key);
			Assert.Equal("1 + 1", list[0].Value.ToString());
		}

		[Fact]
		public void EnumerateEquations_MultipleEquations()
		{
			var manager = new EquationManager();
			string[] names = ["eq1", "eq2", "eq3"];
			string[] equations = ["1 + 1", "2 + 2", "3 + 3"];
			for (int i = 0; i < names.Length; i++)
			{
				manager.AddEquation(names[i], equations[i]);
			}

			var enumerable = manager.EnumerateEquations();
			var list = enumerable.ToList();

			Assert.Equal(3, list.Count);
			Assert.True(list.All(kvp => names.Contains(kvp.Key)));
			Assert.True(list.All(kvp => equations.Contains(kvp.Value.ToString())));
		}

		[Fact]
		public void EnumerateEquations_DoesNotContainOldDeleted()
		{
			var manager = new EquationManager();
			string[] names = ["eq1", "eq2", "eq3"];
			string[] equations = ["1 + 1", "2 + 2", "3 + 3"];
			for (int i = 0; i < names.Length; i++)
			{
				manager.AddEquation(names[i], equations[i]);
			}

			manager.DeleteEquation("eq2");

			var enumerable = manager.EnumerateEquations();
			var list = enumerable.ToList();

			Assert.Equal(2, list.Count);
			Assert.DoesNotContain(list, kv => kv.Key == "eq2");
		}

		[Fact]
		public void EnumerateEquations_InvalidationAfterDeletion()
		{
			var manager = new EquationManager();
			string[] names = ["eq1", "eq2", "eq3"];
			string[] equations = ["1 + 1", "2 + 2", "3 + 3"];
			for (int i = 0; i < names.Length; i++)
			{
				manager.AddEquation(names[i], equations[i]);
			}


			var enumerable = manager.EnumerateEquations();

			manager.DeleteEquation("eq2");

			foreach (var kvp in enumerable)
			{
				Assert.True(manager.HasEquation(kvp.Key));
			}
		}

		// ========== ClearEquations Tests ==========

		[Fact]
		public void ClearEquations_EmptyManager()
		{
			var manager = new EquationManager();
			manager.ClearEquations();

			Assert.Empty(manager.EnumerateEquations());
		}

		[Fact]
		public void ClearEquations_RemovesAllEquations()
		{
			var manager = new EquationManager();
			string[] names = ["eq1", "eq2", "eq3"];
			string[] equations = ["1 + 1", "2 + 2", "3 + 3"];
			for (int i = 0; i < names.Length; i++)
			{
				manager.AddEquation(names[i], equations[i]);
			}

			manager.ClearEquations();

			Assert.Empty(manager.EnumerateEquations());
			Assert.True(names.All(name => !manager.HasEquation(name)));
		}

		[Fact]
		public void ClearEquations_AddAfterClear()
		{
			var manager = new EquationManager();
			manager.AddEquation("eq1", "1 + 1");
			manager.ClearEquations();
			manager.AddEquation("eq1", "2 + 2");

			Assert.True(manager.HasEquation("eq1"));
			Assert.Equal("2 + 2", manager.GetEquation("eq1").ToString());
		}
	}
}
