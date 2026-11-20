using System.Globalization;
using System.Text;
using NetSpell.SpellChecker;
using NetSpell.SpellChecker.Dictionary;

namespace GitUITests.SpellChecker;

[TestFixture]
public sealed class SpellingTests
{
    // a word longer than the stack buffers used by the span based lookups
    private static readonly string _longWord = new('a', 300);

    private static readonly string[] _dictionaryLines =
    [
        "[Copyright]",
        "test dictionary",
        "[Try]",
        "abcdefghijklmnopqrstuvwxyz",
        "абвгдеёжзийклмнопрстуфхцчшщъыьэюя",
        "ąćęłńóśźż",
        "",
        "[Replace]",
        "ie ei",
        "ei ie",
        "",
        "[Prefix]",
        "A Y 1",
        "A   0     re         .",
        "P Y 1",
        "P   0     по         .",
        "",
        "[Suffix]",
        "G Y 2",
        "G   e     ing        e",
        "G   0     ing        [^e]",
        "D Y 3",
        "D   y     ied        [^aeiou]y",
        "D   0     d          e",
        "D   0     ed         [^ey]",
        "S Y 3",
        "S   y     ies        [^aeiou]y",
        "S   0     es         [sxzh]",
        "S   0     s          [^sxzhy]",
        "R Y 2",
        "R   а     ой         а",
        "R   а     ами        а",
        "T Y 1",
        "T   0     ami        .",
        "L Y 1",
        "L   0     ler        .",
        "M Y 1",
        "M   0     x\u00ADy        .",
        "",
        "[Words]",
        "walk/GDSA",
        "bake/GDSA",
        "carry/GDSA",
        "receive/GDSA",
        "tie/S",
        "ab",
        "книга/RP",
        "łódź/T",
        "straße",
        "țară/S",
        "isim/L",
        "k/M",
        $"{_longWord}/SA",
    ];

    private string _dictionaryFolder = null!;
    private WordDictionary _dictionary = null!;
    private Spelling _spelling = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dictionaryFolder = Path.Combine(Path.GetTempPath(), "GitExtensionsSpellingTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dictionaryFolder);
        File.WriteAllLines(Path.Combine(_dictionaryFolder, "test.dic"), _dictionaryLines, new UTF8Encoding(false));

        _dictionary = new WordDictionary { DictionaryFolder = _dictionaryFolder, DictionaryFile = "test.dic" };
        _dictionary.Initialize();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _dictionary.Dispose();
        Directory.Delete(_dictionaryFolder, recursive: true);
    }

    [SetUp]
    public void SetUp()
    {
        _spelling = new Spelling { Dictionary = _dictionary };
    }

    [TearDown]
    public void TearDown()
    {
        _spelling.Dispose();
    }

    [TestCase("ab")] // base word
    [TestCase("baking")] // suffix replacing the stripped 'e'
    [TestCase("carried")] // suffix replacing the stripped 'y'
    [TestCase("rewalk")] // prefix
    public void FindWord_should_find_known_word(string word)
    {
        _spelling.FindWord(word.AsSpan(), out string? foundWord).Should().BeTrue();
        foundWord.Should().Be(word);
    }

    [TestCase("")]
    [TestCase("rere")] // prefix only
    [TestCase("ing")] // suffix only
    [TestCase("tieing")] // suffix not allowed for the word
    public void FindWord_should_not_find_unknown_word(string word)
    {
        _spelling.FindWord(word.AsSpan(), out string? foundWord).Should().BeFalse();
        foundWord.Should().BeNull();
    }

    [Test]
    public void FindWord_should_handle_words_longer_than_stack_buffers()
    {
        _spelling.FindWord(_longWord.AsSpan(), out string? foundWord).Should().BeTrue();
        foundWord.Should().Be(_longWord);

        _spelling.FindWord((_longWord + "s").AsSpan(), out foundWord).Should().BeTrue();
        foundWord.Should().Be(_longWord + "s");

        _spelling.FindWord(("re" + _longWord + "s").AsSpan(), out foundWord).Should().BeTrue();
        foundWord.Should().Be("re" + _longWord + "s");

        _spelling.FindWord((_longWord + "z").AsSpan(), out foundWord).Should().BeFalse();
        foundWord.Should().BeNull();
    }

    [TestCase("walk")]
    [TestCase("walking")]
    [TestCase("rewalks")]
    [TestCase("carried")]
    [TestCase("Walk")]
    [TestCase("WALKED")]
    [TestCase("walkz")]
    [TestCase("tieing")]
    [TestCase("")]
    public void FindWord_span_should_match_string_overload(string word)
    {
        string byRef = word;
        bool expectedFound = _spelling.FindWord(ref byRef);

        bool found = _spelling.FindWord(word.AsSpan(), out string? foundWord);

        found.Should().Be(expectedFound);
        if (expectedFound)
        {
            foundWord.Should().Be(byRef);
        }
        else
        {
            foundWord.Should().BeNull();
        }
    }

    [TestCase("recieve", "receive")] // replacement characters
    [TestCase("Wlak", "walk")] // lower case is found
    [TestCase("bakeing", "baking")] // suffix
    public void Suggest_should_suggest_near_miss(string misspelled, string expected)
    {
        _spelling.Suggest(misspelled);

        _spelling.Suggestions.Should().Contain(expected);
    }

    [Test]
    public void Suggest_should_suggest_two_words()
    {
        _spelling.Suggest("walkbake");

        _spelling.Suggestions.Should().Contain("walk bake");
    }

    [Test]
    public void Suggest_should_not_suggest_for_correct_word()
    {
        _spelling.Suggest("walking");

        _spelling.Suggestions.Should().BeEmpty();
    }

    [Test]
    public void Suggest_should_handle_words_longer_than_stack_buffers()
    {
        // one char missing from a word that is only found with its suffix
        _spelling.Suggest(_longWord[1..] + "s");

        _spelling.Suggestions.Should().Contain(_longWord + "s");
    }

    [TestCase("kitten", "sitting", false, ExpectedResult = 3)]
    [TestCase("walk", "walk", false, ExpectedResult = 0)]
    [TestCase("walk", "walk", true, ExpectedResult = 0)]
    [TestCase("abc", "", false, ExpectedResult = 3)]
    [TestCase("", "abc", false, ExpectedResult = 3)]
    [TestCase("cat", "bat", true, ExpectedResult = 2)] // first char differs
    [TestCase("cat", "cab", true, ExpectedResult = 2)] // last char differs
    public int EditDistance_should_return_number_of_edits(string source, string target, bool positionPriority)
    {
        return _spelling.EditDistance(source, target, positionPriority);
    }

    [Test]
    public void EditDistance_should_handle_words_longer_than_stack_buffers()
    {
        string source = new('a', 200);
        string target = new string('a', 200) + "b";

        _spelling.EditDistance(source, target, positionPriority: false).Should().Be(1);
        _spelling.EditDistance(source, target, positionPriority: true).Should().Be(2);
        _spelling.EditDistance(source, new string('b', 200), positionPriority: false).Should().Be(200);
    }

    [Test]
    public void EditDistance_should_be_symmetric()
    {
        string[] words = ["", "a", "walk", "walking", "kitten", "sitting", "receive", "recieve"];

        foreach (string a in words)
        {
            foreach (string b in words)
            {
                _spelling.EditDistance(a, b, positionPriority: false)
                    .Should().Be(_spelling.EditDistance(b, a, positionPriority: false), $"'{a}' vs '{b}'");
            }
        }
    }

    [TestCase("книга")] // Cyrillic
    [TestCase("книгой")] // suffix replacing the stripped 'а'
    [TestCase("книгами")]
    [TestCase("покнигой")] // prefix and suffix
    [TestCase("łódź")] // Polish
    [TestCase("łódźami")]
    [TestCase("straße")] // German
    [TestCase("țară")] // Romanian
    [TestCase("țarăs")]
    [TestCase("isim")] // Turkish
    [TestCase("isimler")]
    public void FindWord_should_find_non_ascii_word_in_any_culture(string word)
    {
        InEachCulture(culture =>
        {
            _spelling.FindWord(word.AsSpan(), out string? foundWord).Should().BeTrue($"culture {culture}");
            foundWord.Should().Be(word, $"culture {culture}");
        });
    }

    [TestCase("КНИГА", "книга")]
    [TestCase("КНИГОЙ", "книгой")]
    [TestCase("ПОКНИГОЙ", "покнигой")]
    [TestCase("Łódź", "łódź")]
    [TestCase("ŁÓDŹAMI", "łódźami")]
    [TestCase("STRAßE", "straße")]
    [TestCase("Țară", "țară")]
    [TestCase("ISIM", "isim")] // invariant lower casing: 'I' is never the dotless 'ı'
    [TestCase("ISIMLER", "isimler")]
    public void FindWord_should_return_lower_case_non_ascii_word_in_any_culture(string word, string expected)
    {
        InEachCulture(culture =>
        {
            _spelling.FindWord(word.AsSpan(), out string? foundWord).Should().BeTrue($"culture {culture}");
            foundWord.Should().Be(expected, $"culture {culture}");
        });
    }

    [TestCase("книгаа")]
    [TestCase("КНИГАА")]
    [TestCase("łódźm")]
    [TestCase("STRASSE")] // lower case 'ss' is not 'ß'
    [TestCase("isimle\u00ADr")] // ordinal comparison does not ignore the soft hyphen when matching the suffix
    [TestCase("ISIMLE\u00ADR")]
    [TestCase("kaxy")] // a culture sensitive comparison would match the suffix "x<soft hyphen>y" and strip 3 chars leaving the word "k"
    public void FindWord_should_not_find_unknown_non_ascii_word_in_any_culture(string word)
    {
        InEachCulture(culture =>
        {
            _spelling.FindWord(word.AsSpan(), out string? foundWord).Should().BeFalse($"culture {culture}");
            foundWord.Should().BeNull($"culture {culture}");
        });
    }

    [TestCase("lódź", "łódź")] // wrong char
    [TestCase("книшга", "книга")] // extra char
    [TestCase("кнега", "книга")] // wrong char
    [TestCase("кнга", "книга")] // forgotten char
    [TestCase("кинга", "книга")] // swapped chars
    public void Suggest_should_suggest_non_ascii_near_miss_in_any_culture(string misspelled, string expected)
    {
        InEachCulture(culture =>
        {
            _spelling.Suggest(misspelled);

            _spelling.Suggestions.Should().Contain(expected, $"culture {culture}");
        });
    }

    [TestCase("книга", "книги", false, ExpectedResult = 1)]
    [TestCase("Книга", "книга", false, ExpectedResult = 1)]
    [TestCase("Книга", "книга", true, ExpectedResult = 1)] // position priority ignores case, the distance does not
    [TestCase("łódź", "lodz", false, ExpectedResult = 3)]
    [TestCase("łódź", "lodz", true, ExpectedResult = 5)] // first and last char differ
    public int EditDistance_should_count_non_ascii_chars(string source, string target, bool positionPriority)
    {
        return _spelling.EditDistance(source, target, positionPriority);
    }

    private static void InEachCulture(Action<string> test)
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUICulture = CultureInfo.CurrentUICulture;
        try
        {
            foreach (string name in new[] { "en-US", "tr-TR", "ru-RU", "pl-PL", "ro-RO", "de-DE", "ja-JP" })
            {
                CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo(name);
                test(name);
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUICulture;
        }
    }
}
