using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class SessionParametersConfigurationSection : ConfigurationSection
    {
        public static SessionParametersConfigurationSection GetSection(string name)
        {
            return (SessionParametersConfigurationSection) ConfigurationManager.GetSection(name);
        }

        [ConfigurationProperty("minSessionLength", IsRequired = true)]
        public int MinSessionLength
        {
            get
            {
                return (int) this["minSessionLength"];
            }
            set
            {
                this["minSessionLength"] = value;
            }
        }

        [ConfigurationProperty("minAlphanumericRate", IsRequired = true)]
        public double MinAlphanumericRate
        {
            get
            {
                return (double) this["minAlphanumericRate"];
            }
            set
            {
                this["minAlphanumericRate"] = value;
            }
        }

        [ConfigurationProperty("minAlphanumericVariety", IsRequired = true)]
        public int MinAlphanumericVariety
        {
            get
            {
                return (int)this["minAlphanumericVariety"];
            }
            set
            {
                this["minAlphanumericVariety"] = value;
            }
        }

        [ConfigurationProperty("initialTrainingSessions", IsRequired = true)]
        public int InitialTrainingSessions
        {
            get
            {
                return (int)this["initialTrainingSessions"];
            }
            set
            {
                this["initialTrainingSessions"] = value;
            }
        }

        [ConfigurationProperty("retrainClassifierAfter", IsRequired = true)]
        public int RetrainClassifierAfter
        {
            get
            {
                return (int) this["retrainClassifierAfter"];
            }
            set
            {
                this["retrainClassifierAfter"] = value;
            }
        }
    }
}
