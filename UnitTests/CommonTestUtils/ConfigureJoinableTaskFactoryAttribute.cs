using System;
using System.Collections;
using System.Threading;
using System.Windows.Forms;
using GitUI;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace CommonTestUtils
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class ConfigureJoinableTaskFactoryAttribute : Attribute, ITestAction
    {
        public ActionTargets Targets => ActionTargets.Test;

        public void BeforeTest(ITest test)
        {
            IList apartmentState = null;
            for (var scope = test; scope != null; scope = scope.Parent)
            {
                apartmentState = scope.Properties[nameof(ApartmentState)];
                if (apartmentState.Count > 0)
                {
                    break;
                }
            }

            if (!apartmentState.Contains(ApartmentState.STA))
            {
                return;
            }

            Assert.AreEqual(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());

            // This form created for obtain UI synchronization context only
            using (new Form())
            {
                // Store the shared JoinableTaskContext
                ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
            }
        }

        public void AfterTest(ITest test)
        {
            ThreadHelper.JoinableTaskContext?.Factory.Run(() => ThreadHelper.JoinPendingOperationsAsync());
            ThreadHelper.JoinableTaskContext = null;
        }
    }
}
