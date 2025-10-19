using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace CommonTestUtils;

// Declare [assembly: TestCleanUp] in any test library that uses
// GitModuleTestHelper or ReferenceRepository (which uses
// GitModuleTestHelper internally). Otherwise, the temporary
// repositories created in the temp folder for use in tests
// won't get cleaned up.
//
// What this class does:
//
// Some tests create a physical Git repository on-disk as part of the
// environment in which to perform the test. This needs to be cleaned
// up. But, the test run's duration is measurably increased by doing
// this clean-up synchronously. So, instead, the clean-up is run in a
// background task. But, when the [ConfigureJoinableTaskFactory]
// attribute is used, there is an AfterTest action that runs _after_
// the test's TearDown is complete. The purpose of that AfterTest
// action is to block until all tasks started within the test have
// completed. If the TearDown kicks off clean-up of the repository,
// it could delete the files out from under code that is still running.
// This won't cause the test to fail, because it's a background task
// fired up in the ambient ThreadHelper.JoinableTaskContext, but it
// can cause unhandled exceptions to occur, resulting in a messagebox
// appearing in the middle of the test run. And, this exception blocks
// the worker task, and the AfterTest hook supplied by the
// [ConfigureJoinableTaskFactory] joins on the worker task, so the test
// run is blocked from continuing until the exception dialog is dealt
// with.
//
// To resolve this issue, clean-up actions are deferred until after
// the AfterTest hook from [ConfigureJoinableTestFactory] has finished
// joining the threads. This is done by declaring this attribute before
// the [ConfigureJoinableTestFactory]; this causes NUnit to process
// this ITestAction for BeforeTest first and AfterTest last.
//
// ThreadCleanUp.BeforeTest
//   ConfigureJoinableTestFactory.BeforeTest
//     Test.SetUp
//     Test
//     Test.TearDown
//   ConfigureJoinableTestFactory.AfterTest -> wait for detached tasks
// ThreadCleanUp.AfterTest -> kick off clean-up tasks
//
// The clean-up tasks are thus kept from interfering with detached
// operations started during the test.
//
// It is possible at the end of the test suite execution for a clean-
// up action to still be in progress when the test process is ready
// to exit. There is a theoretical possibility that a clean-up action
// might fail to run or be interrupted. To avoid this possibility,
// TestCleanUpAttribute also registers with NUnit for the Suite
// target. This allows it to receive BeforeTest and AfterTest
// notifications for the overall test suite, and the AfterTest
// callback can be used to wait until all ongoing clean-up operations
// are complete.
//
// ThreadCleanUp.BeforeTest (suite)
//   ThreadCleanUp.BeforeTest
//     ConfigureJoinableTestFactory.BeforeTest
//       Test.SetUp
//       Test
//       Test.TearDown
//     ConfigureJoinableTestFactory.AfterTest -> wait for detached tasks
//   ThreadCleanUp.AfterTest -> kick off clean-up tasks
//   ...
//   ThreadCleanUp.BeforeTest
//     ConfigureJoinableTestFactory.BeforeTest
//       Test.SetUp
//       Test
//       Test.TearDown
//     ConfigureJoinableTestFactory.AfterTest -> wait for detached tasks
//   ThreadCleanUp.AfterTest -> kick off clean-up tasks
// ThreadCleanUp.AfterTest (suite) -> wait for clean-up tasks before exit
public class EpilogueAttribute : Attribute, ITestAction
{
    public ActionTargets Targets => ActionTargets.Suite | ActionTargets.Test;

    public void BeforeTest(ITest test)
    {
    }

    public void AfterTest(ITest test)
    {
        if (test.IsSuite)
        {
            Epilogue.ExecuteAfterSuiteActions();
        }
        else
        {
            Epilogue.ExecuteAfterTestActions();
        }
    }
}
