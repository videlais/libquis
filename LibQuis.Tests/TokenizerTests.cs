using LibQuis;

namespace LibQuis.Tests;

public class TokenizerTests
{
    [Fact]
    public void Tokenize_Number_ShouldReturnNumberToken()
    {
        // Arrange
        var tokenizer = new Tokenizer("42");
        
        // Act
        var tokens = tokenizer.Tokenize();
        
        // Assert
        Assert.Equal(2, tokens.Count); // NUMBER + EOF
        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal("42", tokens[0].Value);
        Assert.Equal(0, tokens[0].Position);
    }

    [Fact]
    public void Tokenize_DecimalNumber_ShouldReturnNumberToken()
    {
        // Arrange
        var tokenizer = new Tokenizer("3.14");
        
        // Act
        var tokens = tokenizer.Tokenize();
        
        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal("3.14", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_NegativeNumber_ShouldReturnNumberToken()
    {
        // Arrange
        var tokenizer = new Tokenizer("-15");
        
        // Act
        var tokens = tokenizer.Tokenize();
        
        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal("-15", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_String_ShouldReturnStringToken()
    {
        // Arrange
        var tokenizer = new Tokenizer("\"hello\"");
        
        // Act
        var tokens = tokenizer.Tokenize();
        
        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal("hello", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_SingleQuoteString_ShouldReturnStringToken()
    {
        // Arrange
        var tokenizer = new Tokenizer("'world'");
        
        // Act
        var tokens = tokenizer.Tokenize();
        
        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal("world", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_BooleanTrue_ShouldReturnBooleanToken()
    {
        // Arrange
        var tokenizer = new Tokenizer("true");
        
        // Act
        var tokens = tokenizer.Tokenize();
        
        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Boolean, tokens[0].Type);
        Assert.Equal("true", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_BooleanFalse_ShouldReturnBooleanToken()
    {
        // Arrange
        var tokenizer = new Tokenizer("false");
        
        // Act
        var tokens = tokenizer.Tokenize();
        
        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Boolean, tokens[0].Type);
        Assert.Equal("false", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_Null_ShouldReturnNullToken()
    {
        // Arrange
        var tokenizer = new Tokenizer("null");
        
        // Act
        var tokens = tokenizer.Tokenize();
        
        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Null, tokens[0].Type);
        Assert.Equal("null", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_Variable_ShouldReturnVariableToken()
    {
        // Arrange
        var tokenizer = new Tokenizer("$user");
        
        // Act
        var tokens = tokenizer.Tokenize();
        
        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Variable, tokens[0].Type);
        Assert.Equal("user", tokens[0].Value);
    }

    [Theory]
    [InlineData("==", TokenType.Equals)]
    [InlineData("!=", TokenType.NotEquals)]
    [InlineData(">=", TokenType.GreaterThanEqual)]
    [InlineData("<=", TokenType.LessThanEqual)]
    [InlineData("&&", TokenType.And)]
    [InlineData("||", TokenType.Or)]
    public void Tokenize_TwoCharOperators_ShouldReturnCorrectTokens(string input, TokenType expectedType)
    {
        // Arrange
        var tokenizer = new Tokenizer(input);
        
        // Act
        var tokens = tokenizer.Tokenize();
        
        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(input, tokens[0].Value);
    }

    [Theory]
    [InlineData(">", TokenType.GreaterThan)]
    [InlineData("<", TokenType.LessThan)]
    [InlineData("(", TokenType.LeftParen)]
    [InlineData(")", TokenType.RightParen)]
    [InlineData(".", TokenType.Dot)]
    [InlineData("[", TokenType.LeftBracket)]
    [InlineData("]", TokenType.RightBracket)]
    [InlineData(":", TokenType.Colon)]
    [InlineData("!", TokenType.Not)]
    [InlineData("+", TokenType.Plus)]
    [InlineData("-", TokenType.Minus)]
    [InlineData("*", TokenType.Multiply)]
    [InlineData("/", TokenType.Divide)]
    public void Tokenize_SingleCharOperators_ShouldReturnCorrectTokens(string input, TokenType expectedType)
    {
        // Arrange
        var tokenizer = new Tokenizer(input);
        
        // Act
        var tokens = tokenizer.Tokenize();
        
        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(input, tokens[0].Value);
        Assert.Equal(1, tokens[0].Value.Length); // Single character operators should have length 1
    }

    [Theory]
    [InlineData("and", TokenType.And)]
    [InlineData("or", TokenType.Or)]
    [InlineData("not", TokenType.Not)]
    [InlineData("is", TokenType.Is)]
    [InlineData("gt", TokenType.Gt)]
    [InlineData("gte", TokenType.Gte)]
    [InlineData("lt", TokenType.Lt)]
    [InlineData("lte", TokenType.Lte)]
    [InlineData("custom", TokenType.Custom)]
    public void Tokenize_Keywords_ShouldReturnCorrectTokens(string input, TokenType expectedType)
    {
        // Arrange
        var tokenizer = new Tokenizer(input);
        
        // Act
        var tokens = tokenizer.Tokenize();
        
        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(input, tokens[0].Value);
        Assert.True(input.Length > 1, "Keywords should be multi-character tokens");
    }

    [Fact]
    public void Tokenize_ComplexExpression_ShouldReturnCorrectTokens()
    {
        // Arrange
        var tokenizer = new Tokenizer("$user.age >= 18 && $user.active == true");
        
        // Act
        var tokens = tokenizer.Tokenize();
        
        // Assert
        Assert.Equal(12, tokens.Count); // 11 tokens + EOF
        Assert.Equal(TokenType.Variable, tokens[0].Type);
        Assert.Equal("user", tokens[0].Value);
        Assert.Equal(TokenType.Dot, tokens[1].Type);
        Assert.Equal(TokenType.Identifier, tokens[2].Type);
        Assert.Equal("age", tokens[2].Value);
        Assert.Equal(TokenType.GreaterThanEqual, tokens[3].Type);
        Assert.Equal(TokenType.Number, tokens[4].Type);
        Assert.Equal("18", tokens[4].Value);
        Assert.Equal(TokenType.And, tokens[5].Type);
        Assert.Equal(TokenType.Variable, tokens[6].Type);
        Assert.Equal("user", tokens[6].Value);
        Assert.Equal(TokenType.Dot, tokens[7].Type);
        Assert.Equal(TokenType.Identifier, tokens[8].Type);
        Assert.Equal("active", tokens[8].Value);
        Assert.Equal(TokenType.Equals, tokens[9].Type);
        Assert.Equal(TokenType.Boolean, tokens[10].Type);
        Assert.Equal("true", tokens[10].Value);
        Assert.Equal(TokenType.EOF, tokens[11].Type);
    }

    [Fact]
    public void Tokenize_PropertyAccessWithBrackets_ShouldReturnCorrectTokens()
    {
        // Arrange
        var tokenizer = new Tokenizer("$user[\"name\"]");
        
        // Act
        var tokens = tokenizer.Tokenize();
        
        // Assert
        Assert.Equal(5, tokens.Count); // 4 tokens + EOF
        Assert.Equal(TokenType.Variable, tokens[0].Type);
        Assert.Equal("user", tokens[0].Value);
        Assert.Equal(TokenType.LeftBracket, tokens[1].Type);
        Assert.Equal(TokenType.String, tokens[2].Type);
        Assert.Equal("name", tokens[2].Value);
        Assert.Equal(TokenType.RightBracket, tokens[3].Type);
    }

    [Fact]
    public void Tokenize_UnexpectedCharacter_ShouldThrowException()
    {
        // Arrange
        var tokenizer = new Tokenizer("@");
        
        // Act & Assert
        var ex = Assert.Throws<QuitSyntaxException>(() => tokenizer.Tokenize());
        Assert.Contains("Unexpected character", ex.Message);
    }

    [Fact]
    public void Tokenize_UnterminatedString_ShouldThrowException()
    {
        // Arrange
        var tokenizer = new Tokenizer("\"unterminated");
        
        // Act & Assert
        var ex = Assert.Throws<QuitSyntaxException>(() => tokenizer.Tokenize());
        Assert.Contains("Unterminated string", ex.Message);
    }
}