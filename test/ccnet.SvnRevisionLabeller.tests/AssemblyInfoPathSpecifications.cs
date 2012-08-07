using System.IO;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Remote;

namespace CcNet.Labeller.Tests
{
    [TestFixture]
    public class WhenAssemblyInfoPathIsSet : Specification
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
            var assemblyInfoFilename = Path.GetTempFileName();
            File.WriteAllText(assemblyInfoFilename,
                              @"
using System.Reflection;
[assembly: AssemblyVersion(""4.5.*"")]
            ");

            var anotherRandomFile = assemblyInfoFilename + ".nothere";
            _labeller.AssemblyInfoPath = string.Format("{0},{1}", anotherRandomFile, assemblyInfoFilename);
        }

        protected override void Act()
        {
            _label = _labeller.Generate(_previousResult);
        }

        [Test]
        public void AssemblyInfoParsed()
        {
            Assert.That(_label, Is.EqualTo("4.5.0.0"));
        }

        private SvnRevisionLabellerStub _labeller;
        private IIntegrationResult _previousResult;
        private string _label;
    }
}