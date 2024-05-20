using GitExtensions.Extensibility.Translations;
using GitExtensions.Extensibility.Translations.Xliff;
using GitUI;

namespace GitUITests
{
    [TestFixture]
    public sealed class TranslationTest
    {
        [SetUp]
        public void SetUp()
        {
            GitModuleForm.IsUnitTestActive = true;
        }

        [TearDown]
        public void TearDown()
        {
            GitModuleForm.IsUnitTestActive = false;
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void CreateInstanceOfClass()
        {
            UserEnvironmentInformation.Initialise("0123456789012345678901234567890123456789", isDirty: false);

            Dictionary<string, List<Type>> translatableTypes = TranslationUtil.GetTranslatableTypes();

            List<(string typeName, Exception exception)> problems = [];

            foreach (List<Type> types in translatableTypes.Values)
            {
                TranslationFile translation = new();

                foreach (Type type in types)
                {
                    try
                    {
                        using ITranslate obj = (ITranslate)TranslationUtil.CreateInstanceOfClass(type);
                        obj.AddTranslationItems(translation);
                        obj.TranslateItems(translation);
                    }
                    catch (Exception ex)
                    {
                        problems.Add((type.FullName, ex));
                    }
                }
            }

            if (problems.Count != 0)
            {
                Assert.Fail(string.Join(
                    "\n\n--------\n\n",
                    problems.Select(p => $"Problem with type {p.typeName}\n\n{p.exception}")));
            }
        }
    }
}
