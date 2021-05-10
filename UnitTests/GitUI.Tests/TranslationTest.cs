using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GitUI;
using NUnit.Framework;
using ResourceManager;
using ResourceManager.Xliff;

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

            var translatableTypes = TranslationUtil.GetTranslatableTypes();

            List<(string typeName, Exception exception)> problems = new();

            foreach (var types in translatableTypes.Values)
            {
                TranslationFile translation = new();

                foreach (var type in types)
                {
                    try
                    {
                        using var obj = (ITranslate)TranslationUtil.CreateInstanceOfClass(type);
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
