using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.Diagnostics;
using System.IO;

using NLog;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.Attributes;
using KSDSLD.FiniteContexts.Features;
using KSDSLD.FiniteContexts.Models;
using KSDSLD.FiniteContexts.PatternVector;
using KSDSLD.FiniteContexts.Profiles;
using KSDSLD.FiniteContexts.Store;
using KSDSLD.Pipelines;
using KSDSLD.Util;


namespace KSDSLD.FiniteContexts.Classifiers
{
    public class ClassifierWEKA : Classifier
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public override string Name { get { return "WEKA"; } }
        public List<string> AdditionalFeaturesNotIncludedInParameters { get; private set; } = new List<string>();

        public ClassifierWEKA(User user, FiniteContextsConfiguration parameters, DirectoryInfo temp_folder)
            : base(user, parameters, temp_folder)
        {
        }

        public override void ClearClassifierCache()
        {
            if (File.Exists(SessionsFile))
                File.Delete(SessionsFile);
        }

        public override void Initialize()
        {
            CanAuthenticate = File.Exists(ModelFile);

            JAVA_PATH = System.Configuration.ConfigurationManager.AppSettings["path.java"];
            if (!File.Exists(JAVA_PATH))
            {
                log.Error("The java executable could not be found at '" + JAVA_PATH + "' (path.java).");
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
                log.Error("The jar file 'weka.jar' could not be found at '" + WEKA_PATH + "' (path.weka).");
                Environment.Exit(-1004);
            }
        }

        ARFF InitializeARFF(string name)
        {
            ARFF arff = new ARFF(name);
            arff.StartHeaders();

            foreach (var kv in AdditionalFeaturesNotIncludedInParameters)
                arff.AddNumericAttribute(kv);

            foreach (var kv in Parameters.NumericAttributes)
                arff.AddNumericAttribute(kv.Key);

            arff.AddCategory("RESULT", "legitimate,impostor");
            arff.StartData();
            return arff;
        }

        public string ModelFile
        {
            get
            {
                return TempFolder.FullName + "/" + Name + "." + User.UserID + ".model";
            }
        }

        public string SessionsFile
        {
            get
            {
                return TempFolder.FullName + "/" + Name + "." + User.UserID + ".sessions";
            }
        }

        public override void AppendTrainingSession(bool legitimate, Dictionary<string, double> method_values)
        {
            StreamWriter sw = new StreamWriter(SessionsFile, true);

            foreach (var kv in AdditionalFeaturesNotIncludedInParameters)
            {
                sw.Write(method_values[kv]);
                sw.Write(",");
            }

            foreach (var kv in Parameters.NumericAttributes)
            {
                sw.Write(method_values[kv.Key]);
                sw.Write(",");
            }

            sw.WriteLine(legitimate ? "legitimate" : "impostor");
            sw.Flush();
            sw.Close();
        }

        public static int MAX_CACHED_SESSIONS { get; set; } = 200;

        public ARFF LastTrainingSet { get; private set; }

        public override void SaveTraining(string path)
        {
            string src = LastTrainingSet.Filename + ".arff";
            File.Move(src, path);
        }


        public ARFF LastTestingSet { get; private set; }
        public ARFF LastEvaluationSet { get; private set; }

        public override void SaveLastEvaluation(string path)
        {
            string src = LastEvaluationSet.Filename + ".arff";
            File.Move(src, path);
        }


        string[] UpdateSessionsFile()
        {
            string[] lines = File.ReadAllLines(SessionsFile);
            if (lines.Length > MAX_CACHED_SESSIONS)
            {
                lines = lines.Skip(lines.Length - MAX_CACHED_SESSIONS).Take(MAX_CACHED_SESSIONS).ToArray();
                File.WriteAllLines(SessionsFile, lines);
            }

            return lines;
        }

        public ARFF GetFullARFF()
        {
            string[] lines = UpdateSessionsFile();

            ARFF retval = InitializeARFF(TempFolder.FullName + "/" + Name + "." + User.UserID + ".FULL");
            for (int i = 0; i < lines.Length; i++)
                retval.AppendLineVerbatim(lines[i]);

            retval.Close();
            return retval;
        }

        public ARFF LastCrossValidation { get; private set; }
        public BinaryConfusionMatrix CrossValidate(string kernel = null)
        {
            ARFF arff = GetFullARFF();
            LastCrossValidation = arff;

            string KERNEL = "weka.classifiers.functions.supportVector.PolyKernel -E 1.0 -C 250007";
            if (kernel != null)
                KERNEL = kernel;

            string[] lines = RunWEKA("-classpath " + WEKA_JAR + " weka.classifiers.functions.SMO -C 1.0 -L 0.001 -P 1.0E-12 -N 0 -V -1 -W 1 -K \"" + KERNEL + "\" -calibrator \"weka.classifiers.functions.Logistic -R 1.0E-8 -M -1 -num-decimal-places 4\" -t <%=training%>"
                .Replace("<%=training%>", "\"" + arff.Filename + ".arff\"") +
                (arff.InstanceCount < 10 ? (" -x " + arff.InstanceCount.ToString()) : ""));

            int ll, li, il, ii;
            ReadBinaryConfusionMatrix(lines, out ll, out li, out il, out ii);

            foreach (var line in lines)
                Console.WriteLine(line);

            return new BinaryConfusionMatrix(ll, li, ii, il);
        }

        public class CrossValidationInstanceDetail
        {
            public CrossValidationInstanceDetail(Sample session, string actual, string predicted)
            {
                Session = session;
                Actual = actual;
                Predicted = predicted;
            }

            public Sample Session { get; private set; }
            
            
            public string Actual { get; private set; }
            public string Predicted { get; private set; }
        }

        public CrossValidationInstanceDetail[] CrossValidateDetailed(Profile profile)
        {
            ARFF arff = GetFullARFF();
            LastCrossValidation = arff;

            string KERNEL = "weka.classifiers.functions.supportVector.PolyKernel -E 1.0 -C 250007";
            string[] lines = RunWEKA("-classpath " + WEKA_JAR + " weka.classifiers.functions.SMO -C 1.0 -L 0.001 -P 1.0E-12 -N 0 -V -1 -W 1 -K \"" + KERNEL + "\" -calibrator \"weka.classifiers.functions.Logistic -R 1.0E-8 -M -1 -num-decimal-places 4\" -t <%=training%>"
                .Replace("<%=training%>", "\"" + arff.Filename + ".arff\"") +
                (arff.InstanceCount < 10 ? (" -x " + arff.InstanceCount.ToString()) : "")
                + " -p 1,2,3");

            int i = 0;
            for (i = 0; i < lines.Length; i++)
                if (lines[i].Contains("predicted error prediction"))
                {
                    i++;
                    break;
                }

            List<CrossValidationInstanceDetail> retval = new List<CrossValidationInstanceDetail>();
            for (; i < lines.Length; i++)
            {
                string line = lines[i].Trim().Replace("+","");
                if (line == "")
                    break;

                while (line.Contains("  "))
                    line = line.Replace("  ", " ");

                string[] fields = line.Split(' ');
                string actual = fields[1].Split(':')[1];
                string predicted = fields[2].Split(':')[1];

                string[] str_attributes = fields[4].Replace("(", "").Replace(")", "").Split(',');
                double[] attributes = str_attributes.Select(a => double.Parse(a)).ToArray();

                var candidates = profile.LegitimateTraining;
                if (actual == "impostor")
                    candidates = profile.ImpostorTraining;

                Sample session = null;
                string[] keys = Parameters.NumericAttributes.Keys.ToArray();
                foreach (var candidate in candidates)
                {
                    int j = 0;
                    for (j = 0; j < 3; j++)
                        if (candidate.Value[keys[j]] < attributes[j] - 0.0001 || candidate.Value[keys[j]] > attributes[j] + 0.0001)
                            break;

                    if (j == 3)
                    {
                        session = candidate.Key;
                        break;
                    }
                }

                if (session == null)
                    throw new ArgumentException("INSTANCE NOT FOUND");
                else
                {
                    CrossValidationInstanceDetail tmp = new CrossValidationInstanceDetail(session, actual, predicted);
                    retval.Add(tmp);
                }
            }

            return retval.ToArray();
        }


        public bool DoTrainTestSplit { get; private set; } = false;
        public double TrainTestSplit { get; set; } = 0.75;

        public override void Retrain()
        {
            string[] lines = UpdateSessionsFile();

            ARFF data_train = InitializeARFF(TempFolder.FullName + "/" + Name + "." + User.UserID + ".TRAIN");
            LastTrainingSet = data_train;
            ARFF data_test = InitializeARFF(TempFolder.FullName + "/" + Name + "." + User.UserID + ".TEST");
            LastTestingSet = data_test;

            string[] lines_legitimate = lines.Where(l => l.Contains("legitimate")).ToArray();
            string[] lines_impostor = lines.Where(l => l.Contains("impostor")).ToArray();

            double train_test_split = DoTrainTestSplit ? TrainTestSplit : 1.0;
            for (int i = 0; i < lines_legitimate.Length; i++)

                if (i <= TrainTestSplit * lines_legitimate.Length)
                    data_train.AppendLineVerbatim(lines_legitimate[i]);
                else
                    data_test.AppendLineVerbatim(lines_legitimate[i]);

            for (int i = 0; i < lines_impostor.Length; i++)
                if (i <= TrainTestSplit * lines_impostor.Length)
                    data_train.AppendLineVerbatim(lines_impostor[i]);
                else
                    data_test.AppendLineVerbatim(lines_impostor[i]);

            data_train.Close();
            data_test.Close();

            if (File.Exists(ModelFile))
                File.Delete(ModelFile);

            TrainModel(data_train.Filename + ".arff", ModelFile);
            if (!File.Exists(ModelFile))
                throw new Exception("MODEL WAS NOT BUILT.");

            if (train_test_split != 1.0)
                EvaluateModel(data_test.Filename + ".arff", ModelFile);
            
            CanAuthenticate = true;
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

        void TrainModel(string training, string model)
        {
            Process p = new Process();
            p.StartInfo.FileName = JAVA_PATH;
            p.StartInfo.Arguments = "-classpath \"" + WEKA_JAR + "\" weka.classifiers.functions.SMO -C 1.0 -L 0.001 -P 1.0E-12 -N 0 -V -1 -W 1 -K \"weka.classifiers.functions.supportVector.PolyKernel -E 1.0 -C 250007\" -calibrator \"weka.classifiers.functions.Logistic -R 1.0E-8 -M -1 -num-decimal-places 4\" -t <%=training%> -d <%=model%>"
                .Replace("<%=training%>", "\"" + training + "\"")
                .Replace("<%=model%>", "\"" + model + "\"");

            if (!VerboseWEKACommands)
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            else
            {
                log.Info(p.StartInfo.FileName + " " + p.StartInfo.Arguments);
                p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            }

            p.Start();
            p.WaitForExit();
        }


        void ParseLine(string line, out int f1, out int f2)
        {
            while (line.Contains("  "))
                line = line.Replace("  ", " ");

            line = line.Trim().Replace(" ", "\t");
            string[] fields = line.Split('\t');
            f1 = int.Parse(fields[0]);
            f2 = int.Parse(fields[1]);
        }

        string[] RunWEKA(string command_line)
        {
            Process p = new Process();
            p.StartInfo.FileName = JAVA_PATH;
            p.StartInfo.Arguments = "-classpath \"" + WEKA_JAR + "\" " + command_line;

            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            if (!VerboseWEKACommands)
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            else
            {
                log.Info(p.StartInfo.FileName + " " + p.StartInfo.Arguments);
                p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            }

            p.Start();

            string stdout = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            string logname = TempFolder.FullName + "/WEKA." + User.UserID + ".TEST.log";
            File.WriteAllText(logname, stdout);

            return File.ReadAllLines(logname);
        }

        void EvaluateModel(string test, string model)
        {
            string[] lines = RunWEKA("weka.classifiers.functions.SMO -l <%=model%> -T <%=test%> "
                .Replace("<%=test%>", "\"" + test + "\"")
                .Replace("<%=model%>", "\"" + model + "\""));

            int ll, li, il, ii;
            ReadBinaryConfusionMatrix(lines, out ll, out li, out il, out ii);

            FRR = 100.0 * (double)li / (double)ll;
            FAR = 100.0 * (double)il / (double)ii;
            ExpectedErrorRate = 100.0 * (double)(li + il) / (double)(ll + li + il + ii);
        }

        void ReadBinaryConfusionMatrix(string[] lines, out int ll, out int li, out int il, out int ii)
        {
            int i = 0;
            for (i = lines.Length - 1; i >= 0 && !lines[i].Contains("Confusion Matrix"); i--) ;
            i += 3;

            ParseLine(lines[i], out ll, out li);
            ParseLine(lines[i + 1], out il, out ii);
        }

        public override bool IsLegitimate(Dictionary<string,double> numeric_attribute_values)
        {
            ARFF evaluation = InitializeARFF(TempFolder.FullName + "/" + Name + "." + User.UserID + ".EVALUATION");
            double[] values = new double[numeric_attribute_values.Count];

            int count = 0;
            foreach (var kv in Parameters.NumericAttributes)
            {
                values[count] = numeric_attribute_values[kv.Key];
                count++;
            }

            evaluation.AppendData(values);
            evaluation.AppendCategory("legitimate");
            evaluation.Close();

            string model_name = TempFolder.FullName + "/" + Name + "." + User.UserID + ".model";
            string[] lines = RunWEKA("weka.classifiers.functions.SMO -l <%=model%> -T <%=test%> -classifications PlainText"
                .Replace("<%=test%>", "\"" + evaluation.Filename + ".arff\"")
                .Replace("<%=model%>", "\"" + model_name + "\""));

            string l = lines[5].Trim();
            while (l.Contains("  ")) l = l.Replace("  ", " ");
            string[] fields = l.Split(' ');       

            return fields[2].Contains("legitimate");
        }

        string[] WriteLogFile(string stdout)
        {
            string logname = TempFolder.FullName + "/WEKA." + User.UserID + ".TEST.log";
            File.WriteAllText(logname, stdout);

            return File.ReadAllLines(logname);
        }


        public override bool[] AreLegitimate(Dictionary<string, double>[] numeric_attribute_values)
        {
            ARFF evaluation = InitializeARFF(TempFolder.FullName + "/" + Name + "." + User.UserID + ".EVALUATION");
            LastEvaluationSet = evaluation;

            for (int i = 0; i < numeric_attribute_values.Length; i++)
            {
                double[] values = new double[numeric_attribute_values[i].Count];

                int count = 0;
                foreach (var kv in Parameters.NumericAttributes)
                {
                    values[count] = numeric_attribute_values[i][kv.Key];
                    count++;
                }

                evaluation.AppendData(values);
                evaluation.AppendCategory("legitimate");
            }

            evaluation.Close();

            string model_name = TempFolder.FullName + "/" + Name + "." + User.UserID + ".model";
            string[] lines = RunWEKA("weka.classifiers.functions.SMO -l <%=model%> -T <%=test%> -classifications PlainText"
                .Replace("<%=test%>", "\"" + evaluation.Filename + ".arff\"")
                .Replace("<%=model%>", "\"" + model_name + "\""));

            int pos = 5;
            bool[] retval = new bool[numeric_attribute_values.Length];
            for (int i = 0; i < numeric_attribute_values.Length; i++, pos++)
            {
                string l = lines[pos].Trim();
                while (l.Contains("  ")) l = l.Replace("  ", " ");
                string[] fields = l.Split(' ');
                retval[i] = fields[2].Contains("legitimate");
            }

            return retval;
        }
    }
}
