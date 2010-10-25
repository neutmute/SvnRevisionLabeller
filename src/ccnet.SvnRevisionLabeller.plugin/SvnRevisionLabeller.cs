using System;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace CcNet.Labeller
{
	/// <summary>
	/// Generates CC.NET label numbers using the Microsoft-recommended versioning 
	/// format (ie. Major.Minor.Build.Revision). The build number is auto-
	/// incremented for each successful build, and the latest Subversion commit number
	/// is used to generate the revision. The resultant label is accessible from 
	/// apps such as MSBuild via the <c>$(CCNetLabel)</c> property , and NAnt via 
	/// the <c>${CCNetLabel}</c> property.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class was inspired by Jonathan Malek's post on his blog 
	/// (<a href="http://www.jonathanmalek.com/blog/CruiseControlNETAndSubversionRevisionNumbersUsingNAnt.aspx">CruiseControl.NET and Subversion Revision Numbers using NAnt</a>),
	/// which used NAnt together with Subversion to retrieve the latest revision number. This plug-in moves it up into 
	/// CruiseControl.NET itself, so that you can see the latest revision number appearing in CCTray. 
	/// </para>
	/// <para>
	/// The plugin was then substantially rewritten by fezguy (http://code.google.com/u/fezguy/), incorporating
	/// the following new features:
	/// <ul>
	/// <li>defaults to use the Microsoft recommended versioning format;</li>
	/// <li>option to increment the build number always, similar to DefaultLabeller [default: false];</li>
	/// <li>option to reset the build number to 0 after a (major/minor) version change [default: true];</li>
	/// <li>option to use "--trust-server-cert" command line parameter (Subversion v1.6+)</li>
	/// <li>"pattern" property to support user-defined build number format;</li>
	/// <li>handles an additional "rebuild" field via "pattern" property which counts builds of same revision;</li>
	/// <li>option to include a postfix on version;</li>
	/// <li>handles the quoting of Subversion URLs with spaces</li>
	/// </ul>
	/// </para>
	/// </remarks>
	[ReflectorType("svnRevisionLabeller")]
	public class SvnRevisionLabeller : ILabeller
	{
		private const string MajorToken = "{major}";
		private const string MinorToken = "{minor}";
		private const string BuildToken = "{build}";
		private const string RevisionToken = "{revision}";
		private const string RebuildToken = "{rebuild}";
		private const string DateToken = "{date}";
		private const string MsRevisionToken = "{msrevision}";
		private const string RevisionXPath = "/log/logentry/@revision";
		private readonly ISystemClock _systemClock;
		private int _rebuild;

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="SvnRevisionLabeller"/> class.
		/// </summary>
		public SvnRevisionLabeller() : this(new SystemClock())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SvnRevisionLabeller"/> class.
		/// </summary>
		/// <param name="systemClock">The system clock implementation that will be used when calculating date-based
		/// revision numbers.</param>
		public SvnRevisionLabeller(ISystemClock systemClock)
		{
			_systemClock = systemClock;

			Major = 1;
			Minor = 0;
			Build = -1;
			Pattern = "{major}.{minor}.{build}.{revision}";
			Executable = "svn.exe";
			ResetBuildAfterVersionChange = true;

			_rebuild = 0;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the major version.
		/// </summary>
		/// <value>The major version number.</value>
		[ReflectorProperty("major", Required = false)]
		public int Major { get; set; }

		/// <summary>
		/// Gets or sets the minor version.
		/// </summary>
		/// <value>The minor version number.</value>
		[ReflectorProperty("minor", Required = false)]
		public int Minor { get; set; }

		/// <summary>
		/// Gets or sets the build number.
		/// </summary>
		/// <value>The build number.</value>
		[ReflectorProperty("build", Required = false)]
		public int Build { get; set; }

		/// <summary>
		/// Gets or sets the version format pattern.
		/// </summary>
		/// <value>The pattern.</value>
		[ReflectorProperty("pattern", Required = false)]
		public string Pattern { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the build number should 
		/// increment on failed build.
		/// </summary>
		/// <value><c>true</c> if the build number should increment on failure; otherwise, <c>false</c>.</value>
		[ReflectorProperty("incrementOnFailure", Required = false)]
		public bool IncrementOnFailure { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to reset build number after a version change.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the build number should be reset after a version change; otherwise, <c>false</c>.
		/// </value>
		[ReflectorProperty("resetBuildAfterVersionChange", Required = false)]
		public bool ResetBuildAfterVersionChange { get; set; }

		/// <summary>
		/// Gets or sets the path to the Subversion executable.
		/// </summary>
		/// <remarks>
		/// By default, the labeller checks the <c>PATH</c> environment variable.
		/// </remarks>
		/// <value>The executable.</value>
		[ReflectorProperty("executable", Required = false)]
		public string Executable { get; set; }

		/// <summary>
		/// Gets or sets the username that will be used to access the Subversion repository.
		/// </summary>
		/// <value>The username.</value>
		[ReflectorProperty("username", Required = false)]
		public string Username { get; set; }

		/// <summary>
		/// Gets or sets the password that will be used to access the Subversion repository.
		/// </summary>
		/// <value>The password.</value>
		[ReflectorProperty("password", Required = false)]
		public string Password { get; set; }

		/// <summary>
		/// Gets or sets the URL that will be used to access the Subversion repository.
		/// </summary>
		/// <value>A string representing the URL of the Subversion repository.</value>
		[ReflectorProperty("url", Required = true)]
		public string Url { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the server certificate should 
		/// be trusted blindly. This is useful for server installations of v1.6 or
		/// greater with self-signed SSL certificates.
		/// </summary>
		/// <value><c>true</c> if the server's SSL certificate should be trusted; otherwise, <c>false</c>.</value>
		[ReflectorProperty("trustServerCertificate", Required = false)]
		public bool TrustServerCertificate { get; set; }

		/// <summary>
		/// Gets or sets the start date that date-based build numbers will be calculated from.
		/// </summary>
		/// <value>The start date that date-based build numbers will be calculated from. Build numbers
		/// will be the number of days sicne the specified date.</value>
		[ReflectorProperty("startDate", Required = false)]
		public string StartDate { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Runs the task, given the specified <see cref="IIntegrationResult"/>, in the specified <see cref="IProject"/>.
		/// </summary>
		/// <param name="result">The label for the current build.</param>
		public void Run(IIntegrationResult result)
		{
			result.Label = Generate(result);
		}

		/// <summary>
		/// Returns the label to use for the current build.
		/// </summary>
		/// <param name="resultFromLastBuild">IntegrationResult from last build used to determine the next label.</param>
		/// <returns>The label for the current build.</returns>
		/// <exception cref="System.ArgumentException">Thrown when an error occurs while formatting the version number using the various formatting tokens.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when an error occurs while formatting the version number and an argument has not been specified.</exception>
		public string Generate(IIntegrationResult resultFromLastBuild)
		{
			// get last revision from Subversion
			int revision = GetRevision();

			// get last revision from CC
			Version lastVersion = ParseVersion(revision, resultFromLastBuild);

			int build = 0;

			// check if the build property has been explicitly set
			if (Build > -1)
			{
				// use the user-defined build number
				build = Build;
			}
			else
			{
				// determine if one or both major/minor version has changed
				if ((Major > lastVersion.Major) || (Minor > lastVersion.Minor))
				{
					// if we're resetting, the build is already 0
					// otherwise we'll want to increment build number
					if (!ResetBuildAfterVersionChange)
					{
						// keep incrementing
						build = lastVersion.Build + 1;
					}
				}
				else
				{
					// build will at least be equal to last build
					build = lastVersion.Build;

					// determine if last build was success, or if the user wants to 
					// increment build number always
					if (((resultFromLastBuild.LastIntegrationStatus == IntegrationStatus.Success) ||
						IncrementOnFailure) && (revision > lastVersion.Revision))
					{
						// increment the build number
						build = lastVersion.Build + 1;
					}
				}
			}

			// check to see if "forced" applies
			if (revision == lastVersion.Revision)
			{
				_rebuild++;
			}

			int elapsedDays = CalculateElapsedDays(StartDate, _systemClock.Now);
			int msRevision = CalculateMsRevision(_systemClock.Today, _systemClock.Now);

			// Replace the tokens in the pattern with the appropriate string formatting placeholders
			string format = Pattern.Replace(MajorToken, "{0}")
				.Replace(MinorToken, "{1}")
				.Replace(BuildToken, "{2}")
				.Replace(RevisionToken, "{3}")
				.Replace(RebuildToken, "{4}")
				.Replace(DateToken, "{5}")
				.Replace(MsRevisionToken, "{6}");

			return String.Format(format, Major, Minor, build, revision, _rebuild, elapsedDays, msRevision);
		}

		/// <summary>
		/// Calculates the revision using the standard Microsoft format, being the number of
		/// seconds since midnight divided by 2.
		/// </summary>
		/// <param name="startDate">The start date.</param>
		/// <param name="currentDate">The current date.</param>
		/// <returns>The number of seconds since midnight, divided by 2.</returns>
		private static int CalculateMsRevision(DateTime startDate, DateTime currentDate)
		{
			return (int)((currentDate - startDate).TotalSeconds / 2);
		}

		/// <summary>
		/// Calculates the number of elapsed days.
		/// </summary>
		/// <param name="startDate">The start date, parsed from the labeller's configuration.</param>
		/// <param name="currentDate">The current date.</param>
		/// <returns>An integer representing the number of elapsed days between the
		/// two specified dates. If the <c>startDate</c> string value cannot be parsed, <c>0</c>
		/// will be returned.</returns>
		private static int CalculateElapsedDays(string startDate, DateTime currentDate)
		{
			DateTime calculateFrom;
			return DateTime.TryParse(startDate, out calculateFrom)
				? (int)((currentDate - calculateFrom).TotalDays)
				: 0;
		}

		/// <summary>
		/// Gets the latest Subversion revision by checking the last log entry.
		/// </summary>
		/// <remarks>
		/// If an error occurs while parsing the Subversion log, the revision number will be returned
		/// as a <c>0</c>.
		/// </remarks>
		/// <returns>The last revision number.</returns>
		protected virtual int GetRevision()
		{
			// Set up the command-line arguments required
			ProcessArgumentBuilder argBuilder = new ProcessArgumentBuilder();
			argBuilder.AppendArgument("log");
			argBuilder.AppendArgument("--xml");
			argBuilder.AppendArgument("--limit 1");
			argBuilder.AddArgument(Quote(Url));

			if (TrustServerCertificate)
			{
				argBuilder.AppendArgument("--trust-server-cert");
			}

			if (!String.IsNullOrEmpty(Username))
			{
				AppendCommonSwitches(argBuilder);
			}

			// Run the svn log command and capture the results
			ProcessResult result = RunSvnProcess(argBuilder);
			Log.Debug("Received XML : " + result.StandardOutput);

			try
			{
				// Load the results into an XML document
				XmlDocument xml = new XmlDocument();
				xml.LoadXml(result.StandardOutput);

				// Retrieve the revision number from the XML
				XmlNode node = xml.SelectSingleNode(RevisionXPath);
				return Convert.ToInt32(node.InnerText);
			}
			catch (XmlException)
			{
				return 0;
			}
			catch (XPathException)
			{
				return 0;
			}
			catch (OverflowException)
			{
				return 0;
			}
			catch (FormatException)
			{
				return 0;
			}
		}

		/// <summary>
		/// Ensures that the SVN URL is surrounded with quotation marks, so that paths with 
		/// spaces in them do not cause an exception.
		/// </summary>
		/// <param name="urlToBeQuoted">The URL to be quoted.</param>
		/// <returns>The original URL surrounded with quotation marks</returns>
		protected virtual string Quote(string urlToBeQuoted)
		{
			return String.Format(@"""{0}""", urlToBeQuoted);
		}

		/// <summary>
		/// Appends the arguments required to authenticate against Subversion.
		/// </summary>
		/// <param name="buffer">The argument builder.</param>
		protected virtual void AppendCommonSwitches(ProcessArgumentBuilder buffer)
		{
			buffer.AddArgument("--username", Username);
			buffer.AddArgument("--password", Password);
			buffer.AddArgument("--non-interactive");
			buffer.AddArgument("--no-auth-cache");
		}

		/// <summary>
		/// Runs the Subversion process.
		/// </summary>
		/// <param name="arguments">The command-line arguments.</param>
		/// <returns>The results of executing the process including output.</returns>
		protected virtual ProcessResult RunSvnProcess(ProcessArgumentBuilder arguments)
		{
			// prepare process
			ProcessInfo info = new ProcessInfo(Executable, arguments.ToString());

			// execute process
			ProcessExecutor executor = new ProcessExecutor();
			ProcessResult result = executor.Execute(info);

			// return results
			return result;
		}

		/// <summary>
		/// Parses a version string.
		/// </summary>
		/// <param name="revision">The current revision.</param>
		/// <param name="resultFromLastBuild">The result from last build.</param>
		/// <returns>A <see cref="System.Version"/> populated with the previous version info.</returns>
		protected virtual Version ParseVersion(int revision, IIntegrationResult resultFromLastBuild)
		{
			string pattern = null;

			// determine if pattern is to be used
			if (!String.IsNullOrEmpty(Pattern))
			{
				// convert pattern to regex to be used to reconstitute version parts
				pattern = Pattern
					.Replace(MajorToken, "(?<major>[0-9]+)")
					.Replace(MinorToken, "(?<minor>[0-9]+)")
					.Replace(BuildToken, "(?<build>[0-9]+)")
					.Replace(RevisionToken, "(?<revision>[0-9]+)")
					.Replace(RebuildToken, "(?<rebuild>[0-9]+)");
			}

			// get previous build label
			string label = resultFromLastBuild.LastSuccessfulIntegrationLabel;

			try
			{
				// create new version
				Version version;

				// determine if pattern is being used
				if (String.IsNullOrEmpty(pattern))
				{
					// create version based on string
					version = new Version(label);
				}
				else
				{
					// extract version components
					Regex regex = new Regex(pattern);
					Match match = regex.Match(label);

					int major = Convert.ToInt32(match.Groups["major"].Value);
					int minor = Convert.ToInt32(match.Groups["minor"].Value);
					int build = Convert.ToInt32(match.Groups["build"].Value);
					int rev = Convert.ToInt32(match.Groups["revision"].Value);

					// special case for "forced"
					Int32.TryParse(match.Groups["rebuild"].Value, out _rebuild);

					// create version based on components
					version = new Version(major, minor, build, rev);
				}

				return version;
			}
			catch (SystemException)
			{
				// uh oh! fake it out
				return new Version(Major, Minor, 0, revision);
			}
		}

		#endregion
	}
}