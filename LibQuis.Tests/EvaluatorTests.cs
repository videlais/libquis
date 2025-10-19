using LibQuis;

namespace LibQuis.Tests;

public class EvaluatorTests
{
    [Fact]
    public void Evaluate_LiteralNode_ShouldReturnValue()
    {
        // Arrange
        var evaluator = new Evaluator();
        var literalNode = new LiteralNode(42);
        
        // Act
        var result = evaluator.Evaluate(literalNode);
        
        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Evaluate_StringLiteralNode_ShouldReturnString()
    {
        // Arrange
        var evaluator = new Evaluator();
        var literalNode = new LiteralNode("hello");
        
        // Act
        var result = evaluator.Evaluate(literalNode);
        
        // Assert
        Assert.Equal("hello", result);
    }

    [Fact]
    public void Evaluate_BooleanLiteralNode_ShouldReturnBoolean()
    {
        // Arrange
        var evaluator = new Evaluator();
        var literalNode = new LiteralNode(true);
        
        // Act
        var result = evaluator.Evaluate(literalNode);
        
        // Assert
        Assert.True((bool)result!);
    }

    [Fact]
    public void Evaluate_NullLiteralNode_ShouldReturnNull()
    {
        // Arrange
        var evaluator = new Evaluator();
        var literalNode = new LiteralNode(null);
        
        // Act
        var result = evaluator.Evaluate(literalNode);
        
        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Evaluate_VariableNode_ShouldCallValuesCallback()
    {
        // Arrange
        var options = new ParseOptions
        {
            Values = name => name == "example" ? 42 : null
        };
        var evaluator = new Evaluator(options);
        var variableNode = new VariableNode("example");
        
        // Act
        var result = evaluator.Evaluate(variableNode);
        
        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Evaluate_PropertyAccessNode_ShouldAccessProperty()
    {
        // Arrange
        var user = new { name = "John", age = 25 };
        var options = new ParseOptions
        {
            Values = name => name == "user" ? user : null
        };
        var evaluator = new Evaluator(options);
        var propertyNode = new PropertyAccessNode("user", "name", "dot");
        
        // Act
        var result = evaluator.Evaluate(propertyNode);
        
        // Assert
        Assert.Equal("John", result);
    }

    [Theory]
    [InlineData(5.0, 3.0, "+", 8.0)]
    [InlineData(5.0, 3.0, "-", 2.0)]
    [InlineData(5.0, 3.0, "*", 15.0)]
    [InlineData(6.0, 3.0, "/", 2.0)]
    public void Evaluate_ArithmeticOperations_ShouldReturnCorrectResult(double left, double right, string op, double expected)
    {
        // Arrange
        var evaluator = new Evaluator();
        var leftNode = new LiteralNode(left);
        var rightNode = new LiteralNode(right);
        var binaryNode = new BinaryOpNode(op, leftNode, rightNode);
        
        // Act
        var result = evaluator.Evaluate(binaryNode);
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(5.0, 3.0, "==", false)]
    [InlineData(5.0, 5.0, "==", true)]
    [InlineData(5.0, 3.0, "!=", true)]
    [InlineData(5.0, 5.0, "!=", false)]
    [InlineData(5.0, 3.0, ">", true)]
    [InlineData(3.0, 5.0, ">", false)]
    [InlineData(3.0, 5.0, "<", true)]
    [InlineData(5.0, 3.0, "<", false)]
    [InlineData(5.0, 5.0, ">=", true)]
    [InlineData(5.0, 3.0, ">=", true)]
    [InlineData(3.0, 5.0, ">=", false)]
    [InlineData(5.0, 5.0, "<=", true)]
    [InlineData(3.0, 5.0, "<=", true)]
    [InlineData(5.0, 3.0, "<=", false)]
    public void Evaluate_ComparisonOperations_ShouldReturnCorrectResult(double left, double right, string op, bool expected)
    {
        // Arrange
        var evaluator = new Evaluator();
        var leftNode = new LiteralNode(left);
        var rightNode = new LiteralNode(right);
        var binaryNode = new BinaryOpNode(op, leftNode, rightNode);
        
        // Act
        var result = evaluator.Evaluate(binaryNode);
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(true, true, "&&", true)]
    [InlineData(true, false, "&&", false)]
    [InlineData(false, true, "&&", false)]
    [InlineData(false, false, "&&", false)]
    [InlineData(true, true, "||", true)]
    [InlineData(true, false, "||", true)]
    [InlineData(false, true, "||", true)]
    [InlineData(false, false, "||", false)]
    public void Evaluate_LogicalOperations_ShouldReturnCorrectResult(bool left, bool right, string op, bool expected)
    {
        // Arrange
        var evaluator = new Evaluator();
        var leftNode = new LiteralNode(left);
        var rightNode = new LiteralNode(right);
        var binaryNode = new BinaryOpNode(op, leftNode, rightNode);
        
        // Act
        var result = evaluator.Evaluate(binaryNode);
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(true, "!", false)]
    [InlineData(false, "!", true)]
    [InlineData(true, "not", false)]
    [InlineData(false, "not", true)]
    public void Evaluate_UnaryOperations_ShouldReturnCorrectResult(bool operand, string op, bool expected)
    {
        // Arrange
        var evaluator = new Evaluator();
        var operandNode = new LiteralNode(operand);
        var unaryNode = new UnaryOpNode(op, operandNode);
        
        // Act
        var result = evaluator.Evaluate(unaryNode);
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Evaluate_CustomCondition_ShouldCallCustomEvaluator()
    {
        // Arrange
        var customConditions = new Dictionary<string, CustomConditionEvaluator>
        {
            { "contains", (text, search) => text?.ToString()?.Contains(search?.ToString() ?? "") == true }
        };
        var options = new ParseOptions { CustomConditions = customConditions };
        var evaluator = new Evaluator(options);
        
        var leftNode = new LiteralNode("Hello World");
        var rightNode = new LiteralNode("World");
        var customNode = new CustomConditionNode("contains", leftNode, rightNode);
        
        // Act
        var result = evaluator.Evaluate(customNode);
        
        // Assert
        Assert.True((bool)result!);
    }

    [Fact]
    public void Evaluate_UnknownCustomCondition_ShouldThrowException()
    {
        // Arrange
        var evaluator = new Evaluator();
        var leftNode = new LiteralNode("test");
        var rightNode = new LiteralNode("value");
        var customNode = new CustomConditionNode("unknown", leftNode, rightNode);
        
        // Act & Assert
        var ex = Assert.Throws<QuitSyntaxException>(() => evaluator.Evaluate(customNode));
        Assert.Contains("Unknown custom condition", ex.Message);
    }

    [Fact]
    public void Evaluate_ComplexArithmeticExpression_ShouldRespectPrecedence()
    {
        // Test: (2 + 3) * (4 - 1) = 5 * 3 = 15
        // Arrange
        var evaluator = new Evaluator();
        
        var innerLeft = new BinaryOpNode("+", new LiteralNode(2.0), new LiteralNode(3.0));
        var innerRight = new BinaryOpNode("-", new LiteralNode(4.0), new LiteralNode(1.0));
        var outerNode = new BinaryOpNode("*", innerLeft, innerRight);
        
        // Act
        var result = evaluator.Evaluate(outerNode);
        
        // Assert
        Assert.Equal(15.0, result);
    }

    [Fact]
    public void Evaluate_StringArithmetic_ShouldConvertToNumbers()
    {
        // Arrange
        var evaluator = new Evaluator();
        var leftNode = new LiteralNode("5");
        var rightNode = new LiteralNode("2");
        var binaryNode = new BinaryOpNode("*", leftNode, rightNode);
        
        // Act
        var result = evaluator.Evaluate(binaryNode);
        
        // Assert
        Assert.Equal(10.0, result);
    }

    [Fact]
    public void Evaluate_DivisionByZero_ShouldReturnInfinity()
    {
        // Arrange
        var evaluator = new Evaluator();
        var leftNode = new LiteralNode(5.0);
        var rightNode = new LiteralNode(0.0);
        var binaryNode = new BinaryOpNode("/", leftNode, rightNode);
        
        // Act
        var result = evaluator.Evaluate(binaryNode);
        
        // Assert
        Assert.Equal(double.PositiveInfinity, result);
    }

    [Fact]
    public void Evaluate_InvalidNumber_ShouldReturnNaN()
    {
        // Arrange
        var evaluator = new Evaluator();
        var leftNode = new LiteralNode("not-a-number");
        var rightNode = new LiteralNode(5.0);
        var binaryNode = new BinaryOpNode("*", leftNode, rightNode);
        
        // Act
        var result = evaluator.Evaluate(binaryNode);
        
        // Assert
        Assert.Equal(double.NaN, result);
    }

    [Fact]
    public void Evaluate_PropertyAccessOnNull_ShouldReturnNull()
    {
        // Arrange
        var options = new ParseOptions { Values = _ => null };
        var evaluator = new Evaluator(options);
        var propertyNode = new PropertyAccessNode("nonexistent", "property", "dot");
        
        // Act
        var result = evaluator.Evaluate(propertyNode);
        
        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Evaluate_VariableCallbackThrows_ShouldReturnNull()
    {
        // Arrange
        var options = new ParseOptions
        {
            Values = _ => throw new InvalidOperationException("Test exception")
        };
        var evaluator = new Evaluator(options);
        var variableNode = new VariableNode("test");
        
        // Act
        var result = evaluator.Evaluate(variableNode);
        
        // Assert
        Assert.Null(result);
    }
}