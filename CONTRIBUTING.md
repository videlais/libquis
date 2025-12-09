# Contributing to LibQuis

First off, thanks for taking the time to contribute! 🎉

The following is a set of guidelines for contributing to LibQuis. These are mostly guidelines, not rules. Use your best judgment, and feel free to propose changes to this document in a pull request.

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Building the Project

1. Clone the repository:

   ```bash
   git clone https://github.com/videlais/libquis.git
   cd libquis
   ```

2. Build the solution:

   ```bash
   dotnet build
   ```

### Running Tests

We use xUnit for testing. To run the test suite:

```bash
dotnet test
```

### Running Benchmarks

If you are making performance-sensitive changes, please run the benchmarks to ensure no regressions:

```bash
cd LibQuis.Benchmarks
dotnet run -c Release
```

## How to Contribute

### Reporting Bugs

This section guides you through submitting a bug report for LibQuis.

- **Use a clear and descriptive title** for the issue to identify the problem.
- **Describe the exact steps to reproduce the problem** in as much detail as possible.
- **Provide specific examples** to demonstrate the steps.

### Pull Requests

1. Fork the repo and create your branch from `main`.
2. If you've added code that should be tested, add tests.
3. If you've changed APIs, update the documentation.
4. Ensure the test suite passes.
5. Make sure your code follows the existing code style.

## Style Guide

- We follow standard C# coding conventions.
- Use `var` when the type is obvious.
- Use `async`/`await` for I/O bound operations.
- Prefer `Span<T>` and `Memory<T>` for performance-critical paths.

## License

By contributing, you agree that your contributions will be licensed under its MIT License.
