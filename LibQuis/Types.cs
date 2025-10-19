namespace LibQuis;

/// <summary>
/// Callback function type for variable resolution
/// </summary>
/// <param name="variableName">The name of the variable to resolve</param>
/// <returns>The value of the variable</returns>
public delegate object? ValuesCallback(string variableName);

/// <summary>
/// Custom condition evaluator function type
/// </summary>
/// <param name="value">The left operand value</param>
/// <param name="expected">The right operand value</param>
/// <returns>True if the condition is met, false otherwise</returns>
public delegate bool CustomConditionEvaluator(object? value, object? expected);

/// <summary>
/// Options for the parse function
/// </summary>
public class ParseOptions
{
    /// <summary>
    /// Callback function to resolve variable values
    /// </summary>
    public ValuesCallback? Values { get; set; }
    
    /// <summary>
    /// Registry of custom condition evaluators
    /// </summary>
    public Dictionary<string, CustomConditionEvaluator>? CustomConditions { get; set; }
}

/// <summary>
/// Exception thrown when parsing fails
/// </summary>
public class QuitSyntaxException : Exception
{
    /// <summary>
    /// Initializes a new instance of the QuitSyntaxException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public QuitSyntaxException(string message) : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the QuitSyntaxException class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public QuitSyntaxException(string message, Exception innerException) : base(message, innerException) { }
}