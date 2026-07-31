using GitUI.AI;

namespace GitUITests.AI;

[TestFixture]
public sealed class DeterministicDiffNoiseTests
{
    private static string Diff(params string[] changedLines)
    {
        // Minimal unified-diff body: headers + a hunk with the given +/- lines.
        List<string> lines =
        [
            "diff --git a/file b/file",
            "index 111..222 100644",
            "--- a/file",
            "+++ b/file",
            "@@ -1,3 +1,3 @@",
            .. changedLines
        ];
        return string.Join('\n', lines);
    }

    [TestCase(".py")]
    [TestCase(".pyi")]
    [TestCase(".yaml")]
    [TestCase(".yml")]
    [TestCase(".fs")]
    [TestCase(".txt")]
    [TestCase("")]
    public void IsWhitespaceInsignificant_is_false_for_significant_or_unknown(string ext)
        => DeterministicDiffNoise.IsWhitespaceInsignificant($"file{ext}").Should().BeFalse();

    [TestCase(".cs")]
    [TestCase(".java")]
    [TestCase(".ts")]
    [TestCase(".cpp")]
    [TestCase(".go")]
    [TestCase(".json")]
    public void IsWhitespaceInsignificant_is_true_for_insignificant(string ext)
        => DeterministicDiffNoise.IsWhitespaceInsignificant($"file{ext}").Should().BeTrue();

    [Test]
    public void DiffHasContentChanges_detects_hunk_lines_but_ignores_headers()
    {
        DeterministicDiffNoise.DiffHasContentChanges(null).Should().BeFalse();
        DeterministicDiffNoise.DiffHasContentChanges("").Should().BeFalse();
        DeterministicDiffNoise.DiffHasContentChanges("diff --git a/f b/f\n--- a/f\n+++ b/f\n").Should().BeFalse();
        DeterministicDiffNoise.DiffHasContentChanges(Diff("+real change")).Should().BeTrue();
        DeterministicDiffNoise.DiffHasContentChanges(Diff("-removed")).Should().BeTrue();
    }

    [Test]
    public void IsImportOnly_true_for_csharp_using_directives()
    {
        string diff = Diff("+using System.Linq;", "-using System.Text;", "+global using System;", "+using static System.Math;", "+using Alias = System.Collections.Generic.List<int>;");
        DeterministicDiffNoise.IsImportOnly("Foo.cs", diff).Should().BeTrue();
    }

    [Test]
    public void IsImportOnly_false_for_csharp_using_statement_and_declaration()
    {
        DeterministicDiffNoise.IsImportOnly("Foo.cs", Diff("+using (var x = Open()) { }")).Should().BeFalse();
        DeterministicDiffNoise.IsImportOnly("Foo.cs", Diff("+using var x = Open();")).Should().BeFalse();
    }

    [Test]
    public void IsImportOnly_false_when_a_real_change_is_present()
    {
        string diff = Diff("+using System.Linq;", "+var x = 1;");
        DeterministicDiffNoise.IsImportOnly("Foo.cs", diff).Should().BeFalse();
    }

    [Test]
    public void IsImportOnly_ignores_blank_line_changes()
    {
        string diff = Diff("+using System.Linq;", "+", "-");
        DeterministicDiffNoise.IsImportOnly("Foo.cs", diff).Should().BeTrue();
    }

    [Test]
    public void IsImportOnly_true_for_python_imports()
    {
        string diff = Diff("+import os", "+import sys, re", "+from collections import OrderedDict", "+from . import utils", "-import json as j");
        DeterministicDiffNoise.IsImportOnly("mod.py", diff).Should().BeTrue();
    }

    [Test]
    public void IsImportOnly_false_for_python_lookalike_identifiers()
    {
        DeterministicDiffNoise.IsImportOnly("mod.py", Diff("+import_count = 5")).Should().BeFalse();
        DeterministicDiffNoise.IsImportOnly("mod.py", Diff("+from_address = get()")).Should().BeFalse();
    }

    [Test]
    public void IsImportOnly_true_for_java_imports()
    {
        string diff = Diff("+import java.util.List;", "-import java.io.*;", "+import static java.lang.Math.PI;");
        DeterministicDiffNoise.IsImportOnly("Foo.java", diff).Should().BeTrue();
    }

    [Test]
    public void IsImportOnly_true_for_js_ts_imports()
    {
        string diff = Diff(
            "+import foo from './foo';",
            "+import { a, b } from 'pkg';",
            "+import * as ns from 'pkg';",
            "+import 'side-effect';",
            "+export { x } from './x';",
            "-const y = require('legacy');");
        DeterministicDiffNoise.IsImportOnly("app.ts", diff).Should().BeTrue();
    }

    [Test]
    public void IsImportOnly_false_for_js_dynamic_import_expression()
    {
        DeterministicDiffNoise.IsImportOnly("app.ts", Diff("+const m = await import('x');")).Should().BeFalse();
        DeterministicDiffNoise.IsImportOnly("app.ts", Diff("+import('x').then(run);")).Should().BeFalse();
    }

    [Test]
    public void IsImportOnly_false_for_unsupported_language()
        => DeterministicDiffNoise.IsImportOnly("main.go", Diff("+import \"fmt\"")).Should().BeFalse();
}
