namespace LibQuis;

/// <summary>
/// Defines the types of tokens that can be recognized in the Quis expression language.
/// These token types are used by the tokenizer to categorize different elements
/// of an expression during the parsing process.
/// </summary>
public enum TokenType
{
    // Literals
    /// <summary>
    /// Represents a numeric literal (integer or floating-point number).
    /// </summary>
    /// <example>42, 3.14, -100</example>
    Number,
    
    /// <summary>
    /// Represents a string literal enclosed in double quotes.
    /// </summary>
    /// <example>"hello", "world", "test string"</example>
    String,
    
    /// <summary>
    /// Represents a boolean literal (true or false).
    /// </summary>
    /// <example>true, false</example>
    Boolean,
    
    /// <summary>
    /// Represents a null literal value.
    /// </summary>
    /// <example>null</example>
    Null,
    
    // Variables
    /// <summary>
    /// Represents a variable reference (prefixed with $).
    /// </summary>
    /// <example>$user, $health, $level</example>
    Variable,
    
    /// <summary>
    /// Represents the dot operator (.) used for property access.
    /// </summary>
    /// <example>$user.name, $player.inventory</example>
    Dot,
    
    /// <summary>
    /// Represents the left bracket ([) used for bracket notation property access.
    /// </summary>
    /// <example>$user["status"], $data[0]</example>
    LeftBracket,
    
    /// <summary>
    /// Represents the right bracket (]) used for bracket notation property access.
    /// </summary>
    /// <example>$user["status"], $data[0]</example>
    RightBracket,
    
    // Operators
    /// <summary>
    /// Represents the equality operator (==) for comparing two values.
    /// </summary>
    /// <example>$age == 18, $name == "John"</example>
    Equals,
    
    /// <summary>
    /// Represents the inequality operator (!=) for comparing two values.
    /// </summary>
    /// <example>$status != "inactive", $count != 0</example>
    NotEquals,
    
    /// <summary>
    /// Represents the greater than operator (&gt;) for numeric comparisons.
    /// </summary>
    /// <example>$age &gt; 18, $score &gt; 100</example>
    GreaterThan,
    
    /// <summary>
    /// Represents the greater than or equal operator (&gt;=) for numeric comparisons.
    /// </summary>
    /// <example>$age &gt;= 18, $score &gt;= 100</example>
    GreaterThanEqual,
    
    /// <summary>
    /// Represents the less than operator (&lt;) for numeric comparisons.
    /// </summary>
    /// <example>$age &lt; 65, $temperature &lt; 32</example>
    LessThan,
    
    /// <summary>
    /// Represents the less than or equal operator (&lt;=) for numeric comparisons.
    /// </summary>
    /// <example>$age &lt;= 65, $temperature &lt;= 32</example>
    LessThanEqual,
    
    // Arithmetic operators
    /// <summary>
    /// Represents the addition operator (+) for arithmetic operations.
    /// </summary>
    /// <example>$health + 10, 5 + 3</example>
    Plus,
    
    /// <summary>
    /// Represents the subtraction operator (-) for arithmetic operations.
    /// </summary>
    /// <example>$health - 5, 10 - 3</example>
    Minus,
    
    /// <summary>
    /// Represents the multiplication operator (*) for arithmetic operations.
    /// </summary>
    /// <example>$damage * 2, 5 * 3</example>
    Multiply,
    
    /// <summary>
    /// Represents the division operator (/) for arithmetic operations.
    /// </summary>
    /// <example>$total / 2, 10 / 5</example>
    Divide,
    
    // Shorthand operators
    /// <summary>
    /// Represents the "is" operator for equality comparison (alternative to ==).
    /// </summary>
    /// <example>$status is "active", $value is null</example>
    Is,
    
    /// <summary>
    /// Represents the "is not" operator for inequality comparison (alternative to !=).
    /// </summary>
    /// <example>$status is not "inactive", $value is not null</example>
    IsNot,
    
    /// <summary>
    /// Represents the "gt" operator for greater than comparison (alternative to &gt;).
    /// </summary>
    /// <example>$age gt 18, $score gt 100</example>
    Gt,
    
    /// <summary>
    /// Represents the "gte" operator for greater than or equal comparison (alternative to &gt;=).
    /// </summary>
    /// <example>$age gte 18, $score gte 100</example>
    Gte,
    
    /// <summary>
    /// Represents the "lt" operator for less than comparison (alternative to &lt;).
    /// </summary>
    /// <example>$age lt 65, $temperature lt 32</example>
    Lt,
    
    /// <summary>
    /// Represents the "lte" operator for less than or equal comparison (alternative to &lt;=).
    /// </summary>
    /// <example>$age lte 65, $temperature lte 32</example>
    Lte,
    
    // Logical operators
    /// <summary>
    /// Represents the logical AND operator (&amp;&amp; or "and") for boolean operations.
    /// </summary>
    /// <example>$isActive &amp;&amp; $isVerified, $age &gt; 18 and $hasPermission</example>
    And,
    
    /// <summary>
    /// Represents the logical OR operator (|| or "or") for boolean operations.
    /// </summary>
    /// <example>$isAdmin || $isModerator, $age &lt; 13 or $age &gt; 65</example>
    Or,
    
    /// <summary>
    /// Represents the logical NOT operator (!) for boolean negation.
    /// </summary>
    /// <example>!$isBlocked, !($age &lt; 18)</example>
    Not,
    
    // Grouping
    /// <summary>
    /// Represents the left parenthesis (() used for expression grouping and precedence control.
    /// </summary>
    /// <example>($age + $bonus) * 2, !($isActive &amp;&amp; $isVerified)</example>
    LeftParen,
    
    /// <summary>
    /// Represents the right parenthesis ()) used for expression grouping and precedence control.
    /// </summary>
    /// <example>($age + $bonus) * 2, !($isActive &amp;&amp; $isVerified)</example>
    RightParen,
    
    // Custom conditions
    /// <summary>
    /// Represents the "custom" keyword used to introduce custom condition evaluators.
    /// </summary>
    /// <example>$text custom:contains "hello", $age custom:between "18-65"</example>
    Custom,
    
    /// <summary>
    /// Represents the colon (:) used as a separator in custom conditions.
    /// </summary>
    /// <example>custom:contains, custom:between, custom:matches</example>
    Colon,
    
    // Identifiers
    /// <summary>
    /// Represents an identifier (variable or property name) used in expressions.
    /// </summary>
    /// <example>name, age, status, inventory</example>
    Identifier,
    
    // End of file
    /// <summary>
    /// Represents the end of the input stream (End of File marker).
    /// </summary>
    /// <remarks>This token is automatically added at the end of every token sequence.</remarks>
    EOF
}