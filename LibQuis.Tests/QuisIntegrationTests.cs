using LibQuis;

namespace LibQuis.Tests;

public class QuisIntegrationTests
{
    [Fact]
    public void Parse_SimpleBoolean_ShouldReturnCorrectResult()
    {
        // Arrange & Act
        var result = Quis.Parse("true");
        
        // Assert
        Assert.True((bool)result!);
    }

    [Fact]
    public void Parse_SimpleBooleanExpression_ShouldReturnCorrectResult()
    {
        // Arrange & Act
        var result1 = Quis.Parse("true && false");
        var result2 = Quis.Parse("true || false");
        
        // Assert
        Assert.False((bool)result1!);
        Assert.True((bool)result2!);
    }

    [Fact]
    public void Parse_ComparisonExpression_ShouldReturnCorrectResult()
    {
        // Arrange & Act
        var result1 = Quis.Parse("5 > 3");
        var result2 = Quis.Parse("5 < 3");
        var result3 = Quis.Parse("5 >= 5");
        var result4 = Quis.Parse("5 <= 4");
        var result5 = Quis.Parse("5 == 5");
        var result6 = Quis.Parse("5 != 3");
        
        // Assert
        Assert.True((bool)result1!);
        Assert.False((bool)result2!);
        Assert.True((bool)result3!);
        Assert.False((bool)result4!);
        Assert.True((bool)result5!);
        Assert.True((bool)result6!);
    }

    [Fact]
    public void Parse_ShorthandOperators_ShouldReturnCorrectResult()
    {
        // Arrange & Act
        var result1 = Quis.Parse("5 gt 3");
        var result2 = Quis.Parse("5 lt 3");
        var result3 = Quis.Parse("5 gte 5");
        var result4 = Quis.Parse("5 lte 4");
        var result5 = Quis.Parse("5 is 5");
        var result6 = Quis.Parse("5 is not 3");
        
        // Assert
        Assert.True((bool)result1!);
        Assert.False((bool)result2!);
        Assert.True((bool)result3!);
        Assert.False((bool)result4!);
        Assert.True((bool)result5!);
        Assert.True((bool)result6!);
    }

    [Fact]
    public void Parse_ArithmeticExpression_ShouldReturnCorrectResult()
    {
        // Arrange & Act
        var result1 = Quis.Parse("2 + 3");
        var result2 = Quis.Parse("5 - 2");
        var result3 = Quis.Parse("4 * 3");
        var result4 = Quis.Parse("10 / 2");
        
        // Assert
        Assert.Equal(5.0, result1);
        Assert.Equal(3.0, result2);
        Assert.Equal(12.0, result3);
        Assert.Equal(5.0, result4);
    }

    [Fact]
    public void Parse_WithValuesCallback_ShouldResolveVariables()
    {
        // Arrange
        ValuesCallback values = (string name) => name switch
        {
            "example" => 2,
            "test" => 5,
            _ => null
        };
        
        // Act
        var result1 = Quis.Parse("$example > 1", values);
        var result2 = Quis.Parse("$test == 5", values);
        var result3 = Quis.Parse("$example + $test", values);
        
        // Assert
        Assert.True((bool)result1!);
        Assert.True((bool)result2!);
        Assert.Equal(7.0, result3);
    }

    [Fact]
    public void Parse_PropertyAccess_ShouldAccessObjectProperties()
    {
        // Arrange
        var user = new { name = "John", age = 25, status = "active" };
        ValuesCallback values = (string name) => name == "user" ? user : null;
        
        // Act
        var result1 = Quis.Parse("$user.age >= 18", values);
        var result2 = Quis.Parse("$user.name == \"John\"", values);
        var result3 = Quis.Parse("$user[\"status\"] == \"active\"", values);
        
        // Assert
        Assert.True((bool)result1!);
        Assert.True((bool)result2!);
        Assert.True((bool)result3!);
    }

    [Fact]
    public void Parse_ComplexExpression_ShouldEvaluateCorrectly()
    {
        // Arrange
        var user = new { age = 25, active = true, role = "admin" };
        ValuesCallback values = (string name) => name == "user" ? user : null;
        
        // Act
        var result = Quis.Parse("$user.age >= 18 && $user.active == true && $user.role == \"admin\"", values);
        
        // Assert
        Assert.True((bool)result!);
    }

    [Fact]
    public void Parse_WithCustomConditions_ShouldUseCustomEvaluators()
    {
        // Arrange
        var customConditions = new Dictionary<string, CustomConditionEvaluator>
        {
            { "contains", (text, search) => text?.ToString()?.Contains(search?.ToString() ?? "") == true },
            { "between", (value, range) => {
                double num;
                if (value is double d) 
                    num = d;
                else if (value is int i) 
                    num = i;
                else if (double.TryParse(value?.ToString(), out var parsed)) 
                    num = parsed;
                else 
                    return false;
                    
                var parts = range?.ToString()?.Split('-');
                if (parts?.Length != 2) return false;
                if (!double.TryParse(parts[0], out var min) || !double.TryParse(parts[1], out var max)) return false;
                return num >= min && num <= max;
            }}
        };
        
        ValuesCallback values = (string name) => name switch
        {
            "text" => "Hello World",
            "age" => 25,
            _ => null
        };
        
        var options = new ParseOptions 
        { 
            Values = values,
            CustomConditions = customConditions 
        };
        
        // Act
        var result1 = Quis.Parse("$text custom:contains \"Hello\"", options);
        var result2 = Quis.Parse("$age custom:between \"18-30\"", options);
        var result3 = Quis.Parse("$text custom:contains \"Goodbye\"", options);
        
        // Assert
        Assert.True((bool)result1!);
        Assert.True((bool)result2!);
        Assert.False((bool)result3!);
    }

    [Fact]
    public void Parse_OperatorPrecedence_ShouldFollowCorrectOrder()
    {
        // Arrange & Act
        var result1 = Quis.Parse("2 + 3 * 4"); // Should be 2 + (3 * 4) = 14
        var result2 = Quis.Parse("(2 + 3) * 4"); // Should be (2 + 3) * 4 = 20
        var result3 = Quis.Parse("true || false && false"); // Should be true || (false && false) = true
        
        // Assert
        Assert.Equal(14.0, result1);
        Assert.Equal(20.0, result2);
        Assert.True((bool)result3!);
    }

    [Fact]
    public void Parse_NestedParentheses_ShouldRespectGrouping()
    {
        // Arrange & Act
        var result1 = Quis.Parse("(((true)))");
        var result2 = Quis.Parse("((2 + 3) * (4 - 1))"); // (2 + 3) * (4 - 1) = 5 * 3 = 15
        
        // Assert
        Assert.True((bool)result1!);
        Assert.Equal(15.0, result2);
    }

    [Fact]
    public void Parse_WordOperators_ShouldWorkLikeSymbolicOperators()
    {
        // Arrange & Act
        var result1 = Quis.Parse("true and false");
        var result2 = Quis.Parse("true or false");
        var result3 = Quis.Parse("not true");
        
        // Assert
        Assert.False((bool)result1!);
        Assert.True((bool)result2!);
        Assert.False((bool)result3!);
    }

    [Fact]
    public void Parse_InvalidSyntax_ShouldThrowException()
    {
        // Arrange & Act & Assert
        Assert.Throws<QuitSyntaxException>(() => Quis.Parse("5 +"));
        Assert.Throws<QuitSyntaxException>(() => Quis.Parse("5 > "));
        Assert.Throws<QuitSyntaxException>(() => Quis.Parse("true false"));
    }

    [Fact]
    public void Parse_FilteringExample_ShouldWorkLikeTypeScriptVersion()
    {
        // This mimics the example from the TypeScript README
        // Arrange
        ValuesCallback values = (string name) => name switch
        {
            "example" => 2,
            "user" => new { 
                name = "John", 
                age = 25, 
                status = "active",
                health = 50,
                role = "premium"
            },
            "inventory" => new { potion = true },
            _ => null
        };

        var content = new[]
        {
            new { condition = "$example > 3", text = "A" },
            new { condition = "$example == 2", text = "B" },
            new { condition = "$user.age >= 18", text = "C" },
            new { condition = "$user[\"status\"] == \"active\"", text = "D" },
            new { condition = "$user.age >= 21 && $user.role == \"premium\"", text = "E" },
            new { condition = "$user.health < 20 || $inventory.potion == true", text = "F" }
        };

        // Act
        var results = content.Where(entry => 
        {
            var parseResult = Quis.Parse(entry.condition, values);
            var isTrue = parseResult is bool boolResult && boolResult;
            return isTrue;
        }).ToArray();

        // Assert
        Assert.Equal(5, results.Length);
        Assert.Contains(results, r => r.text == "B"); // $example == 2
        Assert.Contains(results, r => r.text == "C"); // $user.age >= 18  
        Assert.Contains(results, r => r.text == "D"); // $user.status == "active"
        Assert.Contains(results, r => r.text == "E"); // $user.age >= 21 && $user.role == "premium"
        Assert.Contains(results, r => r.text == "F"); // $inventory.potion == true
    }

    [Fact]
    public void Parse_EmptyInput_ShouldThrowException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => Quis.Parse(""));
    }

    [Fact]
    public void Parse_WhitespaceOnly_ShouldThrowException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => Quis.Parse("   "));
    }

    [Fact]
    public void Parse_AlternativeAPIOverloads_ShouldWork()
    {
        // Arrange
        ValuesCallback values = (string name) => name == "test" ? 42 : null;
        var customConditions = new Dictionary<string, CustomConditionEvaluator>
        {
            { "always", (_, _) => true }
        };

        // Act
        var result1 = Quis.Parse("$test == 42", values);
        var result2 = Quis.Parse("$test custom:always 0", values, customConditions);

        // Assert
        Assert.True((bool)result1!);
        Assert.True((bool)result2!);
    }
}