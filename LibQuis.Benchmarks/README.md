# LibQuis Benchmarks

Performance benchmarking suite for the LibQuis expression parser.

## Running Benchmarks

From the repository root:

```bash
cd LibQuis.Benchmarks
dotnet run -c Release
```

## Benchmark Scenarios

The benchmark suite includes the following scenarios:

### Basic Operations

- **Simple literal**: `42`
- **Simple arithmetic**: `5 + 3 * 2`
- **Simple comparison**: `5 > 3`
- **Boolean logic**: `true && false || true`

### Arithmetic

- **Complex arithmetic**: `(2 + 3) * (4 - 1) + 10 / 2`
- **Nested parentheses**: `((2 + 3) * (4 - 1)) / ((5 + 1) - 2)`

### Variable & Property Access

- **Variable access**: `$health > 50`
- **Property access (dot)**: `$user.age >= 18`
- **Property access (bracket)**: `$user["status"] == "active"`

### Complex Expressions

- **Complex expression**: `$user.age >= 18 && $user.status == "active" && $health > 50`
- **Very complex expression**: Multi-condition with nested logic

### String Operations

- **String literal**: `"Hello World"`
- **String with escapes**: `"Line 1\nLine 2\tTabbed"`

### Advanced Features

- **Custom condition**: `$text custom:contains "Hello"`
- **Shorthand operators**: `5 gt 3 and 10 lte 20 or 15 is not 20`

## Optimization Impact

Key optimizations measured:

1. **Span\<char\>** - Zero-allocation string parsing in tokenizer
2. **ArrayPool** - Buffer reuse for string literals
3. **Reflection caching** - ConcurrentDictionary for property access
4. **StringComparer.OrdinalIgnoreCase** - Optimized keyword lookup
5. **IReadOnlyList** - Collection access patterns
6. **Explicit loops** - Avoiding LINQ overhead in hot paths

## Results Interpretation

- **Mean**: Average execution time
- **Allocated**: Heap memory allocated per operation
- Lower is better for both metrics

Look for:

- Property access should show benefits from reflection caching
- String operations should show minimal allocations (Span + ArrayPool)
- Complex expressions should demonstrate overall optimization gains
