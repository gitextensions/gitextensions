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

            List<Type> translatableTypes = TranslationUtl.GetTranslatableTypes();

            var testTranslation = new Translation();

            foreach (Type type in translatableTypes)
            {
                try
                {
                    ITranslate obj = TranslationUtl.CreateInstanceOfClass(type) as ITranslate;
                    obj.AddTranslationItems(testTranslation);
                    obj.TranslateItems(testTranslation);
                }
                catch (System.Exception)
                {
                    Trace.WriteLine("Problem with class: " + type.FullName);
                    throw;
                }
            }
        }       
    }
}
