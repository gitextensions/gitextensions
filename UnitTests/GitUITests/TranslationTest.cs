using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using GitUI;
using NUnit.Framework;
using ResourceManager;
using ResourceManager.Xliff;

namespace GitUITests
{
    [TestFixture]
    public class TranslationTest
    {
        [Test]
        [Apartment(ApartmentState.STA)]
        public void CreateInstanceOfClass()
        {
            // just reference to GitUI
            MouseWheelRedirector.Active = true;

            var translatableTypes = TranslationUtil.GetTranslatableTypes();

            var testTranslation = new Dictionary<string, TranslationFile>();

            foreach (var (key, types) in translatableTypes)
            {
                var translation = new TranslationFile();
                foreach (Type type in types)
                {
                    try
                    {
                        var obj = (ITranslate)TranslationUtil.CreateInstanceOfClass(type);

                        obj.AddTranslationItems(translation);
                        obj.TranslateItems(translation);
                    }
                    catch
                    {
                        Trace.WriteLine("Problem with class: " + type.FullName);
                        throw;
                    }
                }

                testTranslation[key] = translation;
            }
        }
    }
}
