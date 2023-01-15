using GitCommands.Gpg;
using GitUI.CommandsDialogs;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    [TestFixture]
    public sealed class FormBrowseControllerTests
    {
        private FormBrowseController _controller;
        private IGitGpgController _gitGpgController;

        [SetUp]
        public void Setup()
        {
            _gitGpgController = Substitute.For<IGitGpgController>();

            _controller = new FormBrowseController(_gitGpgController);
        }
    }
}
