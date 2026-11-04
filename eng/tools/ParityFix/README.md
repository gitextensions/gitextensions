# ParityFix

Temporary source-parity tooling for reviewed `ParityInventory` schema 5 `types/*.json` reports.
Targets plain `net10.0`; belongs to neither application solution. Run from the repository root.

```powershell
dotnet build eng/tools/ParityFix
dotnet artifacts/Debug/bin/ParityFix/net10.0/ParityFix.dll `
  --inventory eng/avalonia/parity-evidence/gates/latest-inventory/types `
  --ownership eng/avalonia/parity-evidence/P8.6i/application-shell-126/closure-ownership.json `
  --owner progress126 `
  --report eng/avalonia/parity-evidence/tier1-fix-pilot/preview.json
```

The default command previews changes. Add `--apply` to write proved candidate files. Original files,
linked sources outside the twin root, AXAML, cross-part moves and non-mechanical findings are excluded.
`--twin-root` can select another explicit twin project or mapped plugin directory. It must match each
selected report root or contain every twin part in that report. Snapshot paths retain their recorded
root for validation; only writes use the explicit boundary. Linked unchanged originals remain protected.

Before processing, the tool checks the live member and comment facts against every selected report.
Stale facts or an unsupported schema fail before any writes. These inventories do not contain whole-file
hashes, so this is a fact check, not proof of an unchanged checkout. Regenerate the reviewed inventory
after applying changes and before another pass.

Comments are copied verbatim, with indentation adapted to the target line. A leading comment requires
a unique twin member anchor, even when its body differs. Framework parameter changes can match by
member name only when that anchor is unique on both sides. Body comments use the nearest equivalent,
unique statement, looking forward or backward within the member. The statement and the comment must
share their branch, lambda and local-function context. Changed comments additionally require
an unchanged containing type and unchanged XML references: framework adaptations remain for hand review.
Inline, ambiguous and missing anchors are reported without guessing. The entire file must remain Roslyn
`SyntaxFactory.AreEquivalent` after comment edits; overlapping edits or a failed proof leave it untouched.

Proposals require reading the original and twin methods: syntax equivalence cannot establish whether
the words still describe the code. Reject a misplaced proposal or correct its location before committing.
For manual restoration, preserve original wording and change only facts that are false for the twin.
An unmatched anchor is not evidence that its corresponding behavior no longer exists.

```powershell
dotnet artifacts/Debug/bin/ParityFix/net10.0/ParityFix.dll prove `
  --report eng/avalonia/parity-evidence/comment-proof.json
```

`prove` independently compares the complete repository working-tree delta with HEAD, including staged
and unstaged edits and nonignored untracked paths, even when invoked from a subdirectory. Only the exact
paths `eng/avalonia/portmap.json` and `eng/avalonia/parity-ledger.json` are exempt and listed separately
in the report. The existing `eng/avalonia/framework-adaptations.json` is proved separately: every HEAD
entry must remain present and unchanged. Additions must be exact `comment.missing` or
`comment.drifted` pins with reason `codeAbsent` or `codeMoved`, or `comment.changed` pins with reason
`adaptedMinimal` holding both the original and twin text and part. New member/event pins, edited or removed
entries, other manifest-property changes, duplicate identities/properties and malformed JSON fail with
a diagnostic identifying the rejected change. The inventory gate still verifies live source/destination
facts. Finish every placement and pin before proof, so it covers the final tree.

```powershell
pwsh eng/avalonia/gate.ps1 -Level Batch -CommentOnly
pwsh eng/avalonia/gate.ps1 -Verify
```

Comment-only Batch mode runs this proof first and refuses any unproved path before product builds or
inventory. It then builds the Windows Avalonia solution (including XML documentation checks) and runs
the reviewed inventory without inventory tests. All test and Linux legs are skipped with explicit
receipt reasons. Proof, build and inventory are never reused in this mode; Verify accepts its passing
receipt only for that exact tree. An ordinary Batch cannot reuse a comment-only receipt's legs.
Product or test code changes require the ordinary gate and the whole test assembly on both platforms.

Every other changed path must be an existing regular C# file with equivalent syntax,
unchanged directives, unchanged disabled preprocessor text and unchanged file mode. Additions, deletions,
other formats and declaration order changes fail. Inactive `#if` blocks are compared as text, including
their comments. Git LF and working-copy CRLF are normalized for comparison. There is no directory option
for `prove`; `--twin-root` applies only to placement. A report should go to an ignored evidence directory
or outside the repository. This proof supports comments placed by hand without an inventory report;
it exits nonzero for any unproved path and does not prove comment meaning or any complete parity axis.

Member order changes stay inside a partial and cannot cross directive-bearing members, except methods
whose only directives are internal adjacent warning-disable/restore pairs for the same explicit codes.
These pairs can move with their complete member only when the file has no ambient warning directives;
their exact text remains protected by the member-multiset proof. Regions and conditional scopes stay barriers. Unknown members
keep their slots; overloads and other ambiguous declarations remain in place. The full member multiset,
including trivia, and syntax outside the type must be unchanged after reordering and reparsing. Moved
fields, events and properties with initializers are listed and block that file's order changes by default.
After hand review, `--allow-initializers` permits those proposals; the list remains in the report. Compilation
and tests are still required because declaration initializer order can affect behavior.

The report distinguishes restored and unplaced comments, original order findings, reordered files and
unresolved operations. Only a fresh reviewed inventory measures actual closure: moved nodes do not imply
that every order finding closed. No parity axis, adaptation pin or ledger status is changed by this tool.

```powershell
pwsh eng/avalonia/gate.ps1 -Level Batch `
  -Filter 'FullyQualifiedName~SubmoduleNodeStyleStateTests' `
  -ToolTestProject eng/tools/ParityFix.Tests/ParityFix.Tests.csproj
```

The reusable gate builds and tests this standalone project on Windows and in the native-ext4 Linux
mirror, in addition to its application and inventory checks. Unit cases cover comment kinds, declaration
and statement anchors, drift removal, control-flow differences, framework XML references, order proofs,
initializer review, directives, ambiguous overloads, stale facts, write boundaries and encoding.
The dashboard regression cases run only on Windows because they execute the Windows gate's PowerShell
wrapper; the fixer and Git proof cases run on Windows and Linux.
