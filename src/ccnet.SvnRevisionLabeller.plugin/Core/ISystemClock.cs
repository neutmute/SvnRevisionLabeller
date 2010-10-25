using System;

namespace CcNet.Labeller
{
	/// <summary>
	/// Interface for retrieving <see cref="DateTime" /> from the system clock.
	/// </summary>
	public interface ISystemClock
	{
		/// <summary>
		/// Gets the current date and time from the system clock.
		/// </summary>
		/// <value>The now.</value>
		DateTime Now { get; }

		/// <summary>
		/// Gets the current date from the system clock.
		/// </summary>
		/// <value>The today.</value>
		DateTime Today { get; }
	}
}