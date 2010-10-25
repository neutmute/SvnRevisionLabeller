using System;

namespace CcNet.Labeller.Tests
{
	public class FakeSystemClock : ISystemClock
	{
		public FakeSystemClock(DateTime setClockTo)
		{
			Now = setClockTo;
			Today = setClockTo.Date;
		}

		public DateTime Now
		{ 
			get; 
			private set;
		}

		public DateTime Today
		{ 
			get; 
			private set;
		}
	}
}