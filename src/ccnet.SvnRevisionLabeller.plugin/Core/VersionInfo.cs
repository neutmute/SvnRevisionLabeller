using System;

namespace ccnet.Labeller.plugin.Core
{
    public class VersionInfo
    {
        #region Properties

        public int Major { get; set; }
        public int Minor { get; set; }
        public int Build { get; set; }
        public int Revision { get; set; }


        public bool IsMajorValid { get; set; }
        public bool IsMinorValid { get; set; }
        public bool IsBuildValid { get; set; }
        public bool IsRevisionValid { get; set; }

        #endregion

        #region Ctor

        public VersionInfo()
        {
            
        }

        public VersionInfo(string version)
        {
            string[] splitVersion = version.Split('.');
            Major = Convert.ToInt32(splitVersion[0]);
            IsMajorValid = true;

            Minor = Convert.ToInt32(splitVersion[1]);
            IsMinorValid = true;

            int build;
            IsBuildValid = int.TryParse(splitVersion[2], out build);
            Build = build;

            if (splitVersion.Length > 3)
            {
                int revision;
                IsRevisionValid = int.TryParse(splitVersion[3], out revision);
                Revision = revision;
            }
        }

        #endregion

        #region Methods
        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}.{3}", Major, Minor, Build, Revision);
        }
        #endregion
    }
}