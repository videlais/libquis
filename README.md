# LibQuis

A .NET library for parsing and evaluating expressions used in story filtering and conditional logic. This C# implementation provides functionality similar to the TypeScript [Quis library](https://github.com/videlais/quis), offering a powerful AST-based expression parser.

## Features

- **AST-based parsing**: Clean separation between tokenization, parsing, and evaluation
- **Rich expression support**: Variables, property access, arithmetic, comparisons, logical operations
- **Custom conditions**: Extensible system for domain-specific evaluation logic
- **Type coercion**: Automatic conversion between compatible types
- **Bracket notation**: Support for both dot notation (`$user.name`) and bracket notation (`$user["name"]`)
- **Story filtering**: Perfect for interactive fiction, game logic, and conditional content

## Installation

```bash
dotnet add package LibQuis
```

## Quick Start

```csharp
using LibQuis;

// Simple evaluation
var result = Quis.Parse("5 + 3 * 2"); // Returns 11

// Using variables
ValuesCallback values = name => name switch
{
    "health" => 75,
    "level" => 10,
    _ => null
};

var canProgress = Quis.Parse("$health > 50 && $level >= 10", values);
// Returns true
```

## Expression Syntax

### Variables

Variables are prefixed with `$` and resolved through a values callback:

```csharp
Quis.Parse("$player", name => name == "player" ? "Hero" : null)
```

### Property Access

Access object properties using dot notation or bracket notation:

```csharp
// Dot notation
Quis.Parse("$user.age >= 18", values)

// Bracket notation  
Quis.Parse("$user[\"status\"] == \"active\"", values)
```

### Arithmetic Operations

Standard arithmetic with proper operator precedence:

```csharp
Quis.Parse("$health + $bonus * 2") // Multiplication first
Quis.Parse("($health + $bonus) * 2") // Parentheses override precedence
```

### Comparison Operations

- `==`, `is` - Equality
- `!=`, `is not` - Inequality  
- `>`, `gt` - Greater than
- `<`, `lt` - Less than
- `>=`, `gte` - Greater than or equal
- `<=`, `lte` - Less than or equal

### Logical Operations

- `&&`, `and` - Logical AND
- `||`, `or` - Logical OR
- `!` - Logical NOT

### Custom Conditions

Extend the parser with domain-specific conditions:

```csharp
var customConditions = new Dictionary<string, CustomConditionEvaluator>
{
    { "contains", (text, search) => text?.ToString()?.Contains(search?.ToString() ?? "") == true },
    { "between", (value, range) => {
        if (double.TryParse(value?.ToString(), out var num))
        {
            var parts = range?.ToString()?.Split('-');
            if (parts?.Length == 2 && 
                double.TryParse(parts[0], out var min) && 
                double.TryParse(parts[1], out var max))
            {
                return num >= min && num <= max;
            }
        }
        return false;
    }}
};

var options = new ParseOptions 
{ 
    Values = values,
    CustomConditions = customConditions 
};

var result = Quis.Parse("$text custom:contains \"hello\"", options);
```

## Complete Example

```csharp
using LibQuis;

// Define your data
ValuesCallback values = variableName => variableName switch
{
    "player" => new { 
        name = "Hero", 
        level = 15, 
        health = 85,
        inventory = new { gold = 150, potions = 3 }
    },
    "quest" => new { 
        difficulty = "medium",
        requiredLevel = 10 
    },
    _ => null
};

// Define story content with conditions
var storySegments = new[]
{
    new { condition = "$player.level >= $quest.requiredLevel", text = "You are qualified for this quest!" },
    new { condition = "$player.health > 50", text = "You feel strong and ready." },
    new { condition = "$player.inventory.gold >= 100", text = "Your purse feels heavy with gold." },
    new { condition = "$player.level >= 20", text = "You are a veteran adventurer." },
    new { condition = "$quest.difficulty == \"easy\"", text = "This should be a simple task." }
};

// Filter content based on conditions
var availableContent = storySegments
    .Where(segment => 
    {
        var result = Quis.Parse(segment.condition, values);
        return result is bool isTrue && isTrue;
    })
    .Select(segment => segment.text)
    .ToArray();

foreach (var content in availableContent)
{
    Console.WriteLine(content);
}

// Output:
// You are qualified for this quest!
// You feel strong and ready.
// Your purse feels heavy with gold.
```

## Advanced Usage

### Parse Options

Control parsing behavior with `ParseOptions`:

```csharp
var options = new ParseOptions
{
    Values = myValuesCallback,
    CustomConditions = myCustomConditions
};

var result = Quis.Parse(expression, options);
```

### Error Handling

The library throws `QuitSyntaxException` for invalid syntax:

```csharp
try 
{
    var result = Quis.Parse("5 +"); // Missing right operand
}
catch (QuitSyntaxException ex)
{
    Console.WriteLine($"Parse error: {ex.Message}");
}
```

### Type Coercion

The evaluator automatically handles type conversions:

```csharp
// Numbers and strings are compared intelligently
Quis.Parse("$count == \"5\"", name => name == "count" ? 5 : null) // Returns true

// Boolean conversion follows JavaScript-like truthiness
Quis.Parse("$value && true", name => name == "value" ? 1 : null) // Returns true
```

## API Reference

### Quis Static Class

#### `Parse(string expression, ValuesCallback values)`

Parse and evaluate an expression with variable resolution.

#### `Parse(string expression, ParseOptions options)`

Parse and evaluate an expression with full options control.

### ParseOptions Class

Configuration object for parsing behavior:

- `Values`: Callback for resolving variable values
- `CustomConditions`: Dictionary of custom condition evaluators

### CustomConditionEvaluator Delegate

```csharp
public delegate bool CustomConditionEvaluator(object? left, object? right);
```

### ValuesCallback Delegate

```csharp
public delegate object? ValuesCallback(string variableName);
```

## Building from Source

```bash
git clone <repository-url>
cd libquis
dotnet build
dotnet test
```

## Contributing

Contributions are welcome! Please ensure all tests pass and follow the existing code style.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Inspiration

This library is inspired by the TypeScript [Quis library](https://github.com/videlais/quis) by Dan Cox, adapted for the .NET ecosystem with C# language features and conventions.
