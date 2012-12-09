using NUnit.Framework;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestContext = System.Object;
using TestProperty = NUnit.Framework.PropertyAttribute;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCommandsTests.Patch
{
    [TestClass]
    class PatchTest
    {
        [TestMethod]
        public void TestPatchConstructor()
        {
            PatchApply.Patch patch = new PatchApply.Patch();

            Assert.IsNotNull(patch);
        }
    }
}
