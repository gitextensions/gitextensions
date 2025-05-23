# Instructions for GitHub and VisualStudio Copilot
### https://github.blog/changelog/2025-01-21-custom-repository-instructions-are-now-available-for-copilot-on-github-com-public-preview/


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

## Xml-doc Comments

* Use 'langword' tag for "true", "false" and "null" values in xml-doc comments.
  For example, use:
    /// <param><see langword="true"/> if the condition is true; otherwise, <see langword="false"/>.</param>

  instead of
    /// <param>true if the condition is true; otherwise, false.</param>

* When adding xml-doc comments, use the `///` syntax and ensure that the comments are properly formatted.
  Also, for multiline comments, add an extra space for additional readability.
  For example, use:
    /// <summary>
    ///  This is API summary.
    /// </summary>

  instead of
    /// <summary>
    /// This is API summary.
    /// </summary>


### **Variable Declarations:**  

*  Never use `var`, always use explicit type declaration.
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
