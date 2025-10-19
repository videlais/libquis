using LibQuis;

namespace LibQuis.Tests;

public class ParserTests
{
    [Theory]
    [InlineData("42", 42.0)]
    [InlineData("\"hello\"", "hello")]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("null", null)]
    public void Parse_LiteralValues_ShouldReturnCorrectNodes(string input, object? expectedValue)
    {
        // Arrange
        var tokenizer = new Tokenizer(input);
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        
        // Act
        var ast = parser.Parse();
        
        // Assert
        Assert.IsType<LiteralNode>(ast);
        var literalNode = (LiteralNode)ast;
        Assert.Equal(expectedValue, literalNode.Value);
    }

    [Fact]
    public void Parse_Variable_ShouldReturnVariableNode()
    {
        // Arrange
        var tokenizer = new Tokenizer("$user");
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        
        // Act
        var ast = parser.Parse();
        
        // Assert
        Assert.IsType<VariableNode>(ast);
        var variableNode = (VariableNode)ast;
        Assert.Equal("user", variableNode.Name);
    }

    [Fact]
    public void Parse_PropertyAccessDotNotation_ShouldReturnPropertyAccessNode()
    {
        // Arrange
        var tokenizer = new Tokenizer("$user.name");
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        
        // Act
        var ast = parser.Parse();
        
        // Assert
        Assert.IsType<PropertyAccessNode>(ast);
        var propertyNode = (PropertyAccessNode)ast;
        Assert.Equal("user", propertyNode.Object);
        Assert.Equal("name", propertyNode.Property);
        Assert.Equal("dot", propertyNode.Notation);
    }

    [Fact]
    public void Parse_PropertyAccessBracketNotation_ShouldReturnPropertyAccessNode()
    {
        // Arrange
        var tokenizer = new Tokenizer("$user[\"name\"]");
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        
        // Act
        var ast = parser.Parse();
        
        // Assert
        Assert.IsType<PropertyAccessNode>(ast);
        var propertyNode = (PropertyAccessNode)ast;
        Assert.Equal("user", propertyNode.Object);
        Assert.Equal("name", propertyNode.Property);
        Assert.Equal("bracket", propertyNode.Notation);
    }

    [Theory]
    [InlineData("+")]
    [InlineData("-")]
    [InlineData("*")]
    [InlineData("/")]
    public void Parse_ArithmeticOperations_ShouldReturnBinaryOpNode(string op)
    {
        // Arrange
        var tokenizer = new Tokenizer($"5 {op} 3");
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        
        // Act
        var ast = parser.Parse();
        
        // Assert
        Assert.IsType<BinaryOpNode>(ast);
        var binaryNode = (BinaryOpNode)ast;
        Assert.Equal(op, binaryNode.Operator);
        Assert.IsType<LiteralNode>(binaryNode.Left);
        Assert.IsType<LiteralNode>(binaryNode.Right);
    }

    [Theory]
    [InlineData("==")]
    [InlineData("!=")]
    [InlineData(">")]
    [InlineData("<")]
    [InlineData(">=")]
    [InlineData("<=")]
    [InlineData("is")]
    [InlineData("gt")]
    [InlineData("gte")]
    [InlineData("lt")]
    [InlineData("lte")]
    public void Parse_ComparisonOperations_ShouldReturnBinaryOpNode(string op)
    {
        // Arrange
        var tokenizer = new Tokenizer($"5 {op} 3");
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        
        // Act
        var ast = parser.Parse();
        
        // Assert
        Assert.IsType<BinaryOpNode>(ast);
        var binaryNode = (BinaryOpNode)ast;
        Assert.Equal(op, binaryNode.Operator);
    }

    [Theory]
    [InlineData("&&")]
    [InlineData("||")]
    [InlineData("and")]
    [InlineData("or")]
    public void Parse_LogicalOperations_ShouldReturnBinaryOpNode(string op)
    {
        // Arrange
        var tokenizer = new Tokenizer($"true {op} false");
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        
        // Act
        var ast = parser.Parse();
        
        // Assert
        Assert.IsType<BinaryOpNode>(ast);
        var binaryNode = (BinaryOpNode)ast;
        Assert.Equal(op, binaryNode.Operator);
    }

    [Theory]
    [InlineData("not")]
    [InlineData("!")]
    public void Parse_UnaryNotOperations_ShouldReturnUnaryOpNode(string op)
    {
        // Arrange
        var tokenizer = new Tokenizer($"{op} true");
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        
        // Act
        var ast = parser.Parse();
        
        // Assert
        Assert.IsType<UnaryOpNode>(ast);
        var unaryNode = (UnaryOpNode)ast;
        Assert.Equal(op, unaryNode.Operator);
        Assert.IsType<LiteralNode>(unaryNode.Operand);
    }

    [Fact]
    public void Parse_ParenthesizedExpression_ShouldRespectGrouping()
    {
        // Arrange
        var tokenizer = new Tokenizer("(true)");
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        
        // Act
        var ast = parser.Parse();
        
        // Assert
        Assert.IsType<LiteralNode>(ast);
        var literalNode = (LiteralNode)ast;
        Assert.True((bool)literalNode.Value!);
    }

    [Fact]
    public void Parse_OperatorPrecedence_ShouldRespectArithmeticPrecedence()
    {
        // Test: 2 + 3 * 4 should be 2 + (3 * 4)
        // Arrange
        var tokenizer = new Tokenizer("2 + 3 * 4");
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        
        // Act
        var ast = parser.Parse();
        
        // Assert
        Assert.IsType<BinaryOpNode>(ast);
        var rootNode = (BinaryOpNode)ast;
        Assert.Equal("+", rootNode.Operator);
        
        // Left should be literal 2
        Assert.IsType<LiteralNode>(rootNode.Left);
        var leftLiteral = (LiteralNode)rootNode.Left;
        Assert.Equal(2.0, leftLiteral.Value);
        
        // Right should be multiplication node
        Assert.IsType<BinaryOpNode>(rootNode.Right);
        var rightBinary = (BinaryOpNode)rootNode.Right;
        Assert.Equal("*", rightBinary.Operator);
    }

    [Fact]
    public void Parse_OperatorPrecedence_ShouldRespectLogicalPrecedence()
    {
        // Arrange
        var tokenizer = new Tokenizer("true || false && false");
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        
        // Act
        var ast = parser.Parse();
        
        // Assert
        Assert.IsType<BinaryOpNode>(ast);
        var rootNode = (BinaryOpNode)ast;
        Assert.Equal("||", rootNode.Operator);
        
        // Right should be AND node
        Assert.IsType<BinaryOpNode>(rootNode.Right);
        var rightBinary = (BinaryOpNode)rootNode.Right;
        Assert.Equal("&&", rightBinary.Operator);
    }

    [Fact]
    public void Parse_ParenthesesChangePrecedence_ShouldRespectGrouping()
    {
        // Test: (2 + 3) * 4 should be (2 + 3) * 4, not 2 + (3 * 4)
        // Arrange
        var tokenizer = new Tokenizer("(2 + 3) * 4");
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        
        // Act
        var ast = parser.Parse();
        
        // Assert
        Assert.IsType<BinaryOpNode>(ast);
        var rootNode = (BinaryOpNode)ast;
        Assert.Equal("*", rootNode.Operator);
        
        // Left should be addition node
        Assert.IsType<BinaryOpNode>(rootNode.Left);
        var leftBinary = (BinaryOpNode)rootNode.Left;
        Assert.Equal("+", leftBinary.Operator);
        
        // Right should be literal 4
        Assert.IsType<LiteralNode>(rootNode.Right);
        var rightLiteral = (LiteralNode)rootNode.Right;
        Assert.Equal(4.0, rightLiteral.Value);
    }

    [Fact]
    public void Parse_CustomCondition_ShouldReturnCustomConditionNode()
    {
        // Arrange
        var tokenizer = new Tokenizer("$user.age custom:between 18");
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        
        // Act
        var ast = parser.Parse();
        
        // Assert
        Assert.IsType<CustomConditionNode>(ast);
        var customNode = (CustomConditionNode)ast;
        Assert.Equal("between", customNode.Name);
        Assert.IsType<PropertyAccessNode>(customNode.Left);
        Assert.IsType<LiteralNode>(customNode.Right);
    }

    [Fact]
    public void Parse_ComplexExpression_ShouldBuildCorrectAST()
    {
        // Arrange
        var tokenizer = new Tokenizer("$user.age >= 18 && $user.active == true");
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        
        // Act
        var ast = parser.Parse();
        
        // Assert
        Assert.IsType<BinaryOpNode>(ast);
        var rootNode = (BinaryOpNode)ast;
        Assert.Equal("&&", rootNode.Operator);
        
        // Left side should be age >= 18
        Assert.IsType<BinaryOpNode>(rootNode.Left);
        var leftComparison = (BinaryOpNode)rootNode.Left;
        Assert.Equal(">=", leftComparison.Operator);
        
        // Right side should be active == true
        Assert.IsType<BinaryOpNode>(rootNode.Right);
        var rightComparison = (BinaryOpNode)rootNode.Right;
        Assert.Equal("==", rightComparison.Operator);
    }

    [Fact]
    public void Parse_UnexpectedToken_ShouldThrowException()
    {
        // Arrange
        var tokenizer = new Tokenizer("true false");
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        
        // Act & Assert
        var ex = Assert.Throws<QuitSyntaxException>(() => parser.Parse());
        Assert.Contains("Unexpected token", ex.Message);
    }

    [Fact]
    public void Parse_InvalidExpression_ShouldThrowException()
    {
        // Arrange
        var tokenizer = new Tokenizer("5 +");
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        
        // Act & Assert
        Assert.Throws<QuitSyntaxException>(() => parser.Parse());
    }
}