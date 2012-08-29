using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ccnet.Labeller.plugin.Core;
using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core;

namespace ccnet.Labeller.plugin
{
    /// <summary>
    /// Take the entire build label from an assemblyinfo.cs - ie: any text file that has something like this in it [assembly: AssemblyVersion("4.0.0.*")]
    /// Use case is where production cc.net is pulling from a package repo that has the assemblyinfo.cs copied into the root 
    /// </summary>
    [ReflectorType("assemblyInfoLabeller")]
    public class AssemblyInfoLabeller : ILabeller
    {
        [ReflectorProperty("assemblyInfoPath", Required = false)]
        public string AssemblyInfoPath { get; set; }

        #region ILabeller Members

        public string Generate(IIntegrationResult integrationResult)
        {
            var assemblyInfoService = new AssemblyInfoService();
            var versionInfo = assemblyInfoService.ParseForVersionInfo(AssemblyInfoPath);
            return versionInfo.ToString();
        }

        #endregion

        #region ITask Members

        public void Run(IIntegrationResult result)
        {
            result.Label = Generate(result);
        }

        #endregion
    }
}
