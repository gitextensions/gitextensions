using NUnit.Framework;
using PatchApply;
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
    class PatchManagerTest
    {
        [TestMethod]
        public void TestPatchManagerConstructor()
        {
            PatchManager manager = new PatchManager();
            Assert.IsNotNull(manager);
        }
    }
}
