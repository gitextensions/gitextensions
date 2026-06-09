using GitUI.ConsoleEmulation.Mintty;

namespace GitUITests.ConsoleEmulation.Mintty;

[TestFixture]
public sealed class MinttyConsoleRuntimeTests
{
    [Test]
    public void ConvertCommandLineToBash_returns_empty_for_empty_input()
    {
        MinttyConsoleRuntime.ConvertCommandLineToBash("").Should().Be("");
    }

    [Test]
    public void ConvertCommandLineToBash_wraps_single_executable_token()
    {
        MinttyConsoleRuntime.ConvertCommandLineToBash("git").Should().Be("'git'");
    }

    [Test]
    public void ConvertCommandLineToBash_strips_quotes_from_executable_and_keeps_args()
    {
        MinttyConsoleRuntime.ConvertCommandLineToBash(@"""git"" status")
            .Should().Be("'git' 'status'");
    }

    [Test]
    public void ConvertCommandLineToBash_converts_backslashes_in_executable_path_only()
    {
        MinttyConsoleRuntime.ConvertCommandLineToBash(@"""C:\Program Files\Git\bin\git.exe"" status")
            .Should().Be("'C:/Program Files/Git/bin/git.exe' 'status'");
    }

    [Test]
    public void ConvertCommandLineToBash_preserves_backslashes_in_arguments()
    {
        // Argument backslashes must NOT be converted — they may belong to Windows-native
        // paths the target binary needs verbatim (e.g. wsl.exe receiving \\wsl$\... below).
        MinttyConsoleRuntime.ConvertCommandLineToBash(@"""git"" log ""C:\repo\file.txt""")
            .Should().Be(@"'git' 'log' 'C:\repo\file.txt'");
    }

    [Test]
    public void ConvertCommandLineToBash_preserves_wsl_unc_path_for_wsl_repo()
    {
        // Regression test for WSL repos. Previously the raw command line was spliced into
        // a bash script verbatim; bash collapsed the `\\` in the double-quoted UNC path
        // to a single `\` and wsl.exe rejected the malformed argument with E_INVALIDARG.
        string commandLine =
            """
            "wsl" -d Ubuntu-22.04 --cd "\\wsl$\Ubuntu-22.04\home\user\src\WLED" git -c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress "--all" --prune --force
            """.Trim();

        string expected =
            """
            'wsl' '-d' 'Ubuntu-22.04' '--cd' '\\wsl$\Ubuntu-22.04\home\user\src\WLED' 'git' '-c' 'fetch.parallel=0' '-c' 'submodule.fetchjobs=0' 'fetch' '--progress' '--all' '--prune' '--force'
            """.Trim();

        MinttyConsoleRuntime.ConvertCommandLineToBash(commandLine).Should().Be(expected);
    }

    [Test]
    public void ConvertCommandLineToBash_preserves_dollar_sign_in_arguments()
    {
        // `$` must reach the target binary literally; bash would otherwise expand
        // `$VAR` style references inside double quotes.
        MinttyConsoleRuntime.ConvertCommandLineToBash(@"""echo"" ""$HOME and $PATH""")
            .Should().Be("'echo' '$HOME and $PATH'");
    }

    [Test]
    public void ConvertCommandLineToBash_escapes_embedded_single_quote_in_argument()
    {
        // Single quote inside a single-quoted bash string: close, escape, reopen.
        MinttyConsoleRuntime.ConvertCommandLineToBash(@"""git"" commit -m ""it's working""")
            .Should().Be(@"'git' 'commit' '-m' 'it'\''s working'");
    }

    [Test]
    public void ConvertCommandLineToBash_unescapes_backslash_quote_sequence()
    {
        // StringExtensions.Quote() escapes inner `"` as `\"`; the tokenizer must
        // round-trip the literal quote character into the resulting bash token.
        MinttyConsoleRuntime.ConvertCommandLineToBash(@"""git"" commit -m ""say \""hi\""""")
            .Should().Be(@"'git' 'commit' '-m' 'say ""hi""'");
    }

    [Test]
    public void ConvertCommandLineToBash_collapses_runs_of_whitespace_between_tokens()
    {
        MinttyConsoleRuntime.ConvertCommandLineToBash("git    status   --short")
            .Should().Be("'git' 'status' '--short'");
    }

    [Test]
    public void ConvertCommandLineToBash_treats_tab_as_token_separator()
    {
        MinttyConsoleRuntime.ConvertCommandLineToBash("git\tstatus")
            .Should().Be("'git' 'status'");
    }

    [Test]
    public void ConvertCommandLineToBash_keeps_unquoted_executable_intact()
    {
        // Some callers pass an unquoted command (no surrounding `"..."`).
        MinttyConsoleRuntime.ConvertCommandLineToBash(@"C:\tools\bin\app.exe --flag")
            .Should().Be("'C:/tools/bin/app.exe' '--flag'");
    }

    [Test]
    public void ConvertCommandLineToBash_handles_argument_with_spaces_in_quotes()
    {
        MinttyConsoleRuntime.ConvertCommandLineToBash(@"""git"" log --author=""John Doe""")
            .Should().Be("'git' 'log' '--author=John Doe'");
    }

    [Test]
    public void ConvertCommandLineToBash_treats_doubled_backslash_before_closing_quote_as_literal_backslash()
    {
        // A properly escaped trailing backslash in a quoted path is written as `\\"`
        // (two backslashes = one literal backslash, then the closing quote). Without
        // backslash-run counting the closing quote is swallowed and every following
        // token is merged into the path.
        MinttyConsoleRuntime.ConvertCommandLineToBash(@"""git"" status ""C:\dir\\""")
            .Should().Be(@"'git' 'status' 'C:\dir\'");
    }

    [Test]
    public void ConvertCommandLineToBash_does_not_merge_tokens_after_quoted_trailing_backslash()
    {
        // Regression: a quoted argument ending in a backslash must not absorb the
        // arguments that follow it.
        MinttyConsoleRuntime.ConvertCommandLineToBash(@"""git"" -C ""C:\repo\\"" status --short")
            .Should().Be(@"'git' '-C' 'C:\repo\' 'status' '--short'");
    }

    [Test]
    public void ConvertCommandLineToBash_treats_even_backslash_run_before_quote_as_pairs()
    {
        // Four backslashes before a quote = two literal backslashes + a real quote toggle.
        MinttyConsoleRuntime.ConvertCommandLineToBash(@"""git"" ""a\\\\""b")
            .Should().Be(@"'git' 'a\\b'");
    }

    [Test]
    public void ConvertCommandLineToBash_keeps_odd_backslash_run_before_quote_as_escaped_quote()
    {
        // Three backslashes before a quote = one literal backslash + an escaped (literal) quote.
        MinttyConsoleRuntime.ConvertCommandLineToBash(@"""git"" ""a\\\""b""")
            .Should().Be(@"'git' 'a\""b'");
    }
}
