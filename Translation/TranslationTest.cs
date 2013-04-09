#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Category = Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute;
#else
using NUnit.Framework;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestContext = System.Object;
using TestProperty = NUnit.Framework.PropertyAttribute;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
#endif
using GitUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using ResourceManager.Translation;


namespace GitExtensionsTest.Translation
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

            ResourceManager.Translation.Translation testTranslation = new ResourceManager.Translation.Translation();

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
