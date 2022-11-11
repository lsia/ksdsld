using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class FiniteContextsExperimentConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("retrain", IsRequired = true)]
        public bool Retrain
        {
            get
            {
                return (bool) this["retrain"];
            }
            set
            {
                this["retrain"] = value;
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

        [ConfigurationProperty("biometricParameters", IsRequired = true)]
        public string BiometricParameters
        {
            get
            {
                return (string)this["biometricParameters"];
            }
            set
            {
                this["biometricParameters"] = value;
            }
        }

        [ConfigurationProperty("models", IsRequired = true)]
        public string Models
        {
            get
            {
                return (string)this["models"];
            }
            set
            {
                this["models"] = value;
            }
        }

        [ConfigurationProperty("features", IsRequired = true)]
        public string Features
        {
            get
            {
                return (string)this["features"];
            }
            set
            {
                this["features"] = value;
            }
        }

        [ConfigurationProperty("attributes", IsRequired = true)]
        public string Attributes
        {
            get
            {
                return (string)this["attributes"];
            }
            set
            {
                this["attributes"] = value;
            }
        }

        [ConfigurationProperty("calculateEERs", IsRequired = false)]
        public bool CalculateEERs
        {
            get
            {
                return (bool) this["calculateEERs"];
            }
            set
            {
                this["calculateEERs"] = value;
            }
        }

        [ConfigurationProperty("profileBehavior", IsRequired = false)]
        public string ProfileBehavior
        {
            get
            {
                return (string) this["profileBehavior"];
            }
            set
            {
                this["profileBehavior"] = value;
            }
        }

        [ConfigurationProperty("trainingOrder", IsRequired = false)]
        public string TrainingOrder
        {
            get
            {
                return (string)this["trainingOrder"];
            }
            set
            {
                this["trainingOrder"] = value;
            }
        }

        [ConfigurationProperty("minTrainingSessions", IsRequired = false)]
        public int MinTrainingSessions
        {
            get
            {
                return (int) this["minTrainingSessions"];
            }
            set
            {
                this["minTrainingSessions"] = value;
            }
        }

        [ConfigurationProperty("minUserSessions", IsRequired = false)]
        public int MinUserSessions
        {
            get
            {
                return (int)this["minUserSessions"];
            }
            set
            {
                this["minUserSessions"] = value;
            }
        }

        [ConfigurationProperty("maxTrainingSessions", IsRequired = false)]
        public int MaxTrainingSessions
        {
            get
            {
                return (int)this["maxTrainingSessions"];
            }
            set
            {
                this["maxTrainingSessions"] = value;
            }
        }

        [ConfigurationProperty("maxUserSessions", IsRequired = false)]
        public int MaxUserSessions
        {
            get
            {
                return (int)this["maxUserSessions"];
            }
            set
            {
                this["maxUserSessions"] = value;
            }
        }

        [ConfigurationProperty("doNotEvaluateSessions", IsRequired = false)]
        public bool DoNotEvaluateSessions
        {
            get
            {
                return (bool)this["doNotEvaluateSessions"];
            }
            set
            {
                this["doNotEvaluateSessions"] = value;
            }
        }
    }
}
