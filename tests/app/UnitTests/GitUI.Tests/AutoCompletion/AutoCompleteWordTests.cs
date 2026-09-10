using GitUI.AutoCompletion;
using NUnit.Framework;

namespace GitUITests.AutoCompletion;

[TestFixture]
public class AutoCompleteWordTests
{
    [TestCase("CamelHump", "CH", ExpectedResult = true)]
    [TestCase("CamelHump", "ch", ExpectedResult = true)]
    [TestCase("CamelHump", "cH", ExpectedResult = true)]
    [TestCase("CamelHump", "Ch", ExpectedResult = true)]
    [TestCase("CamelHump", "Ca", ExpectedResult = true)]
    [TestCase("CamelHump", "ca", ExpectedResult = true)]
    [TestCase("CamelHump", "CamelH", ExpectedResult = true)]
    [TestCase("CamelHump", "CX", ExpectedResult = false)]
    [TestCase("MyCamelHump", "MCH", ExpectedResult = true)]
    [TestCase("MyCamelHump", "mch", ExpectedResult = true)]
    [TestCase("MyCamelHump", "CH", ExpectedResult = false)]
    [TestCase("Identifier", "i", ExpectedResult = true)]
    [TestCase("Identifier", "I", ExpectedResult = true)]
    [TestCase("Identifier", "id", ExpectedResult = true)]
    [TestCase("Identifier", "Id", ExpectedResult = true)]
    public bool Matches_should_match_prefix_and_camel_humps_case_insensitive(string word, string typedWord)
    {
        var autoCompleteWord = new AutoCompleteWord(word);
        return autoCompleteWord.Matches(typedWord);
    }
}
