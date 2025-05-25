# Instructions for GitHub and VisualStudio Copilot

<https://github.blog/changelog/2025-01-21-custom-repository-instructions-are-now-available-for-copilot-on-github-com-public-preview/>

## General

* Make only high confidence suggestions when reviewing code changes.
* Always use the latest version C#, currently C# 13 features.
* Files must have CRLF line endings.

## Formatting

* Apply code-formatting style defined in `.editorconfig`.
* Prefer file-scoped namespace declarations and single-line using directives.
* Insert a newline before the opening curly brace of any code block (e.g., after `if`, `for`, `while`, `foreach`, `using`, `try`, etc.).
* Ensure that the final return statement of a method is on its own line.
* Use pattern matching and switch expressions wherever possible.
* Use `nameof` instead of string literals when referring to member names.

### Variable Declarations

* Never use `var` for primitive types. Use `var` only when the type is obvious from context. When in doubt, opt for an explicit type declaration.
* Prefer simplified "new()" construct over to "new MyType()" where the type is already declared.
  For example: "MyType t = new()" instead of "var t = new MyType()" or "MyType t = new MyType()".

### Nullable Reference Types

* Declare variables non-nullable, and check for `null` at entry points.
* Always use `is null` or `is not null` instead of `== null` or `!= null`.
* Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.

### Testing

* We use NUnit SDK
* Do not emit "Act", "Arrange" or "Assert" comments.
* The test names must follow snake-casing in the suffix BUT keeping the methods under test intact.
  For example, a test for a method "MyMethod" should be named as "MyMethod_should_return_expected".
* Use NSubstitute for mocking.
* Use FluentAssertions for assertions.
