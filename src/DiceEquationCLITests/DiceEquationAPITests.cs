using DiceEquationCLI;
using System.ComponentModel.DataAnnotations;

namespace DiceEquationCLITests
{
	public class DiceEquationAPITests
	{
		Tuple<DiceEquationAPI, StringReader, StringWriter> GetNewApiInstance()
		{
			var input = new StringReader("");
			var output = new StringWriter();
			var api = new DiceEquationAPI(input, output);

			return Tuple.Create(api, input, output);
		}

		bool CheckExclusiveArgumentCount_IdentifierVersion(int expectedArgCount, Action<string[]> commandAction)
		{
			List<string> args = new List<string>();
			for (int i = 1; i <= 10; i++)
			{
				args.Add($"fake_identifier{i}");
				if (i != expectedArgCount) Assert.Throws<ArgumentException>(() => commandAction(args.ToArray()));
				else
				{
					try
					{
						commandAction(args.ToArray());
					}
					catch (ArgumentException ex)
					{
						if (ex.Message.Contains("argument count")) throw;
					}
				}
			}
			return true;
		}

		bool CheckExclusiveArgumentCount_FilePathVersion(int expectedArgCount, Action<string[]> commandAction)
		{
			List<string> args = new List<string>();
			for (int i = 1; i <= 10; i++)
			{
				args.Add($"fake_file_path{i}.txt");
				if (i == expectedArgCount) commandAction(args.ToArray());
				else Assert.Throws<ArgumentException>(() => commandAction(args.ToArray()));
			}
			return true;
		}

		static string[] invalidIdentifiers = new string[] {
											"1invalid",
											"inv@lid",
											"inv-alid",
											"inv.alid",
											"inv alid",
											" invalid",
											"inv alid ",
											"inv\tl\nid"
										};
		static string[] validIdentifiers = new string[] {
											"_valid",
											"valid_",
											"valid2345",
											"valid",
											"______",
											"__1_1__",
											"a",
											"a2",
											"_",
										};

		// ========== Assert Valid Identifier Tests ==========

		[Fact]
		public void AssertValidIdentifier_ThrowsOnInvalidIdentifiers()
		{
			var (api, input, output) = GetNewApiInstance();
			foreach (var identifier in invalidIdentifiers)
			{
				Assert.Throws<ArgumentException>(() => api.AssertValidIdentifier(identifier));
			}
			foreach (var identifier in validIdentifiers)
			{
				api.AssertValidIdentifier(identifier);
			}
		}


		// ========== Constructor Tests ==========

		[Fact]
		public void Constructor_InitializesWithInputOutput()
		{
			var (api, input, output) = GetNewApiInstance();

			Assert.NotNull(api);
		}

		// ========== Help Command Tests ==========

		[Fact]
		public void PrintHelp_PrintsHelpMessage()
		{
			var (api, input, output) = GetNewApiInstance();

			api.PrintHelp(new string[] { });
			var result = output.ToString();


			Assert.True(result.Length > 0);
		}

		[Fact]
		public void PrintHelp_ContainsBaseCommands()
		{
			var (api, input, output) = GetNewApiInstance();

			api.PrintHelp(new string[] { });
			var result = output.ToString();

			string[] baseCommands = { "help", "exit", "save", "del", "eval", "show", "prob", "export", "import" };
			Assert.True(baseCommands.All(cmd => result.Contains(cmd)));
		}

		[Fact]
		public void PrintHelp_OnlyZeroArguments()
		{
			var (api, input, output) = GetNewApiInstance();
			Assert.True(CheckExclusiveArgumentCount_IdentifierVersion(0, api.PrintHelp));
		}

		// ========== Exit Command Tests ==========

		[Fact]
		public void ExitProgram_DoesNotThrow()
		{
			var (api, input, output) = GetNewApiInstance();

			api.ExitProgram(new string[] { });
			// Since I can't actually check the flag is set and anything to do with Run should be in Run tests
			// Then this test will just make sure the function isn't throwing for no reason
		}

		[Fact]
		public void ExitProgram_ThrowsWithArguments()
		{
			var (api, input, output) = GetNewApiInstance();

			Assert.True(CheckExclusiveArgumentCount_IdentifierVersion(0, api.ExitProgram));
		}

		// ========== Save Command Tests ==========

		[Fact]
		public void SaveEquation_SingleEquation()
		{
			var (api, input, output) = GetNewApiInstance();
			api.SaveEquation(new[] { "test=1+1" });
		}

		[Theory]
		[InlineData("test=1+1*2-3/4")]
		[InlineData("test=(1+1)*2-3d(4*2)")]
		[InlineData("test=(4-2)d6*8/3+(4d6)")]
		public void SaveEquation_ComplexEquations(string equation)
		{
			var (api, input, output) = GetNewApiInstance();
			api.SaveEquation(new[] { equation });
		}

		[Fact]
		public void SaveEquation_ThrowsWithInvalidIdentifier()
		{
			var (api, input, output) = GetNewApiInstance();

			for (int i = 0; i < invalidIdentifiers.Length; i++)
			{
				var identifier = invalidIdentifiers[i];
				var equation = $"{identifier}=1+1";
				Assert.Throws<ArgumentException>(() => api.SaveEquation(new[] { equation }));
			}
		}

		[Fact]
		public void SaveEquation_AcceptsValidIdentifiers()
		{
			var (api, input, output) = GetNewApiInstance();

			for (int i = 0; i < validIdentifiers.Length; i++)
			{
				var identifier = validIdentifiers[i];
				var equation = $"{identifier}=1+1";
				api.SaveEquation(new[] { equation });
			}
		}

		[Fact]
		public void SaveEquation_AcceptsAnyArgCount()
		{
			var (api, input, output) = GetNewApiInstance();
			api.SaveEquation(new[] { "test=1+1" });
			api.SaveEquation(new[] { "test2=", "1+1" });
			api.SaveEquation(new[] { "test3", "=1+1" });
			api.SaveEquation(new[] { "test4", "=", "1", "+", "1" });

			List<string> longEq = ["tempName", "=", "1"];
			var baseName = "longTest";

			for (int i = 0; i < 5; i++) {
				longEq.Add("+");
				longEq.Add("1");
				longEq[0] = $"{baseName}{i}";
				api.SaveEquation(longEq.ToArray());
			}
		}

		// ========== Delete Command Tests ==========

		[Fact]
		public void DeleteEquation_DeleteNonexistent_Throws()
		{
			var (api, input, output) = GetNewApiInstance();

			Assert.Throws<ArgumentException>(() => api.DeleteEquation(new[] { "test" }));
		}

		[Fact]
		public void DeleteEquation_DeletesSpecificEquation()
		{
			var (api, input, output) = GetNewApiInstance();
			api.SaveEquation(new[] { "test=1+1" });
			api.SaveEquation(new[] { "test2=1+1" });

			api.DeleteEquation(new[] { "test" });
			Assert.Throws<ArgumentException>(() => api.DeleteEquation(new[] { "test" }));
			// Not throwing for test2 means that it didn't delete it
			api.DeleteEquation(new[] { "test2" });
		}

		[Fact]
		public void DeleteEquation_DeleteAllFromEmptyManager_DoesNotThrow()
		{
			var (api, input, output) = GetNewApiInstance();

			api.DeleteEquation(new[] { "all" });
		}

		[Fact]
		public void DeleteEquation_DeleteAll()
		{
			var (api, input, output) = GetNewApiInstance();
			api.SaveEquation(new[] { "test=1+1" });
			api.SaveEquation(new[] { "test2=1+1" });

			api.DeleteEquation(new[] { "all" });
			Assert.Throws<ArgumentException>(() => api.DeleteEquation(new[] { "test" }));
			Assert.Throws<ArgumentException>(() => api.DeleteEquation(new[] { "test2" }));
		}

		[Fact]
		public void DeleteEquation_ThrowsWithInvalidArgumentCount()
		{
			var (api, input, output) = GetNewApiInstance();

			Assert.True(CheckExclusiveArgumentCount_IdentifierVersion(1, api.DeleteEquation));
		}

		[Fact]
		public void DeleteEquation_AcceptsValidIdentifiers()
		{
			var (api, input, output) = GetNewApiInstance();

			for (int i = 0; i < validIdentifiers.Length; i++)
			{
				var identifier = validIdentifiers[i];
				var equation = $"{identifier}=1+1";
				api.SaveEquation(new[] { equation });
				api.DeleteEquation(new[] { identifier });
			}
		}

		// ========== Evaluate Command Tests ==========

		[Fact]
		public void EvaluateEquation_EvalBasicEquation()
		{
			var (api, input, output) = GetNewApiInstance();
			api.SaveEquation(new[] { "test=1+1" });
			api.EvaluateEquation(new[] { "test" });

			var result = output.ToString();
			Assert.Contains("2", result);
		}

		[Fact]
		public void EvaluateEquation_EvalNonexistent_Throws()
		{
			var (api, input, output) = GetNewApiInstance();

			Assert.Throws<ArgumentException>(() => api.EvaluateEquation(new[] { "nonexistent" }));
		}

		[Fact]
		public void EvaluateEquation_EvalAllForEmptyManager_DoesNotThrow()
		{
			var (api, input, output) = GetNewApiInstance();

			api.EvaluateEquation(new[] { "all" });
			var result = output.ToString();
			Assert.Equal("", result);
		}

		[Fact]
		public void EvaluateEquation_EvalAll()
		{
			var (api, input, output) = GetNewApiInstance();
			var mupltipleEquations = new string[] {
				"test=1+1",
				"test2=1+2",
				"test3=1+3",
				"test4=1+4",
				"test5=1+5",
				"test6=1+6",
			};
			foreach (var eq in mupltipleEquations)
			{
				api.SaveEquation(new[] { eq });
			}
			api.EvaluateEquation(new[] { "all" });
			var result = output.ToString();

			string[] expectedResults = { "2", "3", "4", "5", "6", "7" };
			Assert.True(expectedResults.All(res => result.Contains(res)));
		}

		[Fact]
		public void EvaluateEquation_ThrowsWithInvalidArgumentCount()
		{
			var (api, input, output) = GetNewApiInstance();

			Assert.True(CheckExclusiveArgumentCount_IdentifierVersion(1, api.EvaluateEquation));
		}

		[Fact]
		public void EvaluateEquation_AcceptsValidIdentifiers()
		{
			var (api, input, output) = GetNewApiInstance();

			for (int i = 0; i < validIdentifiers.Length; i++)
			{
				var identifier = validIdentifiers[i];
				var equation = $"{identifier}=1+1";
				api.SaveEquation(new[] { equation });
				api.EvaluateEquation(new[] { identifier });
			}
		}

		// ========== Show Equation String Command Tests ==========

		[Fact]
		public void ShowEquationString_ShowBasicEquation()
		{
			var (api, input, output) = GetNewApiInstance();
			api.SaveEquation(new[] { "test=1+1" });
			api.ShowEquationString(new[] { "test" });

			var result = output.ToString();
			Assert.Contains("1+1", result);
		}


		[Fact]
		public void ShowEquationString_ThrowsWhenEquationNotExists()
		{
			var (api, input, output) = GetNewApiInstance();

			Assert.Throws<ArgumentException>(() => api.ShowEquationString(new[] { "nonexistent" }));
		}

		[Fact]
		public void ShowEquationString_ShowsAll_EmptyManager()
		{
			var (api, input, output) = GetNewApiInstance();

			api.ShowEquationString(new[] { "all" });
			var result = output.ToString();
			Assert.Equal("", result);
		}

		[Fact]
		public void ShowEquationString_ShowAll()
		{
			var (api, input, output) = GetNewApiInstance();
			var mupltipleEquations = new string[] {
				"test=1+1",
				"test2=1+2",
				"test3=1+3",
				"test4=1+4",
				"test5=1+5",
				"test6=1+6",
			};
			foreach (var eq in mupltipleEquations)
			{
				api.SaveEquation(new[] { eq });
			}
			api.ShowEquationString(new[] { "all" });
			var result = output.ToString();

			string[] expectedResults = { "1+1", "1+2", "1+3", "1+4", "1+5", "1+6" };
			Assert.Contains("1+1", result);
			Assert.Contains("1+2", result);
			Assert.Contains("1+3", result);
			Assert.Contains("1+4", result);
			Assert.Contains("1+5", result);
			Assert.Contains("1+6", result);
			Assert.True(expectedResults.All(res => result.Contains(res)));
		}

		[Fact]
		public void ShowEquationString_ThrowsWithInvalidArgumentCount()
		{
			var (api, input, output) = GetNewApiInstance();

			Assert.True(CheckExclusiveArgumentCount_IdentifierVersion(1, api.ShowEquationString));
		}

		[Fact]
		public void ShowEquationString_AcceptsValidIdentifiers()
		{
			var (api, input, output) = GetNewApiInstance();

			for (int i = 0; i < validIdentifiers.Length; i++)
			{
				var identifier = validIdentifiers[i];
				var equation = $"{identifier}=1+1";
				api.SaveEquation(new[] { equation });
				api.ShowEquationString(new[] { identifier });
			}
		}

		// ========== Show Equation Distribution Command Tests ==========

		[Fact]
		public void ShowEquationDistribution_ShowBasicEquation_OutputNotChecked()
		{
			var (api, input, output) = GetNewApiInstance();
			api.SaveEquation(new[] { "test=1d4" });
			api.ShowEquationDistribution(new[] { "test" });

			var result = output.ToString();
			// Not sure how to check this best tbh
			// For now I'll take not throwing as a success
		}

		[Fact]
		public void ShowEquationDistribution_ThrowsWhenEquationNotExists()
		{
			var (api, input, output) = GetNewApiInstance();

			Assert.Throws<ArgumentException>(() => api.ShowEquationDistribution(new[] { "nonexistent" }));
		}

		[Fact]
		public void ShowEquationDistribution_ThrowsWithInvalidArgumentCount()
		{
			var (api, input, output) = GetNewApiInstance();

			Assert.True(CheckExclusiveArgumentCount_IdentifierVersion(1, api.ShowEquationDistribution));
		}

		[Fact]
		public void ShowEquationDistribution_AcceptsValidIdentifiers()
		{
			var (api, input, output) = GetNewApiInstance();

			for (int i = 0; i < validIdentifiers.Length; i++)
			{
				var identifier = validIdentifiers[i];
				var equation = $"{identifier}=1+1";
				api.SaveEquation(new[] { equation });
				api.ShowEquationDistribution(new[] { identifier });
			}
		}

		// ========== Export Command Tests ==========

		[Fact]
		public void ExportEquations_ThrowsWithInvalidArgumentCount()
		{
			var (api, input, output) = GetNewApiInstance();

			Assert.True(CheckExclusiveArgumentCount_FilePathVersion(1, api.ExportEquations));
		}

		[Fact]
		public void ExportEquations_EmptyManager_ExportsEmptyFile()
		{
			var (api, input, output) = GetNewApiInstance();

			
			var tempFile = Path.GetTempFileName();
			api.ExportEquations(new[] { tempFile });

			var content = File.ReadAllText(tempFile);
			Assert.Equal("", content.Trim());
		}

		[Fact]
		public void ExportEquations_CreateNewFile()
		{
			var (api, input, output) = GetNewApiInstance();

			var tempFile = Path.Combine(Path.GetTempPath(), "dice_test_" + Guid.NewGuid() + ".txt");
			api.ExportEquations(new[] { tempFile });
			Assert.True(File.Exists(tempFile));
		}

		[Fact]
		public void ExportEquations_OverwriteFile()
		{
			var (api, input, output) = GetNewApiInstance();

			var tempFile = Path.Combine(Path.GetTempPath(), "dice_test_" + Guid.NewGuid() + ".txt");
			File.WriteAllText(tempFile, "Existing content");

			api.ExportEquations(new[] { tempFile });
			var content = File.ReadAllText(tempFile);
			Assert.DoesNotContain("Existing content", content);
		}

		[Fact]
		public void ExportEquations_SingleEquation()
		{
			var (api, input, output) = GetNewApiInstance();
			api.SaveEquation(new string[] { "test=1+1" });

			var tempFile = Path.Combine(Path.GetTempPath(), "dice_test_" + Guid.NewGuid() + ".txt");
			api.ExportEquations(new[] { tempFile });
			var content = File.ReadAllText(tempFile);
			Assert.Contains("test=1+1", content);
		}

		[Fact]
		public void ExportEquations_MultipleEquations()
		{
			var (api, input, output) = GetNewApiInstance();
			var mupltipleEquations = new string[] {
				"test=1+1",
				"test2=1+2",
				"test3=1+3",
				"test4=1+4",
				"test5=1+5",
				"test6=1+6",
			};
			foreach (var eq in mupltipleEquations)
			{
				api.SaveEquation(new[] { eq });
			}

			var tempFile = Path.Combine(Path.GetTempPath(), "dice_test_" + Guid.NewGuid() + ".txt");
			api.ExportEquations(new[] { tempFile });
			var content = File.ReadAllText(tempFile);
			Assert.Contains("test=1+1", content);
			Assert.Contains("test2=1+2", content);
			Assert.Contains("test3=1+3", content);
			Assert.Contains("test4=1+4", content);
			Assert.Contains("test5=1+5", content);
			Assert.Contains("test6=1+6", content);
		}

		// ========== Import Command Tests ==========

		[Fact]
		public void ImportEquations_ThrowsWhenFileNotExists()
		{
			var (api, input, output) = GetNewApiInstance();

			Assert.Throws<ArgumentException>(() => api.ImportEquations(new[] { "nonexistent_file.txt" }));
		}

		[Fact]
		public void ImportEquations_ThrowsWithInvalidArgumentCount()
		{
			var (api, input, output) = GetNewApiInstance();

			Assert.True(CheckExclusiveArgumentCount_FilePathVersion(1, api.ImportEquations));
		}

		[Fact]
		public void ImportEquations_ImportsEmptyFile()
		{
			var (api, input, output) = GetNewApiInstance();

			var tempFile = "dice_import_test_empty_" + Guid.NewGuid() + ".txt";
			File.WriteAllText(tempFile, "");
			api.ImportEquations(new[] { tempFile });
		}

		[Fact]
		public void ImportEquations_ImportSingleEquation()
		{
			var (api, input, output) = GetNewApiInstance();

			var tempFile = "dice_import_test_single_equation_" + Guid.NewGuid() + ".txt";
			File.WriteAllText(tempFile, "test=1+1");
			api.ImportEquations(new[] { tempFile });
			var result = output.ToString();

			// Only check for the existence of the equation name in the output
			Assert.Contains("test", result);
			// And the word "saved" to double check the name wasn't from a different message
			Assert.Contains("saved", result);
		}

		[Fact]
		public void ImportEquations_ImportMultipleEquations()
		{
			var (api, input, output) = GetNewApiInstance();

			var tempFile = "dice_import_test_single_equation_" + Guid.NewGuid() + ".txt";
			File.WriteAllText(tempFile, "test=1+1\ntest2=2+2\ntest3=3+3");
			api.ImportEquations(new[] { tempFile });
			var result = output.ToString();

			Assert.Contains("test", result);
			Assert.Contains("test2", result);
			Assert.Contains("test3", result);
			
			// Totally comprehensive output checking
			Assert.DoesNotContain("Warning", result);
			Assert.DoesNotContain("Error", result);
		}

		[Fact]
		public void ImportEquations_SkipExistingEquation()
		{
			var (api, input, output) = GetNewApiInstance();
			api.SaveEquation(new[] { "test=1+1" });

			var tempFile = "dice_import_test_single_equation_" + Guid.NewGuid() + ".txt";
			File.WriteAllText(tempFile, "test=1+1");

			api.ImportEquations(new[] { tempFile });
			var result = output.ToString();

			Assert.Contains("test", result);
			Assert.Contains("exists", result);
		}

		[Fact]
		public void ImportEquations_SkipExistingEquation_ImportMultiple()
		{
			var (api, input, output) = GetNewApiInstance();
			api.SaveEquation(new[] { "test=1+1" });

			var tempFile = "dice_import_test_single_equation_" + Guid.NewGuid() + ".txt";
			File.WriteAllText(tempFile, "test=1+1\ntest2=2+2\ntest3=3+3");

			api.ImportEquations(new[] { tempFile });
			var result = output.ToString();

			// Together with the last test, this should make sure that only "test" was skipped and the others were saved
			Assert.Contains("test", result);
			Assert.Contains("exists", result);
			Assert.Contains("test2", result);
			Assert.Contains("test3", result);
			Assert.Contains("saved", result);
		}

		[Fact]
		public void ImportEquations_SkipComments()
		{
			var (api, input, output) = GetNewApiInstance();

			var tempFile = "dice_import_test_comments" + Guid.NewGuid() + ".txt";
			File.WriteAllText(tempFile, "test=1+1\n# This is a comment\n# Another comment");
			api.ImportEquations(new[] { tempFile });
			var result = output.ToString();

			Assert.Contains("test", result);
			Assert.Contains("saved", result);
		}

		[Fact]
		public void ImportEquations_SkipEmptyLines()
		{
			var (api, input, output) = GetNewApiInstance();

			var tempFile = "dice_import_test_emptylines_" + Guid.NewGuid() + ".txt";
			File.WriteAllText(tempFile, "\n\ntest=1+1\n\n");
			api.ImportEquations(new[] { tempFile });
			var result = output.ToString();

			Assert.Contains("test", result);
			Assert.Contains("saved", result);
		}

		// ========== Run Method Tests ==========

		[Fact]
		public void Run_ExitsWithExitCommand()
		{
			var input = new StringReader("exit\n");
			var output = new StringWriter();
			var api = new DiceEquationAPI(input, output);

			// Should complete instantly
			api.Run();
		}

		[Fact]
		public void Run_UnknownCommand()
		{
			var input = new StringReader("unknown_command\nexit\n");
			var output = new StringWriter();
			var api = new DiceEquationAPI(input, output);

			api.Run();
			var result = output.ToString();

			Assert.Contains("Unknown command", result);
		}

		[Fact]
		public void Run_HelpCommand()
		{
			var input = new StringReader("help\nexit\n");
			var output = new StringWriter();
			var api = new DiceEquationAPI(input, output);

			api.Run();
			var result = output.ToString();

			Assert.Contains("Available commands", result);
		}

		[Fact]
		public void Run_MultipleHelpCommands()
		{
			var input = new StringReader("help\nhelp\nexit\n");
			var output = new StringWriter();
			var api = new DiceEquationAPI(input, output);

			api.Run();
			var result = output.ToString();

			// Should have two help outputs
			var helpCount = result.Split("Available commands").Length - 1;
			Assert.True(helpCount >= 2);
		}

		[Fact]
		public void Run_SaveCommand()
		{
			var input = new StringReader("save test=1+1\nexit\n");
			var output = new StringWriter();
			var api = new DiceEquationAPI(input, output);

			api.Run();
			var result = output.ToString();

			Assert.Contains("test", result);
			Assert.Contains("saved", result);
		}

		[Fact]
		public void Run_DeleteCommand()
		{
			var input = new StringReader("save test=1+1\ndel test\nexit\n");
			var output = new StringWriter();
			var api = new DiceEquationAPI(input, output);

			api.Run();
			var result = output.ToString();

			Assert.Contains("test", result);
			Assert.Contains("deleted", result);
		}

		[Fact]
		public void Run_EvalCommand()
		{
			var input = new StringReader("save test=1+1\neval test\nexit\n");
			var output = new StringWriter();
			var api = new DiceEquationAPI(input, output);

			api.Run();
			var result = output.ToString();

			Assert.Contains("2", result);
		}

		[Fact]
		public void Run_ShowCommand()
		{
			var input = new StringReader("save test=1+1\nshow test\nexit\n");
			var output = new StringWriter();
			var api = new DiceEquationAPI(input, output);

			api.Run();
			var result = output.ToString();

			Assert.Contains("1+1", result);
		}

		[Fact]
		public void Run_ProbCommand()
		{
			var input = new StringReader("save test=1+1\nexit\n");
			var output = new StringWriter();
			var api = new DiceEquationAPI(input, output);

			api.Run();
			var result = output.ToString();

			Assert.DoesNotContain("Error", result);
		}

		[Fact]
		public void Run_ExportCommand()
		{
			var tempFile = Path.Combine(Path.GetTempPath(), "dice_test_" + Guid.NewGuid() + ".txt");
			File.Create(tempFile).Close();

			var input = new StringReader("save test=1+1\nexport " + tempFile + "\nexit\n");
			var output = new StringWriter();
			var api = new DiceEquationAPI(input, output);

			api.Run();
			var result = output.ToString();

			Assert.Contains("Exported", result);
			Assert.Contains("test", result);
		}

		[Fact]
		public void Run_ImportCommand()
		{
			var tempFile = Path.Combine(Path.GetTempPath(), "dice_test_" + Guid.NewGuid() + ".txt");
			File.WriteAllText(tempFile, "test=1+1");

			var input = new StringReader("import " + tempFile + "\nexit\n");
			var output = new StringWriter();
			var api = new DiceEquationAPI(input, output);

			api.Run();
			var result = output.ToString();

			Assert.Contains("saved", result);
			Assert.Contains("test", result);
		}
	}
}
