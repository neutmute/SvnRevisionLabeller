SVN Revision Labeller
---------------------

SVN Revision Labeller is a plugin for CruiseControl.NET that allows you to generate CruiseControl labels for your builds, based upon the revision number of your Subversion working copy.

Requirements
------------

* CruiseControl.NET v1.4.4 SP1 - the plugin has been compiled and tested against this version of CC.NET. As such, it uses the .NET Framework 2.0, and is not backwards compatible with previous versions of CC.NET. If you cannot upgrade to this version of CC.NET and want to use this plugin, you can rebuild from the source code, but your will need to replace the solution and project files to work with Visual Studio 2003/2005. This release is *not* compatible with earlier releases of CC.NET, due to breaking changes introduced. If you want to run v1.4.3 or earlier, you will need v1.0.3.25899 of SvnRevisionLabeller.

Installation
------------

Two builds are provided - Debug and Release. You should only need the Debug build if you are diganosing technical issues with the plugin. Just drop the contents of either src\ccnet.SvnRevisionLabeller.plugin\bin\Debug or src\ccnet.SvnRevisionLabeller.plugin\bin\Release into the folder that CruiseControl.NET is running from, update ccnet.config with the appropriate configuration (see below), and restart the service.

Configuration
-------------

Below is a sample configuration for svnRevisionLabeller, showing the mandatory fields:

<labeller type="svnRevisionLabeller">
	<url>svn://localhost/repository/trunk</url>
</labeller>

The following sample configuration shows the complete set of fields:

<labeller type="svnRevisionLabeller">
	<major>8</major>
	<minor>2</minor>
	<build>0</build>
	<pattern>Prerelease {major}.{minor}.{build}.{revision}</pattern>
	<incrementOnFailure>false</incrementOnFailure>
	<resetBuildAfterVersionChange>false</resetBuildAfterVersionChange>
	<url>https://localhost/repository/branches/dev-project</url>
	<executable>C:\Svn\Bin\svn.exe</executable>
	<username>ccnetuser</username>
	<password>ccnetpassword</password>
</labeller>

Usage
-----

When CruiseControl.NET begins a project build, it generates a label for the build and stores it in the property CCNetLabel - this property can then be used by NAnt or MSBuild to generate the AssemblyInfo.cs for your assemblies, so that CC.NET displays as its label the same version that the assemblies are built with. So, if the configuration for the labeller is set as:

<labeller type="svnRevisionLabeller">
	<major>7</major>
	<minor>11</minor>
	<url>svn://localhost/repository/trunk</url>
</labeller>

and the latest Subversion revision number is 920, the CCNetLabel will be set to 7.11.0.920. Forcing a build without any changes to the repository will not make any changes to the label. A subsequent commit to the repository would then set the label to 7.11.0.921, and so on.

If you want to generate a more complex label, you use the Pattern field. This contains a number of tokens for the Major, Minor, Build, Revision and Rebuilt numbers,and you can effectively create any label you want. For instance:

<labeller type="svnRevisionLabeller">
	<major>1</major>
	<minor>2</minor>
	<pattern>Labelling is as easy as {major} - {minor} - 3 - {revision}. See?</pattern>
	<url>svn://localhost/repository/trunk</url>
</labeller>

and the current revision is 4, then the generated build label be "Labelling is as easy as 1 - 2 - 3 - 4. See?"

The available tokens are:

	{major} - the major build number
	{minor} - the minor build number
	{build} - the build number 
	{revision} - the revision number
	{rebuild} - the number of times the build has been rebuilt (i.e. a forced build)

History
-------

2.0.0.20990
	* FIX - now runs against CC.NET v1.4.4 RC2;
	* NEW - greater control over the formatting of the build label (patch provided by fezguy); the prefix and postfix fields have been removed from configuration, since they are now replaced by the Pattern field, and by default, rebuilds are not counted. To reproduce the original behaviour of the plugin, you would want a Pattern similar to "{major}.{minor}.{revision}.{rebuilt}", so that successive forced builds without a new Subversion commit increments the version number by 1.

1.0.3.25899
	* FIX - the username and password attributes are swapped around (fix provided by Tony Mitchell);

1.0.2.16573
	* Now built against CC.NET v1.3, and tested against CC.NET v1.3
	* FIX - the revision number does not increase on successive builds if a prefix is specified (fix provided by Mike Usner);
	* FIX - if no working copy exists, then the labeller will not be able to work out what the current revision is, and throw an exception (fix provided by Matteo Tontini);

1.0.1.21635
	* Built against CC.NET v1.1, and tested against CC.NET v1.1 and v2.0;
	* initial public release;

