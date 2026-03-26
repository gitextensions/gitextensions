# Instructions for GitHub and VisualStudio Copilot

<https://github.blog/changelog/2025-01-21-custom-repository-instructions-are-now-available-for-copilot-on-github-com-public-preview/>

## General

* Make only high confidence suggestions when reviewing code changes.
* Always use the latest version C#, currently C# 13 features.
* Files must have CRLF line endings.
* When creating or modifying code, follow all StyleCop analyzer rules.
* Test changes locally or verify StyleCop compliance before committing when possible.

## Comments

* Add const with speaking name instead of using magic numbers or repeated string literals. Avoid hard-to-maintain comments which contain the magic number again.
* When a string literal is used more than once in a class, extract it into a `const` field with a descriptive name.
* Omit comments which just describe _what_ is done. In situations where a comment may be necessary describe _why_ an implementation was chosen.

## XML Doc Comments

* Use XML documentation comments for non-private APIs, including properties, methods, and classes. Do not add XML documentation comments to private members.
* Do not add `/// <inheritdoc />` nor other xmldoc when just implementing interface or abstract members.
* Do not add XML documentation comments to `TestAccessor` structs or their members.
* Use correct xmldoc tags, as well as "see", "langword", and "paramref" tags where appropriate.
* Use single-line tags for simple XML comments, such as for the `<returns>` tag.
* When adding multi-line XML documentation comments, add an extra space after the `///` *within* the tags to ensure proper formatting and readability. However, do not add a space after the opening `///` tag in the single-line comments.
For example:
```csharp
/// <summary>
///  This is a summary of the method.
/// </summary>
/// <param name="parameterName">This is a description of the parameter.</param>
```

## Formatting

* Apply code-formatting style defined in `.editorconfig`.
* Prefer file-scoped namespace declarations and single-line `using` declarations.
* Insert a newline before the opening curly brace of any code block (e.g., after `if`, `for`, `while`, `foreach`, `using`, `try`, etc.).
* Ensure that the final return statement of a method is on its own line.
* Use pattern matching and switch expressions wherever possible.
* Use `nameof` instead of string literals when referring to code symbols.
* Local methods must be
    - placed at the end of the parent method,
    - sorted in alphabetical order,
    - preceded with a `return` statement.
  For example:
  ```cs
  private void MyMethod()
  {
      // implementation

      return;

      static void FirstLocalMethod1() { ... }

      static int SecondLocalMethod1() { ... }
  }
  ```
* Do not add trailing whitespace to any lines (StyleCop SA1028).
* Add blank lines after closing braces when required (StyleCop SA1513).
* Prefer C# raw (`"""`) string literals for multi-line strings.

## Variable Declarations

* Never use `var` for primitive types. Use `var` only when the type is obvious from context. When in doubt, opt for an explicit type declaration.
* Prefer simplified "new()" construct over to "new MyType()" where the type is already declared.
  For example: "MyType t = new()" instead of "var t = new MyType()" or "MyType t = new MyType()".

## Nullable Reference Types

* Declare variables non-nullable, and check for `null` at entry points.
* Always use `is null` or `is not null` instead of `== null` or `!= null`.
* Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.

## Testing

* We use NUnit SDK.
* Do not emit "Act", "Arrange" or "Assert" comments.
* The test names must follow snake-casing in the suffix BUT keeping the methods under test intact.
  For example, a test for a method "MyMethod" should be named as "MyMethod_should_return_expected".
* Do not repeat in a comment what the test name already expresses.
* Use `NSubstitute` for mocking.
* Use `FluentAssertions` for assertions, i.e. do not use `ClassicAssert`.

## Commit Messages

* Use https://www.conventionalcommits.org/en/v1.0.0/ for commit messages.
* Note especially that changes in directory src/app/GitExtensions.Extensibility affects the version for the plugin interface. This must be annotated in the commit message.

## Git Commands

* All git executable invocations should be invoked through `IGitModule`, with unit tests added.
* Structure git command arguments using `Commands` (in `GitCommands.Git`), which returns `IGitCommand` instances via the private `GitCommand` record. `IGitCommand` declares whether the command accesses a remote and whether it changes repo state.
* Execute commands with UI feedback through `GitUICommands` (implements `IGitUICommands`). Use `StartCommandLineProcessDialog(owner, IGitCommand)` to run a structured command — it automatically selects the right process dialog (remote vs local) and fires `RepoChangedNotifier` based on `IGitCommand.ChangesRepoState`. For operations that need a dedicated form, use the existing `Start*Dialog` methods on `GitUICommands`.
* Prefer the `-z` flag when available (e.g. `git worktree list`, `git status`) to use NUL-delimited output, which avoids issues with newlines or special characters in paths.
* When parsing NUL or line-delimited git output that contains key-value pairs (e.g. `worktree /path/to/dir`), split on the first space only to handle values that contain spaces.
