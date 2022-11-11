using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.Diagnostics;
using System.IO;

using NLog;

using KSDSLD.Util;
using NLog.Internal;

namespace KSDSLD.FiniteContexts.Classifiers
{
    enum AttributeRankingMethods
    {
        CorrelationAttributeEval,
        GainRatioAttributeEval,
        InfoGainAttributeEval,
        OneRAttributeEval,
        SymmetricalUncertAttributeEval
    }

    class WEKA
    {
        static Logger log = LogManager.GetCurrentClassLogger();
        public WEKA()
        {
        }

        string JAVA_PATH;
        string WEKA_PATH;
        string WEKA_JAR;

        bool VerboseWEKACommands
        {
            get
            {
                return ConfigUtil.GetBoolSetting("verbose.WEKAcommands", false);
            }
        }

        public string[] RunWEKA(string command_line, string logname)
        {
            if (JAVA_PATH == null)
            {
                JAVA_PATH = System.Configuration.ConfigurationManager.AppSettings["path.java"];
                if (!Directory.Exists(JAVA_PATH))
                {
                    log.Error("The path '" + JAVA_PATH + "' does not exist (path.java).");
                    Environment.Exit(-1001);
                }
                if (!File.Exists(JAVA_PATH + "\\javaw.exe"))
                {
                    log.Error("The java executable 'javaw.exe' could be found at '" + JAVA_PATH + "' (path.java).");
                    Environment.Exit(-1002);
                }

                WEKA_PATH = System.Configuration.ConfigurationManager.AppSettings["path.weka"];
                if (!Directory.Exists(WEKA_PATH))
                {
                    log.Error("The path '" + WEKA_PATH + "' does not exist (path.weka).");
                    Environment.Exit(-1003);
                }
                WEKA_JAR = WEKA_PATH + "\\weka.jar";
                if (!File.Exists(WEKA_JAR))
                {
                    log.Error("The jar file 'weka.jar' could be found at '" + WEKA_PATH + "' (path.weka).");
                    Environment.Exit(-1004);
                }
            }

            Process p = new Process();
            p.StartInfo.FileName = JAVA_PATH;
            p.StartInfo.Arguments = "-classpath " + WEKA_JAR + " " + command_line;

            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            if (VerboseWEKACommands)
                log.Info(p.StartInfo.Arguments);

            p.Start();

            string stdout = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            File.WriteAllText(logname, stdout);

            return File.ReadAllLines(logname);
        }

        public string[] SelectAttributesCfsSubset(string arff_file)
        {
            string COMMAND_LINE = "weka.attributeSelection.CfsSubsetEval -P 1 -E 1 " +
                "-s \"weka.attributeSelection.GreedyStepwise -T -1.7976931348623157E308 -N -1 -num-slots 1\" -i " +
                arff_file;

            string logname = "Output/WEKA.STDOUT.SelectAttributes." + Path.GetFileName(arff_file) + ".stdout";
            string[] lines = RunWEKA(COMMAND_LINE, logname);

            int i = 0;
            for (i = 0; i < lines.Length; i++)
                if (lines[i].Contains("Selected attributes"))
                {
                    i++;
                    break;
                }

            List<string> retval = new List<string>();
            for (; i < lines.Length; i++)
            {
                string attribute = lines[i].Trim();
                if (attribute != "")
                    retval.Add(attribute);
            }

            return retval.ToArray();
        }



        public KeyValuePair<string, double>[] RankAttributes(AttributeRankingMethods method, string arff_file)
        {
            string COMMAND_LINE = "weka.attributeSelection." + method.ToString() +
                " -s \"weka.attributeSelection.Ranker -T -1.7976931348623157E308 -N -1\" -i " +
                arff_file;

            string logname = "Output/WEKA.STDOUT.SelectAttributes." + Path.GetFileName(arff_file) + ".stdout";
            string[] lines = RunWEKA(COMMAND_LINE, logname);

            int i = 0;
            for (i = 0; i < lines.Length; i++)
                if (lines[i].Contains("Ranked attributes"))
                {
                    i++;
                    break;
                }

            List<KeyValuePair<string, double>> retval = new List<KeyValuePair<string, double>>();
            for (; i < lines.Length; i++)
            {
                if (lines[i].Contains("Selected"))
                    break;

                string attribute = lines[i].Trim();
                if (attribute != "")
                {
                    while (attribute.Contains("  "))
                        attribute = attribute.Replace("  ", " ");

                    string[] fields = attribute.Split(' ');
                    retval.Add(new KeyValuePair<string, double>(fields[2], double.Parse(fields[0])));
                }
            }

            return retval.ToArray();
        }
    }
}
