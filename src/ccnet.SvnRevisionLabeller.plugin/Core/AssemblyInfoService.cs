using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ccnet.Labeller.plugin.Core
{
    class AssemblyInfoService
    {
        /// <summary>
        /// Extract - if available - the major/minor from the specified path
        /// </summary>
        public VersionInfo ParseForVersionInfo(string assemblyInfoFilePathCsv)
        {
            var versionInfo = new VersionInfo();

            if (!string.IsNullOrEmpty(assemblyInfoFilePathCsv))
            {
                string[] paths = assemblyInfoFilePathCsv.Split(',');
                int pathIndex = 0;
                bool versionFound = false;

                while (!versionFound && pathIndex < paths.Length)
                {
                    Log.Info("{0} paths will be checked for major/minor version info", paths.Length);

                    var testingPath = paths[pathIndex];
                    if (File.Exists(testingPath))
                    {
                        Log.Info("Assembly Major/Minor parsing: Opening ({1}) '{0}'", testingPath, pathIndex);
                        string fileContents = File.ReadAllText(testingPath);

                        Regex regex = new Regex(@"AssemblyVersion\x28\""([\d\.\*]+)\""\x29");
                        Match match = regex.Match(fileContents);

                        if (match.Success)
                        {
                            versionInfo = new VersionInfo(match.Groups[1].Value);
                            versionFound = true;
                            Log.Info("Parsed out '{0}'", versionInfo);
                        }
                        else
                        {
                            Log.Info("AssemblyInfoService: No version info found ({1}) '{0}'", testingPath, pathIndex);
                        }
                    }
                    else
                    {
                        Log.Info("Assembly Major/Minor parsing: File not found ({1}) '{0}'", testingPath, pathIndex);
                    }
                    pathIndex++;
                }
            }

            return versionInfo;
        }
    }
}
