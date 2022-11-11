using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class SessionReportConfigurationSection : ConfigurationSection
    {
        public static SessionReportConfigurationSection GetSection()
        {
            return (SessionReportConfigurationSection) ConfigurationManager.GetSection("sessionReport");
        }

        [ConfigurationProperty("createLatexReport", IsRequired = true)]
        public bool CreateLatexReport
        {
            get
            {
                return (bool)this["createLatexReport"];
            }
            set
            {
                this["createLatexReport"] = value;
            }
        }

        [ConfigurationProperty("showDetailedModelSelection", IsRequired = true)]
        public bool ShowDetailedModelSelection
        {
            get
            {
                return (bool)this["showDetailedModelSelection"];
            }
            set
            {
                this["showDetailedModelSelection"] = value;
            }
        }

        [ConfigurationProperty("showSessionAttributes", IsRequired = true)]
        public bool ShowSessionAttributes
        {
            get
            {
                return (bool)this["showSessionAttributes"];
            }
            set
            {
                this["showSessionAttributes"] = value;
            }
        }
    }
}
