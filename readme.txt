SVN Revision Labeller
---------------------

SVN Revision Labeller is a plugin for CruiseControl.NET that allows you to generate CruiseControl labels for your builds, based upon the revision number of your Subversion working copy.

Requirements
------------

* CruiseControl.NET v1.3 - the plugin has been compiled and tested against this version of CC.NET. As such, it uses the .NET Framework 2.0, and is not backwards compatible with previous versions of CC.NET. If you cannot upgrade to this version of CC.NET and want to use this plugin, you can rebuild from the source code, but your will need to replace the solution and project files to work with Visual Studio 2003.

Installation
------------

Two builds are provided - Debug and Release. You should only need the Debug build if you are diganosing technical issues with the plugin. Just drop the contents of either src\ccnet.SvnRevisionLabeller.plugin\bin\Debug or src\ccnet.SvnRevisionLabeller.plugin\bin\Release into the folder that CruiseControl.NET is running from, update ccnet.config with the appropriate configuration (see below), and restart the service.

Configuration
-------------

Below is a sample configuration for svnRevisionLabeller, showing the mandatory fields:

<labeller type="svnRevisionLabeller">
	<major>7</major>
	<minor>11</minor>
	<url>svn://localhost/repository/trunk</url>
</labeller>

The following sample configuration shows the complete set of fields:

<labeller type="svnRevisionLabeller">
	<prefix>Test</prefix>
	<major>8</major>
	<minor>2</minor>
	<url>svn://localhost/repository/branches/dev-project</url>
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

and the latest Subversion revision number is 920, the CCNetLabel will be set to 7.11.920.0. Forcing a build without any changes to the repository will increment the revision number, so that the label becomes 7.11.920.1. A subsequent commit to the repository would then set the label to 7.11.921.0, and so on. If a prefix is applied, that will need to be stripped from the label before creating AssemblyInfo.cs.

History
-------

1.0.3.25899
	* FIX - the username and password attributes are swapped around (fix provided by Tony Mitchell);

1.0.2.16573
	* Now built against CC.NET v1.3, and tested against CC.NET v1.3
	* FIX - the revision number does not increase on successive builds if a prefix is specified (fix provided by Mike Usner);
	* FIX - if no working copy exists, then the labeller will not be able to work out what the current revision is, and throw an exception (fix provided by Matteo Tontini);

1.0.1.21635
	* Built against CC.NET v1.1, and tested against CC.NET v1.1 and v2.0;
	* initial public release;

