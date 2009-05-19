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
			_labeller.Pattern = "Custom {major}.{minor}.4.{revision} label";
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
}