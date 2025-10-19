namespace LibQuis;

/// <summary>
/// Represents a token with its type, value, and position in the input
/// </summary>
public record Token(TokenType Type, string Value, int Position);