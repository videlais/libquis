namespace LibQuis;

/// <summary>
/// Main Quis library entry point providing expression parsing and evaluation capabilities
/// </summary>
public static class Quis
{
    /// <summary>
    /// Parse a DSL expression string and return the result
    /// </summary>
    /// <param name="input">The DSL expression to parse</param>
    /// <param name="options">Optional parsing configuration</param>
    /// <returns>The result of evaluating the expression</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
    /// <exception cref="QuitSyntaxException">Thrown when parsing fails</exception>
    public static object? Parse(string input, ParseOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(input);
        
        try
        {
            var tokenizer = new Tokenizer(input);
            var tokens = tokenizer.Tokenize();
            var parser = new Parser(tokens);
            var ast = parser.Parse();
            var evaluator = new Evaluator(options);
            return evaluator.Evaluate(ast);
        }
        catch (Exception ex) when (ex is not QuitSyntaxException and not ArgumentException and not ArgumentNullException)
        {
            throw new QuitSyntaxException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Parse a DSL expression string with a values callback
    /// </summary>
    /// <param name="input">The DSL expression to parse</param>
    /// <param name="valuesCallback">Callback function to resolve variable values</param>
    /// <returns>The result of evaluating the expression</returns>
    /// <exception cref="ArgumentNullException">Thrown when input or valuesCallback is null.</exception>
    public static object? Parse(string input, ValuesCallback valuesCallback)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(valuesCallback);
        
        var options = new ParseOptions { Values = valuesCallback };
        return Parse(input, options);
    }

    /// <summary>
    /// Parse a DSL expression string with values and custom conditions
    /// </summary>
    /// <param name="input">The DSL expression to parse</param>
    /// <param name="valuesCallback">Callback function to resolve variable values</param>
    /// <param name="customConditions">Dictionary of custom condition evaluators</param>
    /// <returns>The result of evaluating the expression</returns>
    /// <exception cref="ArgumentNullException">Thrown when input, valuesCallback, or customConditions is null.</exception>
    public static object? Parse(string input, ValuesCallback valuesCallback, Dictionary<string, CustomConditionEvaluator> customConditions)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(valuesCallback);
        ArgumentNullException.ThrowIfNull(customConditions);
        
        var options = new ParseOptions 
        { 
            Values = valuesCallback,
            CustomConditions = customConditions
        };
        return Parse(input, options);
    }

    /// <summary>
    /// Creates a tokenizer for the given input (for advanced usage)
    /// </summary>
    /// <param name="input">The input string to tokenize</param>
    /// <returns>A configured tokenizer</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
    /// <exception cref="ArgumentException">Thrown when input is empty or whitespace.</exception>
    public static Tokenizer CreateTokenizer(string input)
    {
        return new Tokenizer(input);
    }

    /// <summary>
    /// Creates a parser for the given tokens (for advanced usage).
    /// </summary>
    /// <param name="tokens">The read-only list of tokens to parse.</param>
    /// <returns>A configured parser instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when tokens is null.</exception>
    /// <exception cref="ArgumentException">Thrown when tokens is empty.</exception>
    public static Parser CreateParser(IReadOnlyList<Token> tokens)
    {
        return new Parser(tokens);
    }

    /// <summary>
    /// Creates an evaluator with the given options (for advanced usage)
    /// </summary>
    /// <param name="options">The evaluation options</param>
    /// <returns>A configured evaluator</returns>
    public static Evaluator CreateEvaluator(ParseOptions? options = null)
    {
        return new Evaluator(options);
    }
}