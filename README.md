# Dice Equation CLI User Guide

## Table of Contents
- [Dice Equation CLI User Guide](#dice-equation-cli-user-guide)
	- [Table of Contents](#table-of-contents)
	- [Introduction](#introduction)
	- [Commands](#commands)
	- [Identifiers](#identifiers)
	- [Dice Equation Syntax](#dice-equation-syntax)
		- [Explaining the Dice Roll Operator](#explaining-the-dice-roll-operator)

## Introduction

The Dice Equation CLI is a command-line interface program that allows users to evaluate and save so-called "dice equations".
A dice equation is a mathematical expression, which not only supports standard arithmetic operations, but also implements a "dice roll" operator, which allows users to simulate rolling dice in their equations.

The application defines a syntax for dice equations and provides a set of commands for equation management (saving, importing, exporting), evaluation and even listing all possible outcomes of a given dice equation, including their probabilities.

The application is written in C# and requires .NET 10.0 SDK.
It can be run by building and running the `DiceEquationCLI` project in the folder `src/DiceEquationCLI` either with an IDE or by using the `dotnet` command line tool:
```bash
dotnet run --project src/DiceEquationCLI
```
The project relies on two included libraries, the probabilistic number library and the dice number library, which are both included in the solution and are built automatically when building the CLI project.

## Commands

The Dice Equation CLI provides the following commands:
- `help`: Displays a list of available commands and their descriptions.
- `exit`: Exits the application.
- `save <identifier> = <dice_equation>`: Saves a dice equation with the given identifier
	- All whitespace is ignored in both the identifier and the dice equation
	- See [Dice Equation Syntax](#dice-equation-syntax) for more information on the syntax of dice equations.
	- If you want saved equations to persist between application runs, you can use the `export` command to save them to a file and the `import` command to load them back in.
- `del all | <identifier>`: Deletes all saved equations or the equation with the given identifier.
	- When deleting all equations, the user will be prompted for confirmation.
- `show all | <identifier>`: Displays all saved equations or the equation with the given identifier.
- `eval all | <identifier>`: Evaluates all saved equations or the equation with the given identifier and displays the result.
- `prob <identifier>`: Displays all possible outcomes of the equation with the given identifier, along with their probabilities.
	- This command may take a while to complete for equations with 2000+ possible outcomes.
	- But after the first call, the results are cached until the application is closed.
- `export <filename>`: Exports all saved equations to a file with the given filename.
	- If the file already exists, a confirmation prompt will be displayed before overwriting the file.
	- Equations will be saved in the format `<identifier>=<dice_equation>`, one equation per line, so you can consider adding your own equations manually.
- `import <filename>`: Imports saved equations from a file with the given filename.
	- This command will **not** overwrite any existing equations with the same identifier, but will instead skip them and display a warning message for each skipped equation.
	- All empty lines and lines starting with `#` in the file will be ignored, allowing for comments and better organization of the file.

Here is an example usage of the Dice Equation CLI:
```bash
Welcome to the Dice Equation CLI!
Type 'help' for a list of available commands.
> save a = 5 * 5 + 2d5
Equation 'a' saved.
> eval a
32
> eval a
33
> save b = 2d(6 / 2) + 2
Equation 'b' saved.
> show all
a: 5*5+2d5
b: 2d(6/2)+2
> prob b
4   : 11.11%
5   : 22.22%
6   : 33.33%
7   : 22.22%
8   : 11.11%
> export example.txt
Exported 'a', 'b', to example.txt
> del a
Equation 'a' deleted.
> import example.txt
Equation 'a' saved.
Warning: 'b' already exists. Skipping
> del all
Are you sure you want to delete all equations? (y/n): yes
All equations deleted.
```


## Identifiers

An identifier is a string that is used to uniquely identify a saved dice equation.
Identifiers can contain letters, numbers, and underscores, but must start with a letter.
Identifiers are case-sensitive, meaning that "Equation1" and "equation1" would be considered different identifiers.

When saving equations, all whitespace in the identifier is ignored, so "Equation 1" and "Equation1" would be considered the same identifier.
Here is another example:
```
> save a b c = 2 + 2
Equation 'abc' saved.
> eval abc
4
```

## Dice Equation Syntax

An equation as a whole consists of an identifier, an equals sign, and a expression.
The identifier is explained in the previous section, and here we will explain the syntax of the expression itself.

A dice equation expression can contain the following elements:
- Numbers: Any integer or decimal number, e.g. `1`, `42`, `-5`, `3.14`, `-0.5`
- Standard binary operators: `+`, `-`, `*`, `/`, for addition, subtraction, multiplication, division, respectively.
- Parentheses: `(`, `)`, for grouping expressions and controlling the order of operations.
- Unary minus operator: `-`, for negating a number or expression, e.g. `-5`, `-(2 + 3)`.
- Dice roll operator: `d`, for rolling dice, e.g. `2d6` means rolling two six-sided dice and summing the results.
	- Both the left and right side of the `d` operator must be a non-negative integer, representing the number of dice to roll and the number of sides on each die, respectively.

In terms of priority, the order of operations is as follows:
1. Parentheses
2. Unary minus operator
3. Dice roll operator
4. Multiplication and division
5. Addition and subtraction

Here are some examples of valid dice equations:
```
((2 + 3) * 4) / (2 + 2) + 5
3d2 * 100 - 3d2 = (3d2) * 100 - (3d2)
2d3 * 2d3
(2d2)d(2d3)
```

`-(5d6)` is valid, but `-5d6` is not, there is no way to evaluate `-5` dice rolls.

### Explaining the Dice Roll Operator

As mentioned before, both the left and right side of the `d` operator must be a non-negative integer.
You can also use a dice equation as the right side of the `d` operator, e.g. `2d(6 / 2)` or even `2d(2d4 / 2)`.
However, because `(2d4 / 2)` can evaluate to a non-integer, there is a chance that when evaluating `2d(2d4 / 2)`, you will get an error, instead of a result.
You can see the probability of the error with the `prob` command.

Here is an example:
```bash
> save a = 2d(2d4 / 2)
Equation 'a' saved.
> eval a
2
> eval a
3
> eval a
4
> eval a
Invalid dice sides: Must be a positive integer.
> prob a
error: 50.00%
2    : 13.41%
3    : 14.32%
4    : 12.11%
5    : 5.73%
6    : 3.26%
7    : 0.78%
8    : 0.39%
```

You can chain multiple dice roll operators together, e.g. `2d3d4` is valid and will be evaluated as `(2d3)d4`.
However be warned, the number of possible outcomes grows exponentially and the `prob` command takes a long time to complete for equations with 2000+ possible outcomes.
