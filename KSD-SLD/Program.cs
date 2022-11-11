using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;

using NLog;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.Datasets.Readers;
using KSDSLD.Datasets.Summarizers;
using KSDSLD.Experiments;
using KSDSLD.FiniteContexts.Models;
using KSDSLD.Pipelines;
using KSDSLD.Util;

using KSDSLD.FiniteContexts.Partitions;
using KSDSLD.FiniteContexts.Profiles;
using KSDSLD.FiniteContexts.Util;
using KSDSLD.FiniteContexts.Synthesizer;
using System.Runtime.InteropServices;
using KSDSLD.FiniteContexts.Classifiers;
using System.Security.Cryptography;

namespace KSDSLD
{
    class Program
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        User user = User.GetUser(1, "DEFAULT", DateTime.MinValue, Gender.Unknown);
        FiniteContextsHelper FCH = new FiniteContextsHelper();
        void Synthesize(Dataset user_samples, Dataset target_samples, string method)
        {
            FCH.InitializeParameters("finiteContextsExperiment");
            log.Info("Synthesizing target samples...");
            log.Info("    Creating user profile...");

            Classifier classifier = new NullClassifier(user, FCH.Parameters, new DirectoryInfo(Environment.CurrentDirectory));
            Profile profile = new Profile(user, FCH.Parameters, classifier, null);
            profile.BuildInitialProfile(user_samples.Samples, user_samples.Samples.Length);

            KeystrokeDynamicsSynthesizer synthesizer = null;
            if (method.ToUpper() == "AVERAGE")
                synthesizer = new AverageSynthesizer(profile);
            else if (method == "UNIFORM")
                synthesizer = new UniformSynthesizer(profile);
            else if (method == "GAUSSIAN")
                synthesizer = new GaussianSynthesizer(profile);
            else if (method == "HISTOGRAM" || method == "DEFAULT")
                synthesizer = new HistogramSynthesizer(profile);
            else if (method == "NSHISTOGRAM")
                synthesizer = new NonStationaryHistogramSynthesizer(profile);
            else
                Error("Unrecognized synthesizer '" + method + "' (valid synthesizers are Average, Uniform, Gaussian, Histogram, and NSHistogram");

            log.Info("    Method: " + method.ToUpper());
            foreach (var target_sample in target_samples.Samples)
            {
                Sample session = synthesizer.Synthesize(target_sample.VKs);

                StreamWriter sw = new StreamWriter(target_samples.Filename + "\\" + target_sample.ID + "-SYNTHEZISED-" + method.ToUpper() + ".csv");
                sw.WriteLine("VK,HT,FT");
                for (int i = 0; i < session.Length; i++)
                {
                    sw.Write(session.VKs[i]);
                    sw.Write(",");
                    sw.Write(session.HTs[i]);
                    sw.Write(",");
                    sw.Write(session.FTs[i]);
                    sw.WriteLine();
                }
                sw.Close();
            }

            log.Info("Done.");
        }

        KeystrokeDynamicsSynthesizer ChooseSynthesizer(Profile profile)
        {
            double next = RNG.Instance.NextDouble();
            if (next < 0.6)
                return new HistogramSynthesizer(profile);
            else if (next < 0.8)
                return new AverageSynthesizer(profile);
            else
                return new GaussianSynthesizer(profile);
        }

        Profile BuildLivenessDetectionModel(Dataset user_samples)
        {
            log.Info("Building liveness detection model...");
            log.Info("    Creating user profile...");

            Classifier classifier = new NullClassifier(user, FCH.Parameters, new DirectoryInfo(Environment.CurrentDirectory));
            Profile profile = new Profile(user, FCH.Parameters, classifier, null);
            profile.BuildInitialProfile(user_samples.Samples, user_samples.Samples.Length);

            log.Info("    Synthesizing evil bot samples...");
            var impostor_training_samples = new List<Sample>();
            foreach (var session in user_samples.Samples)
            {
                var synthesizer = ChooseSynthesizer(profile);
                Sample training_sample = synthesizer.Synthesize(session);
                impostor_training_samples.Add(training_sample);
            }

            log.Info("    Training model...");
            var model = FCH.CreateProfile(user_samples.Samples, impostor_training_samples.ToArray(), true);

            log.Info("    Evaluating model...");
            var evaluation_impostor_samples = new List<Sample>();
            foreach (var sample in user_samples.Samples)
            {
                var synthesizer = ChooseSynthesizer(profile);
                Sample far_sample = synthesizer.Synthesize(sample);
                evaluation_impostor_samples.Add(far_sample);
            }

            int far_count = 0;
            int frr_count = 0;

            foreach (var auth in model.AuthenticateWithoutRetrain(user_samples.Samples))
                if (!auth.Legitimate)
                    frr_count++;

            foreach (var auth in model.AuthenticateWithoutRetrain(evaluation_impostor_samples.ToArray()))
                if (auth.Legitimate)
                    far_count++;

            double FAR = Math.Round(100.0 * far_count / user_samples.Samples.Length);
            log.Info("       FAR: " + FAR + "%");
            double FRR = Math.Round(100.0 * frr_count / user_samples.Samples.Length);
            log.Info("       FRR: " + FRR + "%");

            return model;
        }

        void Verify(Dataset user_samples, Dataset target_samples)
        {
            FCH.InitializeParameters("finiteContextsExperiment");
            var model = BuildLivenessDetectionModel(user_samples);

            log.Info("Verifying target samples...");
            StreamWriter sw = File.CreateText("RESULTS.csv");
            sw.WriteLine("sample,verdict");

            foreach (var auth in model.AuthenticateWithoutRetrain(target_samples.Samples))
            {
                sw.Write(auth.Sample.Filename);
                sw.Write(",");
                sw.WriteLine(auth.Legitimate ? "legitimate" : "impostor");

                if (auth.Legitimate)
                    log.Info("    legitimate  " + auth.Sample.Filename);
                else
                    log.Info("    impostor    " + auth.Sample.Filename);
            }

            sw.Close();
            log.Info("Done.");
        }

        void Help()
        {
            Console.WriteLine("KSD-SLD.exe {user_samples_folder} {target_samples_folder} {SYNTHESIZE|VERIFY} [method]");
            Environment.Exit(-1);
        }

        void Error(string message)
        {
            Console.WriteLine("ERROR!!! " + message);
            Environment.Exit(-1);
        }

        Dataset LoadDataset(string name, string folder)
        {
            if (!Directory.Exists(folder))
                Error("The " + name + " folder " + folder + " does not exist.");

            CsvDatasetReader reader = new CsvDatasetReader();
            Dataset samples = reader.ReadDataset(folder, name, folder);
            if (samples.Samples.Length == 0)
                Error("There are no samples in the " + name + " folder.");

            log.Info("Cleaning samples...");
            foreach (var sample in samples.Samples)
            {
                ThresholdPartitioner.ProcessSessionWithDefaultValues(sample);
                CleanFTs.ProcessSession(sample);
            }

            return samples;
        }

        void Run(string[] args)
        {
            string[] ARGS = args;
            // string[] ARGS = { "c:\\repos\\dataset\\2078", "c:\\repos\\dataset\\S2078", "SYNTHESIZE", "HISTOGRAM" };
            // string[] ARGS = { "c:\\repos\\dataset\\2078", "c:\\repos\\dataset\\2078", "VERIFY" };

            if (ARGS.Length < 3 || ARGS.Length > 4)
                Help();

            string user_samples_folder = ARGS[0];
            string target_samples_folder = ARGS[1];
            string operation = ARGS[2];
            string method = "DEFAULT";
            if (ARGS.Length > 3)
                method = ARGS[3];

            if (operation != "SYNTHESIZE" && operation != "VERIFY")
                Help();

            Dataset user_samples = LoadDataset("user samples", user_samples_folder);
            Dataset target_samples = LoadDataset("target samples", target_samples_folder); 

            if (operation == "SYNTHESIZE")
                Synthesize(user_samples, target_samples, method);
            else if (operation == "VERIFY")
                Verify(user_samples, target_samples);
            else
                Help();
        }

        static void Main(string[] args)
        {
            try
            {
                Program p = new Program();
                p.Run(args);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
