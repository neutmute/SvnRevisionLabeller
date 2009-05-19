using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Remote;

namespace CcNet.Labeller.Tests
{
	[TestFixture]
	public class WhenRevisionPropertyIsNotSetAndLabelIsGeneratedAfterASuccessfulBuild : Specification
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
			_labeller.SetRevision(105);
		}

		protected override void Act()
		{
			_label = _labeller.Generate(_previousResult);
		}

		[Test]
		public void RevisionNumberIsSetToTheCurrentSvnRevisionNumber()
		{
			Assert.That(_label, Is.EqualTo("1.0.0.105"));
		}

		private SvnRevisionLabellerStub _labeller;
		private IIntegrationResult _previousResult;
		private string _label;
	}
}