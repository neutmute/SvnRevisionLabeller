using System;
using System.Xml;
using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ccnet.SvnRevisionLabeller.plugin
{
	/// <summary>
	/// Generates label numbers using the Subversion revision number.
	/// </summary>
	/// <remarks>
	/// This class was inspired by Jonathan Malek's post on his blog 
	/// (<a href="http://www.jonathanmalek.com/blog/CruiseControlNETAndSubversionRevisionNumbersUsingNAnt.aspx">CruiseControl.NET and Subversion Revision Numbers using NAnt</a>),
	/// which used NAnt together with Subversion to retrieve the latest revision number. This plug-in moves it up into 
	/// CruiseControl.NET itself, so that you can see the latest revision number appearing in CCTray. The label can
	/// then be retrieved from within NAnt by accessing the property <c>${CCNetLabel}</c>.
	/// </remarks>
	[ReflectorType("svnRevisionLabeller")]
	public class SvnRevisionLabeller : ILabeller
	{
		#region Private members

		private int major;
		private int minor;
		private string _url;
		private string executable;
		private string prefix;
		private string username;
		private string password;
		private const string RevisionXPath = "/log/logentry/@revision";

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="SvnRevisionLabeller"/> class.
		/// </summary>
		public SvnRevisionLabeller()
		{
			major = 1;
			minor = 0;
			executable = "svn.exe";
			prefix = String.Empty;
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Returns the label to use for the current build.
		/// </summary>
		/// <param name="resultFromLastBuild">IntegrationResult from last build used to determine the next label</param>
		/// <returns>the label for the new build</returns>
		public string Generate(IIntegrationResult resultFromLastBuild)
		{
			// Get the last revision from the Subversion repository
			int svnRevision = GetRevision();

			// Get the last revision from CruiseControl
			Version version = ParseVersion(svnRevision, resultFromLastBuild);

			// If the revision number hasn't changed (because no new check-ins have been made), increment the build number;
			// Otherwise, reset the build number to 0
			int revision = (svnRevision == version.Build) ? version.Revision + 1 : 0;

			// Construct a new version number, adding any specified prefix
			Version newVersion = new Version(major, minor, svnRevision, revision);
			return prefix + newVersion;
		}

		/// <summary>
		/// Runs the task, given the specified <see cref="IIntegrationResult"/>, in the specified <see cref="IProject"/>.
		/// </summary>
		/// <param name="result"></param>
		public void Run(IIntegrationResult result)
		{
			result.Label = Generate(result);
		}

		#endregion

		#region Public properties

		/// <summary>
		/// Gets or sets the major build number.
		/// </summary>
		/// <value>The major build number.</value>
		[ReflectorProperty("major", Required=false)]
		public int Major
		{
			get
			{
				return major;
			}
			set
			{
				major = value;
			}
		}

		/// <summary>
		/// Gets or sets the minor build number.
		/// </summary>
		/// <value>The minor build number.</value>
		[ReflectorProperty("minor", Required=false)]
		public int Minor
		{
			get
			{
				return minor;
			}
			set
			{
				minor = value;
			}
		}

		/// <summary>
		/// Gets or sets the repository URL from which the <c>svn log</c> command will be run.
		/// </summary>
		/// <value>The repository.</value>
		[ReflectorProperty("url", Required = true)]
		public string Url
		{
			get
			{
				return _url;
			}
			set
			{
				_url = value;
			}
		}

		/// <summary>
		/// Gets or sets the Subversion client executable.
		/// </summary>
		/// <value>The path to the executable.</value>
		/// <remarks>
		/// If the value is not supplied, the task will expect to find <c>svn.exe</c> in the <c>PATH</c> environment variable.
		/// </remarks>
		[ReflectorProperty("executable", Required=false)]
		public string Executable
		{
			get
			{
				return executable;
			}
			set
			{
				executable = value;
			}
		}

		/// <summary>
		/// Gets or sets an optional prefix for the build label.
		/// </summary>
		/// <value>A string to prefix the version number with.</value>
		[ReflectorProperty("prefix", Required=false)]
		public string Prefix
		{
			get 
			{ 
				return prefix; 
			}
			set 
			{
				prefix = value;
			}
		}

		/// <summary>
		/// Gets or sets the username to access SVN repository.
		/// </summary>
		/// <value>The repository.</value>
		[ReflectorProperty("username", Required = false)]
		public string Username
		{
			get
			{
				return username;
			}
			set
			{
				username = value;
			}
		}

		/// <summary>
		/// Gets or sets the password to access SVN repository.
		/// </summary>
		/// <value>The repository.</value>
		[ReflectorProperty("password", Required = false)]
		public string Password
		{
			get
			{
				return password;
			}
			set
			{
				password = value;
			}
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Parses the version.
		/// </summary>
		/// <param name="revision">The revision.</param>
		/// <param name="resultFromLastBuild">The result from last build.</param>
		private Version ParseVersion(int revision, IIntegrationResult resultFromLastBuild)
		{
			try
			{
				string label = resultFromLastBuild.LastSuccessfulIntegrationLabel;
				if (prefix.Length > 0)
				{
					label = label.Replace(prefix, String.Empty).TrimStart('_');
				}
				return new Version(label);
			}
			catch (SystemException)
			{
				return new Version(major, minor, revision, 0);
			}
		}

		/// <summary>
		/// Gets the latest Subversion revision by checking the last log entry.
		/// </summary>
		private int GetRevision()
		{
			// Set up the command-line arguments required
			ProcessArgumentBuilder argBuilder = new ProcessArgumentBuilder();
			argBuilder.AppendArgument("log");
			argBuilder.AppendArgument("--xml");
			argBuilder.AppendArgument("--limit 1");
			argBuilder.AppendArgument(Url);
			if (Username != null && Username.Length > 0 && Password != null && Password.Length > 0)
			{
				AppendCommonSwitches(argBuilder); 
			}

			// Run the svn log command and capture the results
			ProcessResult result = RunProcess(argBuilder);
			Log.Debug("Received XML : " + result.StandardOutput);

			// Load the results into an XML document
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(result.StandardOutput);

			// Retrieve the revision number from the XML
			XmlNode node = xml.SelectSingleNode(RevisionXPath);
			return Convert.ToInt32(node.InnerText);
		}

		/// <summary>
		/// Appends the arguments required to authenticate against Subversion.
		/// </summary>
		/// <param name="buffer"><The argument builder./param>
		private void AppendCommonSwitches(ProcessArgumentBuilder buffer)
		{
			buffer.AddArgument("--username", Username);
			buffer.AddArgument("--password", Password);
			buffer.AddArgument("--non-interactive");
			buffer.AddArgument("--no-auth-cache");
		}

		/// <summary>
		/// Runs the Subversion process using the specified arguments.
		/// </summary>
		/// <param name="arguments">The Subversion client arguments.</param>
		/// <returns>The results of running the process, including captured output.</returns>
		private ProcessResult RunProcess(ProcessArgumentBuilder arguments)
		{
			ProcessInfo info = new ProcessInfo(executable, arguments.ToString(), null);
			Log.Debug("Running Subversion with arguments : " + info.Arguments);

			ProcessExecutor executor = new ProcessExecutor();
			ProcessResult result = executor.Execute(info);
			return result;
		}
		#endregion
	}
}