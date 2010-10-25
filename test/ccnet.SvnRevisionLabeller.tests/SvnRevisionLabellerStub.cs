namespace CcNet.Labeller.Tests
{
	public class SvnRevisionLabellerStub : SvnRevisionLabeller
	{
		public SvnRevisionLabellerStub()
		{
		}

		public SvnRevisionLabellerStub(ISystemClock systemClock) : base(systemClock)
		{
		}

		public void SetRevision(int svnRevision)
		{
			_revision = svnRevision;
		}

		protected override int GetRevision()
		{
			return _revision;
		}

		private int _revision;
	}
}