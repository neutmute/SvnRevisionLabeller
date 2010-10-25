using System;

namespace CcNet.Labeller
{
	public class SystemClock : ISystemClock
	{
		/// <summary>
		/// Gets the current date and time from the system clock.
		/// </summary>
		public DateTime Now
		{
			get { return DateTime.Now; }
		}

		/// <summary>
		/// Gets the current date from the system clock.
		/// </summary>
		public DateTime Today
		{
			get { return DateTime.Today; }
		}
	}
}