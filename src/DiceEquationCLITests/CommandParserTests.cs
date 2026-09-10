using System;
using System.Collections.Generic;
using System.Text;

namespace DiceEquationCLITests
{
	public class CommandParserTests
	{
		[Theory]
		[InlineData("command arg1", "command", new string[] { "arg1" })]
		[InlineData("command arg1 arg2 arg3arg3 arg4", "command", new string[] { "arg1", "arg2", "arg3arg3", "arg4" })]
		[InlineData("long_command_arg0_command arg1 arg2", "long_command_arg0_command", new string[] { "arg1", "arg2" })]
		public void ParseCommand_SplitCommandAndArguments(string input, string keyword, string[] arguments)
		{
			var command = DiceEquationCLI.CommandParser.Parse(input);
			Assert.Equal(keyword, command.Name);
			Assert.Equal(arguments, command.Args);
		}

		[Theory]
		[InlineData("command       arg1", "command", new string[] { "arg1" })]
		[InlineData("          command arg1", "command", new string[] { "arg1" })]
		[InlineData("command arg1          ", "command", new string[] { "arg1" })]
		[InlineData("command\targ1", "command", new string[] { "arg1" })]
		[InlineData("            command         arg1 \t\t\t    arg2 \targ3arg3\t\t arg4    ", "command", new string[] { "arg1", "arg2", "arg3arg3", "arg4" })]
		public void ParseCommand_IgnoreExcessiveWhitespace(string input, string keyword, string[] arguments)
		{
			var command = DiceEquationCLI.CommandParser.Parse(input);
			Assert.Equal(keyword, command.Name);
			Assert.Equal(arguments, command.Args);
		}
	}
}
