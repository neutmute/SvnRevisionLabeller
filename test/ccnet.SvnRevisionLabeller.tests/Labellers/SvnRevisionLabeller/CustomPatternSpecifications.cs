using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Remote;

namespace CcNet.Labeller.Tests
{
    [TestFixture]
    public class WhenCustomPatternIsSet : Specification
    {
        protected override void Arrange()
        {
            _previousResult = Mockery.DynamicMock<IIntegrationResult>();

            using (_mockery.Record())
            {
                Expect.Call(_previousResult.Label).Return("1.0.0.100");
                SetupResult.For(_previousResult.LastIntegrationStatus).Return(IntegrationStatus.Success);
            }

            _mockery.ReplayAll();

            _labeller = new SvnRevisionLabellerStub();
            _labeller.SetRevision(5);
            _labeller.Pattern = "Custom (major).(minor).4.(revision) label";
            _labeller.Major = 2;
            _labeller.Minor = 3;
        }

        protected override void Act()
        {
            _label = _labeller.Generate(_previousResult);
        }

        [Test]
        public void ThePatternIsUsedToGenerateTheLabel()
        {
            Assert.That(_label, Is.EqualTo("Custom 2.3.4.5 label"));
        }

        private SvnRevisionLabellerStub _labeller;
        private IIntegrationResult _previousResult;
        private string _label;
    }

    [TestFixture]
    public class WhenRebuildIsSet : Specification
    {
        protected override void Arrange()
        {
            _previousResult = Mockery.DynamicMock<IIntegrationResult>();

            using (_mockery.Record())
            {
                Expect.Call(_previousResult.Label).Return("2.3.0.100");
                SetupResult.For(_previousResult.LastIntegrationStatus).Return(IntegrationStatus.Success);
            }

            _mockery.ReplayAll();

            _labeller = new SvnRevisionLabellerStub();
            _labeller.SetRevision(100);
            _labeller.Pattern = "(major).(minor).(build).(revision).(rebuild)";
            _labeller.Major = 2;
            _labeller.Minor = 3;
        }

        protected override void Act()
        {
            _label = _labeller.Generate(_previousResult);
            _labelSecondGeneration = _labeller.Generate(_previousResult);
        }

        [Test]
        public void TheRebuildNumberIsIncremented()
        {
            Assert.That(_label, Is.EqualTo("2.3.0.100.1"));
            Assert.That(_labelSecondGeneration, Is.EqualTo("2.3.0.100.2"));
        }

        private SvnRevisionLabellerStub _labeller;
        private IIntegrationResult _previousResult;
        private string _label;
        private string _labelSecondGeneration;
    }
}
