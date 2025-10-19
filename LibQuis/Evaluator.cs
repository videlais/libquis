using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace LibQuis;

/// <summary>
/// Evaluates an Abstract Syntax Tree to produce the final result
/// Handles all node types and operations defined in the Quis DSL
/// </summary>
public class Evaluator
{
    private readonly ParseOptions _options;
    
    /// <summary>
    /// Cache for property reflection to avoid repeated lookups.
    /// Key is (Type, PropertyName), Value is the PropertyInfo or null if not found.
    /// </summary>
    private static readonly ConcurrentDictionary<(Type Type, string PropertyName), PropertyInfo?> PropertyCache = new();

    /// <summary>
    /// Initializes a new instance of the Evaluator class
    /// </summary>
    /// <param name="options">The parse options to use for evaluation. If null, default options will be used.</param>
    public Evaluator(ParseOptions? options = null)
    {
        _options = options ?? new ParseOptions();
    }

    /// <summary>
    /// Evaluates an AST node and returns the result
    /// </summary>
    /// <param name="node">The AST node to evaluate</param>
    /// <returns>The result of evaluating the node</returns>
    public object? Evaluate(IAstNode node)
    {
        return node switch
        {
            LiteralNode literal => EvaluateLiteral(literal),
            VariableNode variable => EvaluateVariable(variable),
            PropertyAccessNode property => EvaluatePropertyAccess(property),
            BinaryOpNode binary => EvaluateBinaryOperation(binary),
            UnaryOpNode unary => EvaluateUnaryOperation(unary),
            CustomConditionNode custom => EvaluateCustomCondition(custom),
            _ => throw new QuitSyntaxException($"Unknown AST node type: {node.Type}")
        };
    }

    private static object? EvaluateLiteral(LiteralNode node)
    {
        return node.Value;
    }

    /// <summary>
    /// Evaluates a variable node by invoking the values callback to resolve the variable's value.
    /// Returns null if the callback throws an exception or if no callback is configured.
    /// </summary>
    /// <param name="node">The variable node containing the variable name.</param>
    /// <returns>The resolved value of the variable, or null if resolution fails.</returns>
    private object? EvaluateVariable(VariableNode node)
    {
        try
        {
            return _options.Values?.Invoke(node.Name);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Evaluates property access on an object, using cached reflection for performance.
    /// </summary>
    /// <param name="node">The property access node containing the object and property names.</param>
    /// <returns>The value of the property, or null if not found or an error occurs.</returns>
    private object? EvaluatePropertyAccess(PropertyAccessNode node)
    {
        try
        {
            var obj = _options.Values?.Invoke(node.Object);
            if (obj == null) return null;

            // Use cached reflection to get property value for better performance
            var type = obj.GetType();
            var property = PropertyCache.GetOrAdd(
                (type, node.Property),
                key => key.Type.GetProperty(key.PropertyName)
            );
            
            if (property != null)
            {
                return property.GetValue(obj);
            }

            // Try as dictionary/indexer access
            if (obj is System.Collections.IDictionary dict)
            {
                return dict[node.Property];
            }

            // Try dynamic object
            if (obj is System.Dynamic.DynamicObject)
            {
                // For dynamic objects, we'd need more complex handling
                // For now, return null
                return null;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Evaluates a binary operation by recursively evaluating both operands and applying the operator.
    /// Supports arithmetic (+, -, *, /), comparison (==, !=, &gt;, &lt;, &gt;=, &lt;=), and logical (&amp;&amp;, ||) operators.
    /// Includes support for shorthand operators (is, gt, gte, lt, lte).
    /// </summary>
    /// <param name="node">The binary operation node containing the operator and operands.</param>
    /// <returns>The result of the binary operation.</returns>
    /// <exception cref="QuitSyntaxException">Thrown when an unknown operator is encountered.</exception>
    private object? EvaluateBinaryOperation(BinaryOpNode node)
    {
        var left = Evaluate(node.Left);
        var right = Evaluate(node.Right);

        return node.Operator switch
        {
            "+" => Add(left, right),
            "-" => Subtract(left, right),
            "*" => Multiply(left, right),
            "/" => Divide(left, right),
            "==" or "is" => AreEqual(left, right),
            "!=" or "is not" => !AreEqual(left, right),
            ">" or "gt" => IsGreater(left, right),
            "<" or "lt" => IsLess(left, right),
            ">=" or "gte" => IsGreaterOrEqual(left, right),
            "<=" or "lte" => IsLessOrEqual(left, right),
            "&&" or "and" => IsTruthy(left) && IsTruthy(right),
            "||" or "or" => IsTruthy(left) || IsTruthy(right),
            _ => throw new QuitSyntaxException($"Unknown binary operator: {node.Operator}")
        };
    }

    /// <summary>
    /// Evaluates a unary operation by recursively evaluating the operand and applying the operator.
    /// Currently supports logical NOT (! and "not") operator.
    /// </summary>
    /// <param name="node">The unary operation node containing the operator and operand.</param>
    /// <returns>The result of the unary operation.</returns>
    /// <exception cref="QuitSyntaxException">Thrown when an unknown operator is encountered.</exception>
    private object? EvaluateUnaryOperation(UnaryOpNode node)
    {
        var operand = Evaluate(node.Operand);

        return node.Operator switch
        {
            "!" or "not" => !IsTruthy(operand),
            _ => throw new QuitSyntaxException($"Unknown unary operator: {node.Operator}")
        };
    }

    /// <summary>
    /// Evaluates a custom condition by looking up and invoking the registered custom evaluator.
    /// Custom conditions allow extending the parser with domain-specific logic.
    /// </summary>
    /// <param name="node">The custom condition node containing the condition name and operands.</param>
    /// <returns>The boolean result from the custom evaluator.</returns>
    /// <exception cref="QuitSyntaxException">Thrown when the custom condition is not registered.</exception>
    private object? EvaluateCustomCondition(CustomConditionNode node)
    {
        if (_options.CustomConditions?.TryGetValue(node.Name, out var evaluator) == true)
        {
            var left = Evaluate(node.Left);
            var right = Evaluate(node.Right);
            return evaluator(left, right);
        }
        
        throw new QuitSyntaxException($"Unknown custom condition: {node.Name}");
    }

    /// <summary>
    /// Converts two values to numbers and adds them together.
    /// Returns NaN if either value cannot be converted to a number.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>The sum of the two numbers, or NaN if conversion fails.</returns>
    private static object? Add(object? left, object? right)
    {
        var leftNum = ToNumber(left);
        var rightNum = ToNumber(right);
        
        if (double.IsNaN(leftNum) || double.IsNaN(rightNum))
        {
            return double.NaN;
        }
        
        return leftNum + rightNum;
    }

    /// <summary>
    /// Converts two values to numbers and subtracts the right from the left.
    /// Returns NaN if either value cannot be converted to a number.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>The difference of the two numbers, or NaN if conversion fails.</returns>
    private static object? Subtract(object? left, object? right)
    {
        var leftNum = ToNumber(left);
        var rightNum = ToNumber(right);
        
        if (double.IsNaN(leftNum) || double.IsNaN(rightNum))
        {
            return double.NaN;
        }
        
        return leftNum - rightNum;
    }

    /// <summary>
    /// Converts two values to numbers and multiplies them together.
    /// Returns NaN if either value cannot be converted to a number.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>The product of the two numbers, or NaN if conversion fails.</returns>
    private static object? Multiply(object? left, object? right)
    {
        var leftNum = ToNumber(left);
        var rightNum = ToNumber(right);
        
        if (double.IsNaN(leftNum) || double.IsNaN(rightNum))
        {
            return double.NaN;
        }
        
        return leftNum * rightNum;
    }

    /// <summary>
    /// Converts two values to numbers and divides the left by the right.
    /// Returns NaN if either value cannot be converted to a number.
    /// Returns PositiveInfinity or NegativeInfinity for division by zero.
    /// </summary>
    /// <param name="left">The left operand (dividend).</param>
    /// <param name="right">The right operand (divisor).</param>
    /// <returns>The quotient of the two numbers, Infinity for division by zero, or NaN if conversion fails.</returns>
    private static object? Divide(object? left, object? right)
    {
        var leftNum = ToNumber(left);
        var rightNum = ToNumber(right);
        
        if (double.IsNaN(leftNum) || double.IsNaN(rightNum))
        {
            return double.NaN;
        }
        
        if (Math.Abs(rightNum) < double.Epsilon)
        {
            return leftNum > 0 ? double.PositiveInfinity : double.NegativeInfinity;
        }
        
        return leftNum / rightNum;
    }

    /// <summary>
    /// Compares two values for equality.
    /// Attempts direct equality first, then numeric comparison, and finally string comparison.
    /// Handles null values and type coercion automatically.
    /// </summary>
    /// <param name="left">The left value to compare.</param>
    /// <param name="right">The right value to compare.</param>
    /// <returns>True if the values are considered equal; otherwise, false.</returns>
    private static bool AreEqual(object? left, object? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left == null || right == null) return false;

        // Try direct equality first
        if (left.Equals(right)) return true;

        // Try comparing as numbers
        var leftNum = ToNumber(left);
        var rightNum = ToNumber(right);
        
        if (!double.IsNaN(leftNum) && !double.IsNaN(rightNum))
        {
            return Math.Abs(leftNum - rightNum) < double.Epsilon;
        }

        // Try comparing as strings
        return left.ToString() == right.ToString();
    }

    /// <summary>
    /// Converts two values to numbers and compares them using greater-than logic.
    /// Returns false if either value cannot be converted to a number.
    /// </summary>
    /// <param name="left">The left value to compare.</param>
    /// <param name="right">The right value to compare.</param>
    /// <returns>True if left is greater than right; otherwise, false.</returns>
    private static bool IsGreater(object? left, object? right)
    {
        var leftNum = ToNumber(left);
        var rightNum = ToNumber(right);
        
        if (double.IsNaN(leftNum) || double.IsNaN(rightNum))
        {
            return false;
        }
        
        return leftNum > rightNum;
    }

    /// <summary>
    /// Converts two values to numbers and compares them using less-than logic.
    /// Returns false if either value cannot be converted to a number.
    /// </summary>
    /// <param name="left">The left value to compare.</param>
    /// <param name="right">The right value to compare.</param>
    /// <returns>True if left is less than right; otherwise, false.</returns>
    private static bool IsLess(object? left, object? right)
    {
        var leftNum = ToNumber(left);
        var rightNum = ToNumber(right);
        
        if (double.IsNaN(leftNum) || double.IsNaN(rightNum))
        {
            return false;
        }
        
        return leftNum < rightNum;
    }

    /// <summary>
    /// Checks if the left value is greater than or equal to the right value.
    /// Combines IsGreater and AreEqual logic.
    /// </summary>
    /// <param name="left">The left value to compare.</param>
    /// <param name="right">The right value to compare.</param>
    /// <returns>True if left is greater than or equal to right; otherwise, false.</returns>
    private static bool IsGreaterOrEqual(object? left, object? right)
    {
        return IsGreater(left, right) || AreEqual(left, right);
    }

    /// <summary>
    /// Checks if the left value is less than or equal to the right value.
    /// Combines IsLess and AreEqual logic.
    /// </summary>
    /// <param name="left">The left value to compare.</param>
    /// <param name="right">The right value to compare.</param>
    /// <returns>True if left is less than or equal to right; otherwise, false.</returns>
    private static bool IsLessOrEqual(object? left, object? right)
    {
        return IsLess(left, right) || AreEqual(left, right);
    }

    /// <summary>
    /// Converts a value to a numeric representation for arithmetic and comparison operations.
    /// Supports conversion from common numeric types, booleans (true=1, false=0), and numeric strings.
    /// Returns NaN if the value cannot be converted to a number.
    /// </summary>
    /// <param name="value">The value to convert to a number.</param>
    /// <returns>The numeric representation of the value, or NaN if conversion fails.</returns>
    private static double ToNumber(object? value)
    {
        return value switch
        {
            null => double.NaN,
            double d => d,
            float f => f,
            int i => i,
            long l => l,
            decimal dec => (double)dec,
            bool b => b ? 1.0 : 0.0,
            string s when double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var num) => num,
            _ => double.NaN
        };
    }

    /// <summary>
    /// Determines if a value should be considered "truthy" in a boolean context.
    /// Follows JavaScript-like truthiness rules:
    /// - null is false
    /// - false is false
    /// - 0 (any numeric type) is false
    /// - NaN is false
    /// - Empty string is false
    /// - All other values are true
    /// </summary>
    /// <param name="value">The value to evaluate for truthiness.</param>
    /// <returns>True if the value is truthy; otherwise, false.</returns>
    private static bool IsTruthy(object? value)
    {
        return value switch
        {
            null => false,
            bool b => b,
            double d => !double.IsNaN(d) && Math.Abs(d) > double.Epsilon,
            float f => !float.IsNaN(f) && Math.Abs(f) > float.Epsilon,
            int i => i != 0,
            long l => l != 0,
            decimal dec => dec != 0,
            string s => !string.IsNullOrEmpty(s),
            _ => true
        };
    }
}