namespace LibQuis;

/// <summary>
/// Base interface for all Abstract Syntax Tree (AST) nodes used in expression parsing.
/// All AST nodes implement this interface to provide a common contract for tree traversal.
/// </summary>
public interface IAstNode
{
    /// <summary>
    /// Gets the type identifier of the AST node, used for pattern matching during evaluation.
    /// </summary>
    /// <value>A string identifier representing the node type (e.g., "literal", "binary", "variable").</value>
    string Type { get; }
}

/// <summary>
/// Represents a literal value in the Abstract Syntax Tree (AST).
/// Literal nodes contain constant values such as numbers, strings, or booleans
/// that are directly embedded in the expression.
/// </summary>
/// <param name="Value">The literal value, which can be a number, string, boolean, or null.</param>
/// <example>
/// Examples of expressions that create LiteralNode instances:
/// <code>
/// "hello"    // Creates LiteralNode with Value = "hello"
/// 42         // Creates LiteralNode with Value = 42
/// true       // Creates LiteralNode with Value = true
/// </code>
/// </example>
public record LiteralNode(object? Value) : IAstNode
{
    /// <inheritdoc />
    public string Type => "literal";
}

/// <summary>
/// Represents a variable reference in the Abstract Syntax Tree (AST).
/// Variable nodes reference named values that are resolved at evaluation time
/// through the values callback function.
/// </summary>
/// <param name="Name">The name of the variable (without the $ prefix).</param>
/// <example>
/// Examples of expressions that create VariableNode instances:
/// <code>
/// $user      // Creates VariableNode with Name = "user"
/// $health    // Creates VariableNode with Name = "health"
/// $level     // Creates VariableNode with Name = "level"
/// </code>
/// </example>
public record VariableNode(string Name) : IAstNode
{
    /// <inheritdoc />
    public string Type => "variable";
}

/// <summary>
/// Represents property access (dot or bracket notation) in the Abstract Syntax Tree (AST).
/// Property access nodes allow accessing properties or elements of objects or arrays
/// using either dot notation or bracket notation.
/// </summary>
/// <param name="Object">The name of the object being accessed (without the $ prefix).</param>
/// <param name="Property">The name of the property or key being accessed.</param>
/// <param name="Notation">The notation used for access: "dot" for dot notation or "bracket" for bracket notation.</param>
/// <example>
/// Examples of expressions that create PropertyAccessNode instances:
/// <code>
/// $user.name        // Creates PropertyAccessNode with Object="user", Property="name", Notation="dot"
/// $user["status"]   // Creates PropertyAccessNode with Object="user", Property="status", Notation="bracket"
/// $data.items[0]    // Creates nested property access nodes
/// </code>
/// </example>
public record PropertyAccessNode(string Object, string Property, string Notation) : IAstNode
{
    /// <inheritdoc />
    public string Type => "property";
}

/// <summary>
/// Represents a binary operation in the Abstract Syntax Tree (AST).
/// Binary operations involve two operands and an operator, such as arithmetic,
/// comparison, or logical operations.
/// </summary>
/// <param name="Operator">The binary operator (e.g., "+", "-", "==", "&amp;&amp;", "&gt;", etc.).</param>
/// <param name="Left">The left operand of the binary operation.</param>
/// <param name="Right">The right operand of the binary operation.</param>
/// <example>
/// Examples of expressions that create BinaryOpNode instances:
/// <code>
/// 5 + 3           // Creates BinaryOpNode with Operator="+", Left=LiteralNode(5), Right=LiteralNode(3)
/// $age &gt;= 18      // Creates BinaryOpNode with Operator="&gt;=", Left=VariableNode("age"), Right=LiteralNode(18)
/// $a &amp;&amp; $b        // Creates BinaryOpNode with Operator="&amp;&amp;", Left=VariableNode("a"), Right=VariableNode("b")
/// </code>
/// </example>
public record BinaryOpNode(string Operator, IAstNode Left, IAstNode Right) : IAstNode
{
    /// <inheritdoc />
    public string Type => "binary";
}

/// <summary>
/// Represents a unary operation in the Abstract Syntax Tree (AST).
/// Unary operations involve a single operand and an operator, such as negation
/// or logical NOT operations.
/// </summary>
/// <param name="Operator">The unary operator (e.g., "!", "-", "+").</param>
/// <param name="Operand">The operand of the unary operation.</param>
/// <example>
/// Examples of expressions that create UnaryOpNode instances:
/// <code>
/// !true           // Creates UnaryOpNode with Operator="!", Operand=LiteralNode(true)
/// -5              // Creates UnaryOpNode with Operator="-", Operand=LiteralNode(5)
/// !$isActive      // Creates UnaryOpNode with Operator="!", Operand=VariableNode("isActive")
/// </code>
/// </example>
public record UnaryOpNode(string Operator, IAstNode Operand) : IAstNode
{
    /// <inheritdoc />
    public string Type => "unary";
}

/// <summary>
/// Represents a custom condition in the Abstract Syntax Tree (AST).
/// Custom conditions allow extending the parser with domain-specific evaluation logic
/// through user-defined condition evaluators.
/// </summary>
/// <param name="Name">The name of the custom condition (after "custom:").</param>
/// <param name="Left">The left operand of the custom condition.</param>
/// <param name="Right">The right operand of the custom condition.</param>
/// <example>
/// Examples of expressions that create CustomConditionNode instances:
/// <code>
/// $text custom:contains "hello"     // Creates CustomConditionNode with Name="contains"
/// $age custom:between "18-65"       // Creates CustomConditionNode with Name="between"
/// $data custom:matches "pattern"    // Creates CustomConditionNode with Name="matches"
/// </code>
/// </example>
public record CustomConditionNode(string Name, IAstNode Left, IAstNode Right) : IAstNode
{
    /// <inheritdoc />
    public string Type => "custom";
}