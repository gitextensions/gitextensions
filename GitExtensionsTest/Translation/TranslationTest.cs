using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using GitUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ResourceManager;
using ResourceManager.Xliff;

namespace GitExtensionsTest.TranslationTest
{
    [TestClass]
    public class TranslationTest
    {

        [TestMethod]
        [STAThread]
        public void CreateInstanceOfClass()
        {
            // just reference to GitUI
            MouseWheelRedirector.Active = true;

            var translatableTypes = TranslationUtl.GetTranslatableTypes();

            var testTranslation = new Dictionary<string, TranslationFile>();

            foreach (var types in translatableTypes)
            {
                var tranlation = new TranslationFile();
                foreach (Type type in types.Value)
                {
                    try
                    {
                        ITranslate obj = TranslationUtl.CreateInstanceOfClass(type) as ITranslate;
                        obj.AddTranslationItems(tranlation);
                        obj.TranslateItems(tranlation);
                    }
                    catch (System.Exception)
                    {
                        Trace.WriteLine("Problem with class: " + type.FullName);
                        throw;
                    }
                }
                testTranslation[types.Key] = tranlation;
            }
        }
    }
}
