using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitUI;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using NUnit.Framework;

namespace GitUITests.Editor
{
    [Apartment(ApartmentState.STA)]
    [TestFixture]
    public class FindAndReplaceFormTests
    {
        private FindAndReplaceForm _findAndReplaceForm;
        private FindAndReplaceForm.TestAccessor _testAccessor;
        private TextEditorControl _textEditorControl;

        [SetUp]
        public void SetUp()
        {
            _findAndReplaceForm = new FindAndReplaceForm();
            _textEditorControl = new TextEditorControl();
            _testAccessor = _findAndReplaceForm.GetTestAccessor();
        }

        [TearDown]
        public void TearDown()
        {
            _findAndReplaceForm.Dispose();
            _textEditorControl.Dispose();
        }

        public static IEnumerable<TestCaseData> MatchCase
        {
            get
            {
                yield return new TestCaseData("line one", "one", false, new TextRange(5, 3));
                yield return new TestCaseData("line one", "ONE", false, new TextRange(5, 3));
                yield return new TestCaseData("line one", "one", true, new TextRange(5, 3));
                yield return new TestCaseData("line one", "ONE", true, null);
            }
        }

        [Test, TestCaseSource(nameof(MatchCase))]
        public async Task FindNextAsync_match_case(string text, string searchPhrase, bool matchCase, TextRange expectedRange)
        {
            Arrange(text, searchPhrase, matchCase);

            var actualRange = await _findAndReplaceForm.FindNextAsync(false, false, null);

            AssertTextRange(expectedRange, actualRange);
        }

        public static IEnumerable<TestCaseData> MatchWholeWordOnly
        {
            get
            {
                yield return new TestCaseData("line one", "one", new TextRange(5, 3));
                yield return new TestCaseData("line one", "on", null);
            }
        }

        [Test, TestCaseSource(nameof(MatchWholeWordOnly))]
        public async Task FindNextAsync_match_whole_world_only(string text, string searchPhrase, TextRange expectedRange)
        {
            Arrange(text, searchPhrase, matchWholeWordOnly: true);

            var actualRange = await _findAndReplaceForm.FindNextAsync(false, true, null);

            AssertTextRange(expectedRange, actualRange);
        }

        public static IEnumerable<TestCaseData> LoopAround
        {
            get
            {
                const string textToSearch = "line odd\r\nline even\r\nline odd";

                // Search entire document, both directions
                yield return new TestCaseData(textToSearch, "odd", false, default, default, new[]
                {
                    new TextRange(5, 3),
                    new TextRange(26, 3),
                    new TextRange(5, 3),
                });

                yield return new TestCaseData(textToSearch, "odd", true, default, default, new[]
                {
                    new TextRange(26, 3),
                    new TextRange(5, 3),
                    new TextRange(26, 3),
                });

                // Search scan region, both directions
                yield return new TestCaseData(textToSearch, "line", false, new TextLocation(0, 1), new TextLocation(5, 2), new[]
                {
                    new TextRange(10, 4),
                    new TextRange(21, 4),
                    new TextRange(10, 4),
                });

                yield return new TestCaseData(textToSearch, "line", true, new TextLocation(0, 1), new TextLocation(5, 2), new[]
                {
                    new TextRange(21, 4),
                    new TextRange(10, 4),
                    new TextRange(21, 4),
                });
            }
        }

        [Test, TestCaseSource(nameof(LoopAround))]
        public async Task FindNextAsync_should_make_a_loop_and_return_to_first_occurrence(
            string text,
            string searchPhrase,
            bool searchBackwards,
            TextLocation scanRegionStart,
            TextLocation scanRegionEnd,
            IEnumerable<TextRange> expectedRanges)
        {
            Arrange(text, searchPhrase, scanRegionStart: scanRegionStart, scanRegionEnd: scanRegionEnd);

            foreach (TextRange expectedRange in expectedRanges)
            {
                var actualRange = await _findAndReplaceForm.FindNextAsync(false, searchBackwards, null);

                AssertTextRange(expectedRange, actualRange);
            }
        }

        [Test]
        public async Task FindNextAsync_pressing_f3_outside_of_scan_region_should_clear_it()
        {
            Arrange("line one\r\nline two\r\nline three",
                "line",
                scanRegionStart: new TextLocation(0, 0),
                scanRegionEnd: new TextLocation(0, 1));

            var actualRange = await _findAndReplaceForm.FindNextAsync(false, false, null);
            AssertTextRange(new TextRange(0, 4), actualRange);

            // Move the caret outside of the originally selected region.
            var newCaretPosition = new TextLocation(1, 1);
            _textEditorControl.ActiveTextAreaControl.Caret.Position = newCaretPosition;

            actualRange = await _findAndReplaceForm.FindNextAsync(true, false, null);
            AssertTextRange(new TextRange(20, 4), actualRange);
        }

        public static IEnumerable<TestCaseData> MultiFileSearch
        {
            get
            {
                // Should find occurrences in both files
                yield return new TestCaseData(
                    new[] { "line one\r\nline two\r\nline three", "content one\r\ncontent two\r\ncontent three" },
                    "two",
                    default,
                    default,
                    new[] { new TextRange(15, 3), new TextRange(21, 3), new TextRange(15, 3) });

                // Has scan region, should search the first file only
                yield return new TestCaseData(
                    new[] { "line one\r\nline two\r\nline three", "content one\r\ncontent two\r\ncontent three" },
                    "two",
                    new TextLocation(0, 1),
                    new TextLocation(0, 2),
                    new[] { new TextRange(15, 3), new TextRange(15, 3) });
            }
        }

        [Test, TestCaseSource(nameof(MultiFileSearch))]
        public async Task FindNextAsync_should_iterate_over_files(
            string[] texts,
            string searchPhrase,
            TextLocation scanRegionStart,
            TextLocation scanRegionEnd,
            IEnumerable<TextRange> expectedRanges)
        {
            int currentIndex = 0;

            bool FileLoader(bool backward, bool loop, out int index, out Task content)
            {
                currentIndex = (currentIndex + 1) % texts.Length;
                index = currentIndex;
                content = Task.CompletedTask;
                _textEditorControl.Text = texts[currentIndex];

                return true;
            }

            Arrange(texts.First(), searchPhrase, scanRegionStart: scanRegionStart, scanRegionEnd: scanRegionEnd, fileLoader: FileLoader);

            foreach (TextRange expectedRange in expectedRanges)
            {
                var actualRange = await _findAndReplaceForm.FindNextAsync(false, false, null);

                AssertTextRange(expectedRange, actualRange);
            }
        }

        [Test]
        public void FindAndReplaceForm_scan_region_clears_if_new_text_was_set()
        {
            Arrange("line 1\r\nline 2\r\nline 3", "line",
                scanRegionStart: new TextLocation(0, 1),
                scanRegionEnd: new TextLocation(0, 2));

            Assert.IsTrue(_testAccessor.Search.HasScanRegion);

            _textEditorControl.Text = "new text";

            Assert.IsFalse(_testAccessor.Search.HasScanRegion);
        }

        private void Arrange(string text,
            string searchPhrase,
            bool matchCase = false,
            bool matchWholeWordOnly = false,
            TextLocation scanRegionStart = default,
            TextLocation scanRegionEnd = default,
            GetNextFileFnc fileLoader = null)
        {
            _textEditorControl.Text = text;
            _testAccessor.SetEditor(_textEditorControl);
            _testAccessor.TxtLookFor.Text = searchPhrase;
            _testAccessor.ChkMatchCase.Checked = matchCase;
            _testAccessor.ChkMatchWholeWord.Checked = matchWholeWordOnly;
            _findAndReplaceForm.SetFileLoader(fileLoader);

            if (scanRegionStart != default || scanRegionEnd != default)
            {
                var selection = new DefaultSelection(_textEditorControl.Document, scanRegionStart, scanRegionEnd);
                _textEditorControl.ActiveTextAreaControl.SelectionManager.SetSelection(selection);
                _textEditorControl.ActiveTextAreaControl.Caret.Position = scanRegionEnd;
                _testAccessor.Search.SetScanRegion(selection);
            }
        }

        private void AssertTextRange(TextRange expectedRange, TextRange actualRange)
        {
            // Assert returned value
            Assert.That(actualRange, Is.EqualTo(expectedRange).Using(new SegmentComparer()));

            if (expectedRange == null)
            {
                return;
            }

            // Assert text selected
            var actualSelection = _textEditorControl.ActiveTextAreaControl.SelectionManager.SelectionCollection.Single();
            Assert.AreEqual(expectedRange.Offset, actualSelection.Offset);
            Assert.AreEqual(expectedRange.Length, actualSelection.Length);

            // Assert caret is at the end of the found range.
            Assert.AreEqual(expectedRange.Offset + expectedRange.Length, _textEditorControl.ActiveTextAreaControl.Caret.Offset);
        }
    }
}
