using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using NLog;


namespace KSDSLD.Experiments
{
    static class ExperimentUtil
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        static string[] __CommandLineParameters = null;
        public static string[] CommandLineParameters
        {
            get
            {
                return __CommandLineParameters;
            }
            set
            {
                if (__CommandLineParameters != null)
                    throw new Exception("The command line parameters have already been set in ExperimentUtil.");

                __CommandLineParameters = value;
            }
        }

        static string __CurrentExperimentName = null;
        public static string CurrentExperimentName
        {
            get
            {
                if (__CurrentExperimentName != null)
                    return __CurrentExperimentName;

                string retval = null;

                Assembly assembly = Assembly.GetExecutingAssembly();
                foreach (var type in assembly.GetTypes())
                {
                    object attr = type.GetCustomAttribute(typeof(DefaultExperimentAttribute));
                    if (attr != null)
                        return type.Name;
                }

                if (retval == null)
                {
                    log.Error("NO EXPERIMENT SPECIFIED.");
                    Environment.Exit(-1);
                }

                __CurrentExperimentName = retval;
                return retval;
            }

            set
            {
                __CurrentExperimentName = value;
            }
        }

        public static Type CurrentExperimentType
        {
            get
            {
                var candidates = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.Name == CurrentExperimentName);
                if (candidates.Count() == 0)
                    throw new ArgumentException("No experiment named " + CurrentExperimentName + " found.");
                else if (candidates.Count() > 1)
                    throw new ArgumentException("Ambiguous experiment name " + CurrentExperimentName + ".");

                return candidates.First();
            }
        }

        public static string OutputBaseFolder
        {
            get
            {
                return "Output/";
            }
        }

        public static string OutputFolder
        {
            get
            {
                ConfigurationFileAttribute attr = CurrentExperimentType.GetCustomAttribute<ConfigurationFileAttribute>();
                if (attr == null || !attr.IncludeConfigurationAsFolder)
                    return OutputBaseFolder + CurrentExperimentName + "/";
                else
                    return OutputBaseFolder + CurrentExperimentName + "/" + attr.Name.Replace(".config","") + "/";
            }
        }

        static bool errors_found = false;
        public static bool ErrorsFound
        {
            get
            {
                return errors_found;
            }
        }

        public static void SetErrorsFound()
        {
            errors_found = true;
        }
    }
}
