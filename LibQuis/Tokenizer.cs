using System.Buffers;
using System.Text;

namespace LibQuis;

/// <summary>
/// Tokenizer for Quis expressions
/// Converts input string into array of tokens
/// </summary>
public class Tokenizer
{
    private readonly string _input;
    private int _position;
    private readonly List<Token> _tokens;
    
    /// <summary>
    /// Keyword lookup dictionary for efficient case-insensitive token type resolution.
    /// Uses StringComparer.OrdinalIgnoreCase for optimal performance.
    /// </summary>
    private static readonly Dictionary<string, TokenType> KeywordMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["true"] = TokenType.Boolean,
        ["false"] = TokenType.Boolean,
        ["null"] = TokenType.Null,
        ["and"] = TokenType.And,
        ["or"] = TokenType.Or,
        ["not"] = TokenType.Not,
        ["is"] = TokenType.Is,
        ["gt"] = TokenType.Gt,
        ["gte"] = TokenType.Gte,
        ["lt"] = TokenType.Lt,
        ["lte"] = TokenType.Lte,
        ["custom"] = TokenType.Custom
    };

    /// <summary>
    /// Initializes a new instance of the Tokenizer class with the specified input string.
    /// </summary>
    /// <param name="input">The input string to tokenize.</param>
    /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
    /// <exception cref="ArgumentException">Thrown when input is empty or whitespace.</exception>
    public Tokenizer(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        
        _input = input.Trim();
        
        if (string.IsNullOrWhiteSpace(_input))
        {
            throw new ArgumentException("Input cannot be empty or whitespace.", nameof(input));
        }
        
        _position = 0;
        _tokens = [];
    }

    /// <summary>
    /// Tokenizes the input string and returns a read-only list of tokens.
    /// The returned collection is immutable to prevent external modifications.
    /// </summary>
    /// <returns>A read-only list of tokens representing the parsed expression.</returns>
    public IReadOnlyList<Token> Tokenize()
    {
        _tokens.Clear();
        _position = 0;

        while (_position < _input.Length)
        {
            SkipWhitespace();
            
            if (_position >= _input.Length) break;

            char ch = Peek();
            
            // Numbers (including negative)
            if (char.IsDigit(ch) || (ch == '-' && char.IsDigit(PeekNext())))
            {
                TokenizeNumber();
            }
            // Strings
            else if (ch == '"' || ch == '\'')
            {
                TokenizeString();
            }
            // Variables
            else if (ch == '$')
            {
                TokenizeVariable();
            }
            // Two-character operators
            else if (_position + 1 < _input.Length)
            {
                string twoChar = _input.Substring(_position, 2);
                if (TokenizeTwoCharOperator(twoChar))
                {
                    continue;
                }
                // Single-character operators
                TokenizeSingleChar(ch);
            }
            // Single-character operators
            else
            {
                TokenizeSingleChar(ch);
            }
        }

        AddToken(TokenType.EOF, "");
        return _tokens;
    }

    private char Peek()
    {
        return _position < _input.Length ? _input[_position] : '\0';
    }

    private char PeekNext()
    {
        return _position + 1 < _input.Length ? _input[_position + 1] : '\0';
    }

    private char Advance()
    {
        return _position < _input.Length ? _input[_position++] : '\0';
    }

    private void SkipWhitespace()
    {
        while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
        {
            _position++;
        }
    }

    private void AddToken(TokenType type, string value)
    {
        _tokens.Add(new Token(type, value, _position - value.Length));
    }

    /// <summary>
    /// Tokenizes a numeric value from the input stream.
    /// Uses ReadOnlySpan&lt;char&gt; to avoid heap allocations during parsing.
    /// </summary>
    private void TokenizeNumber()
    {
        int start = _position;
        
        // Handle negative sign
        if (Peek() == '-')
        {
            Advance();
        }

        while (_position < _input.Length && (char.IsDigit(Peek()) || Peek() == '.'))
        {
            Advance();
        }

        // Use AsSpan to avoid intermediate string allocation
        ReadOnlySpan<char> valueSpan = _input.AsSpan(start, _position - start);
        string value = valueSpan.ToString();
        AddToken(TokenType.Number, value);
    }

    /// <summary>
    /// Tokenizes a string literal from the input stream.
    /// Uses ArrayPool&lt;char&gt; to rent buffers and reduce heap allocations during string parsing.
    /// Handles escape sequences: \n, \r, \t, \\, \", \'.
    /// </summary>
    private void TokenizeString()
    {
        char quote = Advance(); // consume opening quote
        
        // Estimate initial size based on remaining input, capped at 256 characters
        int estimatedSize = Math.Min(_input.Length - _position, 256);
        char[] buffer = ArrayPool<char>.Shared.Rent(estimatedSize);
        int length = 0;

        try
        {
            while (_position < _input.Length && Peek() != quote)
            {
                // Grow buffer if needed
                if (length >= buffer.Length)
                {
                    char[] newBuffer = ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                    Array.Copy(buffer, newBuffer, length);
                    ArrayPool<char>.Shared.Return(buffer);
                    buffer = newBuffer;
                }

                if (Peek() == '\\' && _position + 1 < _input.Length)
                {
                    Advance(); // consume backslash
                    char escaped = Advance();
                    buffer[length++] = escaped switch
                    {
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        '\\' => '\\',
                        '"' => '"',
                        '\'' => '\'',
                        _ => escaped
                    };
                }
                else
                {
                    buffer[length++] = Advance();
                }
            }

            if (_position >= _input.Length)
            {
                throw new QuitSyntaxException($"Unterminated string at position {_position}");
            }

            Advance(); // consume closing quote
            
            // Create string from the buffer portion we used
            string value = new string(buffer, 0, length);
            AddToken(TokenType.String, value);
        }
        finally
        {
            // Always return the buffer to the pool
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    private void TokenizeVariable()
    {
        Advance(); // consume '$'
        string identifier = TokenizeIdentifier();
        AddToken(TokenType.Variable, identifier);
    }

    /// <summary>
    /// Tokenizes an identifier from the input stream.
    /// Uses ReadOnlySpan&lt;char&gt; to avoid heap allocations during parsing.
    /// </summary>
    /// <returns>The identifier string.</returns>
    private string TokenizeIdentifier()
    {
        int start = _position;

        while (_position < _input.Length && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
        {
            Advance();
        }

        // Use AsSpan to avoid intermediate string allocation
        ReadOnlySpan<char> identifierSpan = _input.AsSpan(start, _position - start);
        return identifierSpan.ToString();
    }

    private void TokenizeKeywordOrIdentifier()
    {
        string identifier = TokenizeIdentifier();
        
        // Use dictionary lookup with case-insensitive comparison for better performance
        TokenType tokenType = KeywordMap.TryGetValue(identifier, out var type) 
            ? type 
            : TokenType.Identifier;

        AddToken(tokenType, identifier);
    }

    /// <summary>
    /// Attempts to tokenize a two-character operator or the "is not" keyword.
    /// Uses ReadOnlySpan&lt;char&gt; comparisons to avoid string allocations.
    /// </summary>
    /// <param name="twoChar">The two-character string to check (kept for API compatibility).</param>
    /// <returns>True if a multi-character operator was tokenized; otherwise, false.</returns>
    private bool TokenizeTwoCharOperator(string twoChar)
    {
        TokenType? tokenType = twoChar switch
        {
            "==" => TokenType.Equals,
            "!=" => TokenType.NotEquals,
            ">=" => TokenType.GreaterThanEqual,
            "<=" => TokenType.LessThanEqual,
            "&&" => TokenType.And,
            "||" => TokenType.Or,
            _ => null
        };

        if (tokenType.HasValue)
        {
            _position += 2;
            AddToken(tokenType.Value, twoChar);
            return true;
        }

        // Check for "is not" (special case handling) - use span-based case-insensitive comparison
        if (_position + 5 < _input.Length)
        {
            ReadOnlySpan<char> segment = _input.AsSpan(_position, 6);
            if (segment.Equals("is not", StringComparison.OrdinalIgnoreCase))
            {
                _position += 6; // "is not"
                AddToken(TokenType.IsNot, "is not");
                return true;
            }
        }

        return false;
    }

    private void TokenizeSingleChar(char ch)
    {
        switch (ch)
        {
            case '>':
                Advance();
                AddToken(TokenType.GreaterThan, ">");
                break;
            case '<':
                Advance();
                AddToken(TokenType.LessThan, "<");
                break;
            case '(':
                Advance();
                AddToken(TokenType.LeftParen, "(");
                break;
            case ')':
                Advance();
                AddToken(TokenType.RightParen, ")");
                break;
            case '.':
                Advance();
                AddToken(TokenType.Dot, ".");
                break;
            case '[':
                Advance();
                AddToken(TokenType.LeftBracket, "[");
                break;
            case ']':
                Advance();
                AddToken(TokenType.RightBracket, "]");
                break;
            case ':':
                Advance();
                AddToken(TokenType.Colon, ":");
                break;
            case '!':
                Advance();
                AddToken(TokenType.Not, "!");
                break;
            case '+':
                Advance();
                AddToken(TokenType.Plus, "+");
                break;
            case '-':
                Advance();
                AddToken(TokenType.Minus, "-");
                break;
            case '*':
                Advance();
                AddToken(TokenType.Multiply, "*");
                break;
            case '/':
                Advance();
                AddToken(TokenType.Divide, "/");
                break;
            default:
                if (char.IsLetter(ch) || ch == '_')
                {
                    TokenizeKeywordOrIdentifier();
                }
                else
                {
                    throw new QuitSyntaxException($"Unexpected character '{ch}' at position {_position}");
                }
                break;
        }
    }
}