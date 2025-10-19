using BenchmarkDotNet.Attributes;
using LibQuis;

namespace LibQuis.Benchmarks;

/// <summary>
/// Performance benchmarks for the LibQuis expression parser
/// Run with: dotnet run -c Release
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class QuisBenchmarks
{
    private ValuesCallback _valuesCallback = null!;
    private Dictionary<string, CustomConditionEvaluator> _customConditions = null!;
    private readonly object _testUser = new { name = "John", age = 25, status = "active" };

    [GlobalSetup]
    public void Setup()
    {
        _valuesCallback = variableName => variableName switch
        {
            "user" => _testUser,
            "health" => 75,
            "level" => 10,
            "example" => 2,
            _ => null
        };

        _customConditions = new Dictionary<string, CustomConditionEvaluator>
        {
            { "contains", (text, search) => text?.ToString()?.Contains(search?.ToString() ?? "") == true }
        };
    }

    [Benchmark(Description = "Simple literal")]
    public object? ParseLiteral()
    {
        return Quis.Parse("42");
    }

    [Benchmark(Description = "Simple arithmetic")]
    public object? ParseArithmetic()
    {
        return Quis.Parse("5 + 3 * 2");
    }

    [Benchmark(Description = "Complex arithmetic")]
    public object? ParseComplexArithmetic()
    {
        return Quis.Parse("(2 + 3) * (4 - 1) + 10 / 2");
    }

    [Benchmark(Description = "Simple comparison")]
    public object? ParseComparison()
    {
        return Quis.Parse("5 > 3");
    }

    [Benchmark(Description = "Boolean logic")]
    public object? ParseBooleanLogic()
    {
        return Quis.Parse("true && false || true");
    }

    [Benchmark(Description = "Variable access")]
    public object? ParseVariable()
    {
        return Quis.Parse("$health > 50", _valuesCallback);
    }

    [Benchmark(Description = "Property access (dot)")]
    public object? ParsePropertyAccessDot()
    {
        return Quis.Parse("$user.age >= 18", _valuesCallback);
    }

    [Benchmark(Description = "Property access (bracket)")]
    public object? ParsePropertyAccessBracket()
    {
        return Quis.Parse("$user[\"status\"] == \"active\"", _valuesCallback);
    }

    [Benchmark(Description = "Complex expression")]
    public object? ParseComplexExpression()
    {
        return Quis.Parse("$user.age >= 18 && $user.status == \"active\" && $health > 50", _valuesCallback);
    }

    [Benchmark(Description = "Very complex expression")]
    public object? ParseVeryComplexExpression()
    {
        return Quis.Parse(
            "($user.age >= 18 && $user.age <= 65) && ($user.status == \"active\" || $user.status == \"premium\") && ($health > 50 || $level >= 10)",
            _valuesCallback);
    }

    [Benchmark(Description = "String literal")]
    public object? ParseStringLiteral()
    {
        return Quis.Parse("\"Hello World\"");
    }

    [Benchmark(Description = "String with escapes")]
    public object? ParseStringWithEscapes()
    {
        return Quis.Parse("\"Line 1\\nLine 2\\tTabbed\"");
    }

    [Benchmark(Description = "Custom condition")]
    public object? ParseCustomCondition()
    {
        var options = new ParseOptions
        {
            Values = name => name == "text" ? "Hello World" : null,
            CustomConditions = _customConditions
        };
        return Quis.Parse("$text custom:contains \"Hello\"", options);
    }

    [Benchmark(Description = "Nested parentheses")]
    public object? ParseNestedParentheses()
    {
        return Quis.Parse("((2 + 3) * (4 - 1)) / ((5 + 1) - 2)");
    }

    [Benchmark(Description = "Shorthand operators")]
    public object? ParseShorthandOperators()
    {
        return Quis.Parse("5 gt 3 and 10 lte 20 or 15 is not 20");
    }
}
