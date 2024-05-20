using FluentAssertions;
using GitExtensions.Extensibility.Git;
using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;
using NSubstitute;
using static GitUI.ScriptsEngine.ScriptsManager;

namespace GitUITests.Script;

[SetCulture("en-US")]
[SetUICulture("en-US")]
[TestFixture]
public class ScriptsManagerScriptRunnerTests
{
    private IGitUICommands _commands;
    private IGitModule _module;
    private IScriptOptionsProvider _scriptOptionsProvider;

    [SetUp]
    public void Setup()
    {
        _module = Substitute.For<IGitModule>();
        _scriptOptionsProvider = Substitute.For<IScriptOptionsProvider>();

        _commands = Substitute.For<IGitUICommands>();
        _commands.Module.Returns(_module);
    }

    [Test]
    public void Parse_should_parse_distincts_userInput()
    {
        _commands.GetService<ISimplePromptCreator>().Returns(new SimplePromptCreatorForTest(("input label1", "value", "foo1"), ("input label2", "value2", "bar1"), (null, string.Empty, "foo2")));
        (string? arguments, bool abort, bool cancel) result = ScriptRunner.ParseUserInputs("script_name", "{UserInput:input label1=value}_{{UserInput:input label2=value2}}_{UserInput}_{{UserInput}}", _commands,
            owner: null, scriptOptionsProvider: _scriptOptionsProvider);

        result.abort.Should().Be(false);
        result.arguments.Should().Be(@"foo1_""bar1""_foo2_""foo2""");
    }

    [Test]
    public void Parse_should_parse_customized_userInput_without_default_value_specified()
    {
        _commands.GetService<ISimplePromptCreator>().Returns(new SimplePromptCreatorForTest(("input label1", string.Empty, "foo1")));
        (string? arguments, bool abort, bool cancel) result = ScriptRunner.ParseUserInputs("script_name", "{UserInput:input label1}", _commands,
            owner: null, scriptOptionsProvider: _scriptOptionsProvider);

        result.abort.Should().Be(false);
        result.arguments.Should().Be(@"foo1");
    }

    [Test]
    public void Parse_should_parse_customized_userInputs_and_replace_all_with_same_label()
    {
        _commands.GetService<ISimplePromptCreator>().Returns(new SimplePromptCreatorForTest(("input label1", "value", "foo1")));
        (string? arguments, bool abort, bool cancel) result = ScriptRunner.ParseUserInputs("script_name", "{UserInput:input label1=value}_{UserInput:input label1}_{{UserInput:input label1}}", _commands,
            owner: null, scriptOptionsProvider: _scriptOptionsProvider);

        result.abort.Should().Be(false);
        result.arguments.Should().Be(@"foo1_foo1_""foo1""");
    }

    [Test]
    public void Parse_should_parse_userInput_with_default_value_as_expression_containing_arguments_inside()
    {
        _module.GetRevision(null, Arg.Any<bool>(), Arg.Any<bool>()).Returns(new GitRevision(ObjectId.Parse("79b9792ca4db3d01d7c0f2cd95419dd53665ec41")));
        _commands.GetService<ISimplePromptCreator>().Returns(new SimplePromptCreatorForTest(("input label1", "file_79b9792ca4db3d01d7c0f2cd95419dd53665ec41.bak", "file_foo.bak")));
        (string? arguments, bool abort, bool cancel) result = ScriptRunner.ParseUserInputs("script_name", "{UserInput:input label1=file_{HEAD}.bak}", _commands,
            owner: null, scriptOptionsProvider: _scriptOptionsProvider);

        result.abort.Should().Be(false);
        result.arguments.Should().Be(@"file_foo.bak");
    }

    private class SimplePromptCreatorForTest : ISimplePromptCreator
    {
        private readonly (string label, string defaultValue, string returnedValue)[] _values;
        private int _indexOfNextFakeValueReturned = 0;

        public SimplePromptCreatorForTest(params (string title, string defaultValue, string returnedValue)[] values)
        {
            _values = values;
        }

        public IUserInputPrompt Create(string title, string label, string defaultValue)
        {
            if (_indexOfNextFakeValueReturned >= _values.Length)
            {
                throw new ArgumentException($"SimplePrompt expected to be called only {_values.Length} time(s): Test badly configured? Or implementation bug?!?");
            }

            (string label, string defaultValue, string returnedValue) value = _values[_indexOfNextFakeValueReturned];
            _indexOfNextFakeValueReturned++;

            label.Should().Be(value.label);
            defaultValue.Should().Be(value.defaultValue);

            return new UserInputForTest(value.returnedValue);
        }
    }

    private class UserInputForTest : IUserInputPrompt
    {
        private readonly string _returnValue;

        public string UserInput => _returnValue;

        public UserInputForTest(string returnValue)
        {
            _returnValue = returnValue;
        }

        public void Dispose()
        {
        }

        public DialogResult ShowDialog(IWin32Window owner) => DialogResult.OK;
    }

    [Test]
    public void Parse_should_parse_userFiles()
    {
        _commands.GetService<IFilePromptCreator>().Returns(new FilePromptCreatorForTest(@"C:\a_file_path\file.txt"));
        (string? arguments, bool abort, bool cancel) result = ScriptRunner.ParseUserInputs("script_name", "unquoted path:{UserFiles} - quoted path:{{UserFiles}}", _commands,
            owner: null, scriptOptionsProvider: _scriptOptionsProvider);

        result.abort.Should().Be(false);
        result.arguments.Should().Be(@"unquoted path:C:\a_file_path\file.txt - quoted path:""C:\a_file_path\file.txt""");
    }

    private class FilePromptCreatorForTest : IFilePromptCreator
    {
        public string _returnValue;

        public FilePromptCreatorForTest(string returnValue)
        {
            _returnValue = returnValue;
        }

        public IUserInputPrompt Create() => new UserInputForTest(_returnValue);
    }
}
