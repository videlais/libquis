namespace LibQuis;

/// <summary>
/// Parser that builds an Abstract Syntax Tree from tokens
/// Handles operator precedence and creates a tree structure for evaluation
/// </summary>
public class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private int _current;

    /// <summary>
    /// Initializes a new instance of the Parser class with the specified tokens.
    /// </summary>
    /// <param name="tokens">The read-only list of tokens to parse.</param>
    /// <exception cref="ArgumentNullException">Thrown when tokens is null.</exception>
    /// <exception cref="ArgumentException">Thrown when tokens is empty.</exception>
    public Parser(IReadOnlyList<Token> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);
        
        if (tokens.Count == 0)
        {
            throw new ArgumentException("Token list cannot be empty.", nameof(tokens));
        }
        
        _tokens = tokens;
        _current = 0;
    }

    /// <summary>
    /// Parses the tokens and returns the root AST node
    /// </summary>
    /// <returns>The root AST node</returns>
    public IAstNode Parse()
    {
        var result = ParseOrExpression();
        
        if (!IsAtEnd())
        {
            var token = Peek();
            throw new QuitSyntaxException($"Unexpected token '{token.Value}' at position {token.Position}");
        }
        
        return result;
    }

    // OR has lowest precedence
    private IAstNode ParseOrExpression()
    {
        var left = ParseAndExpression();

        while (Match(TokenType.Or))
        {
            string op = Previous().Value;
            var right = ParseAndExpression();
            left = new BinaryOpNode(op, left, right);
        }

        return left;
    }

    // AND has higher precedence than OR
    private IAstNode ParseAndExpression()
    {
        var left = ParseComparisonExpression();

        while (Match(TokenType.And))
        {
            string op = Previous().Value;
            var right = ParseComparisonExpression();
            left = new BinaryOpNode(op, left, right);
        }

        return left;
    }

    // Comparison has higher precedence than AND
    private IAstNode ParseComparisonExpression()
    {
        // Handle unary NOT
        if (Match(TokenType.Not))
        {
            string op = Previous().Value;
            var operand = ParseComparisonExpression();
            return new UnaryOpNode(op, operand);
        }

        var left = ParseArithmeticExpression();

        // Check for custom condition
        if (Match(TokenType.Custom))
        {
            Consume(TokenType.Colon, "Expected ':' after 'custom'");
            var conditionName = Consume(TokenType.Identifier, "Expected condition name after 'custom:'").Value;
            var right = ParseArithmeticExpression();
            return new CustomConditionNode(conditionName, left, right);
        }

        // Check for comparison operators
        if (MatchComparison())
        {
            string op = Previous().Value;
            var right = ParseArithmeticExpression();
            return new BinaryOpNode(op, left, right);
        }

        return left;
    }

    // Arithmetic expression parsing with proper precedence  
    private IAstNode ParseArithmeticExpression()
    {
        return ParseAdditionExpression();
    }

    // Addition/Subtraction (lowest arithmetic precedence)
    private IAstNode ParseAdditionExpression()
    {
        var left = ParseMultiplicationExpression();

        while (Match(TokenType.Plus, TokenType.Minus))
        {
            string op = Previous().Value;
            var right = ParseMultiplicationExpression();
            left = new BinaryOpNode(op, left, right);
        }

        return left;
    }

    // Multiplication/Division (higher arithmetic precedence)
    private IAstNode ParseMultiplicationExpression()
    {
        var left = ParseValue();

        while (Match(TokenType.Multiply, TokenType.Divide))
        {
            string op = Previous().Value;
            var right = ParseValue();
            left = new BinaryOpNode(op, left, right);
        }

        return left;
    }

    private IAstNode ParseValue()
    {
        // Literals
        if (Match(TokenType.Number))
        {
            return ParseNumberLiteral();
        }

        if (Match(TokenType.String))
        {
            return new LiteralNode(Previous().Value);
        }

        if (Match(TokenType.Boolean))
        {
            bool value = Previous().Value.Equals("true", StringComparison.OrdinalIgnoreCase);
            return new LiteralNode(value);
        }

        if (Match(TokenType.Null))
        {
            return new LiteralNode(null);
        }

        // Handle parenthesized expressions
        if (Match(TokenType.LeftParen))
        {
            var expr = ParseOrExpression();
            Consume(TokenType.RightParen, "Expected ')' after expression");
            return expr;
        }

        // Variables and property access
        if (Match(TokenType.Variable))
        {
            return ParseVariableOrPropertyAccess();
        }

        throw new QuitSyntaxException($"Unexpected token '{Peek().Value}' at position {Peek().Position}");
    }

    private IAstNode ParseNumberLiteral()
    {
        string value = Previous().Value;
        if (double.TryParse(value, out double numValue))
        {
            return new LiteralNode(numValue);
        }
        throw new QuitSyntaxException($"Invalid number format: {value}");
    }

    private IAstNode ParseVariableOrPropertyAccess()
    {
        string varName = Previous().Value;
        
        // Check for property access
        if (Match(TokenType.Dot))
        {
            string property = Consume(TokenType.Identifier, "Expected property name after '.'").Value;
            return new PropertyAccessNode(varName, property, "dot");
        }
        
        if (Match(TokenType.LeftBracket))
        {
            string property = ParseBracketProperty();
            Consume(TokenType.RightBracket, "Expected ']' after property name");
            return new PropertyAccessNode(varName, property, "bracket");
        }
        
        return new VariableNode(varName);
    }

    private string ParseBracketProperty()
    {
        if (Check(TokenType.String))
        {
            return Advance().Value;
        }
        if (Check(TokenType.Identifier))
        {
            return Advance().Value;
        }
        throw new QuitSyntaxException("Expected string or identifier in bracket notation");
    }

    // Utility methods
    /// <summary>
    /// Checks if the current token matches any of the specified types and consumes it if matched.
    /// Note: Uses explicit foreach loop instead of LINQ for better performance during parsing.
    /// </summary>
    /// <param name="types">The token types to match against.</param>
    /// <returns>True if a match was found and the token was consumed; otherwise, false.</returns>
#pragma warning disable IDE0029, IDE0030, IDE0066 // Simplify loop - explicit loop is more performant for hot path
    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }
#pragma warning restore IDE0029, IDE0030, IDE0066

    private bool MatchComparison()
    {
        return Match(
            TokenType.Equals,
            TokenType.NotEquals,
            TokenType.GreaterThan,
            TokenType.GreaterThanEqual,
            TokenType.LessThan,
            TokenType.LessThanEqual,
            TokenType.Is,
            TokenType.IsNot,
            TokenType.Gt,
            TokenType.Gte,
            TokenType.Lt,
            TokenType.Lte
        );
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EOF;
    }

    private Token Peek()
    {
        return _tokens[_current];
    }

    private Token Previous()
    {
        return _tokens[_current - 1];
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        
        var token = Peek();
        throw new QuitSyntaxException($"{message}. Got '{token.Value}' at position {token.Position}");
    }
}