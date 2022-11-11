using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

using NLog;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.Experiments.Distances;
using KSDSLD.FiniteContexts.Attributes;
using KSDSLD.FiniteContexts.Classifiers;
using KSDSLD.FiniteContexts.Features;
using KSDSLD.FiniteContexts.Models;
using KSDSLD.FiniteContexts.PatternVector;
using KSDSLD.FiniteContexts.Profiles;
using KSDSLD.FiniteContexts.Store;
using KSDSLD.Pipelines;
using KSDSLD.Util;


namespace KSDSLD.Experiments.FiniteContexts
{
    public partial class FiniteContextsExperiment : PipelineStage
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public FiniteContextsExperiment(Pipeline pipeline, PipelineStageConfigurationElement configuration)
            : base(pipeline, configuration)
        {
        }

        public FiniteContextsConfiguration Parameters
        {
            get; private set;
        }

        FiniteContextsExperimentConfigurationSection section;
        int TRAINING_SESSIONS = 50;
        public override void Initialize()
        {
            section = (FiniteContextsExperimentConfigurationSection)ConfigurationManager.GetSection(Configuration.Configuration);
            TRAINING_SESSIONS = section.InitialTrainingSessions;

            Parameters = new FiniteContextsConfiguration
                (
                    section.BiometricParameters,
                    section.Models,
                    section.Features,
                    section.Attributes
                );

            Parameters.Initialize();
        }


        int[] SelectAllTimings(Sample[] sessions, byte vk)
        {
            List<int> retval = new List<int>();
            for (int i = 0; i < sessions.Length; i++)
                for (int j = 0; j < sessions[i].VKs.Length; j++)
                    if (sessions[i].VKs[j] == vk)
                        retval.Add(sessions[i].Features[TypingFeature.HT][j]);

            return retval.ToArray();
        }

        void ForEachUser(Dataset dataset, int user_id, Sample[] sessions)
        {
            int training_sessions = (TRAINING_SESSIONS == -1 ? sessions.Length : TRAINING_SESSIONS);
            if (sessions.Length <= TRAINING_SESSIONS)
            {
                log.Warn("NOT ENOUGH TRAINING SESSIONS FOR USER {0}", user_id);
                return;
            }

            User user = User.GetUser(user_id);
            Classifier classifier = null;
            Profile profile = new Profile(user, Parameters, classifier, impostors);
            profile.BuildInitialProfile(sessions, training_sessions);

            if (Configuration.Action == "authenticate")
            {
                profile.EvaluateLegitimate(sessions, training_sessions, section.Retrain);
                List<Sample> selected_impostor_sessions = profile.EvaluateImpostors(sessions.Length - training_sessions);

                profile.Classifier.OnFalseRejection += Profile_OnFalseRejection;
                profile.Classifier.OnFalseAcceptance += Profile_OnFalseAcceptance;
                profile.RetrainClassifier();
                profile.CalculateEERs();
            }
        }

        private void Profile_OnFalseAcceptance(Authentication authentication)
        {
            log.Info("[{0}] FALSE ACCEPTANCE: {1}", authentication.Sample.User.UserID, authentication.Sample.GetSessionText());
            foreach (var method in authentication.MethodValues)
                log.Info("    {0,-10}: {1}", method.Key, method.Value);
        }

        private void Profile_OnFalseRejection(Authentication authentication)
        {
            log.Info("[{0}] FALSE REJECTION: {1}", authentication.Sample.User.UserID, authentication.Sample.GetSessionText());
            foreach (var method in authentication.MethodValues)
                log.Info("    {0,-10}: {1}", method.Key, method.Value);
        }

        void EvaluateUserSession(bool legitimate, Sample session, Builder[] builders, Dictionary<string, List<double>> attribute_values)
        {
            var numeric_attribute_values = CalculateAttributes(session, builders);
            foreach (var attr in numeric_attribute_values)
                attribute_values[attr.Key].Add(attr.Value);
        }

        Dictionary<string, List<double>> EvaluateLegitimate(Sample[] sessions, ModelFeeder feeder, Builder[] builders, int training_sessions)
        {
            Dictionary<string, List<double>> legitimate_values = new Dictionary<string, List<double>>();
            foreach (var attr in Parameters.NumericAttributes)
                legitimate_values.Add(attr.Key, new List<double>());

            for (int i = training_sessions; i < sessions.Length; i++)
            {
                Sample session = sessions[i];
                EvaluateUserSession(true, session, builders, legitimate_values);
                if (section.Retrain)
                    feeder.Feed(session, false);
            }

            return legitimate_values;
        }

        Dictionary<string, List<double>> EvaluateImpostors(List<Sample> selected_sessions, Sample[] all_sessions, int user_id, Builder[] builders, int count)
        {
            Dictionary<string, List<double>> impostor_values = new Dictionary<string, List<double>>();
            foreach (var attr in Parameters.NumericAttributes)
                impostor_values.Add(attr.Key, new List<double>());

            RNG rng = new RNG();
            for (int i = 0; i < count; i++)
            {
                int pos = rng.Next(all_sessions.Length);
                while (all_sessions[pos].User.UserID == user_id)
                    pos = rng.Next(all_sessions.Length);

                selected_sessions.Add(all_sessions[pos]);
                EvaluateUserSession(false, all_sessions[pos], builders, impostor_values);
            }

            return impostor_values;
        }

        bool MustCalculateEERs
        {
            get
            {
                return  Configuration.Action != "train" &&
                        section.CalculateEERs;
            }
        }

        Dictionary<string,double> CalculateEERs(Dictionary<string, List<double>> legitimate_values, Dictionary<string, List<double>> impostor_values)
        {
            if (!MustCalculateEERs)
                return null;

            Dictionary<string, double> retval = new Dictionary<string, double>();
            foreach (var kv in legitimate_values)
            {
                double[] legitimate = legitimate_values[kv.Key].ToArray();
                double[] impostor = impostor_values[kv.Key].ToArray();

                double deer, deer_value;
                FindEER(legitimate, impostor, out deer, out deer_value);
                retval.Add(kv.Key, deer);
            }

            return retval;
        }

        object arff_lock = new object();

        int count_identify_sessions = 0;
        DateTime last_count_identify_sessions = DateTime.MinValue;
        void IdentifyForEachSession(Dataset dataset, Sample session, User[] users, ARFF arff, CsvWriter csv)
        {
            Console.Write(".");
            count_identify_sessions++;
            if ((count_identify_sessions % 100) == 0)
            {
                DateTime now = DateTime.Now;
                if (last_count_identify_sessions != DateTime.MinValue)
                {
                    Console.WriteLine();
                    log.Info("  {0} sessions/sec", Math.Round(100.0 / (now - last_count_identify_sessions).TotalSeconds));
                }

                last_count_identify_sessions = now;
            }

            List<double> all_user_attributes = new List<double>();
            for (int i = 0; i < users.Length; i++)
            {
                Builder[] builders = (Builder[]) users[i].Properties["builders"];
                var values = CalculateAttributes(session, builders);
                foreach (var kv in values)
                    all_user_attributes.Add(kv.Value);
            }

            double[] tmp = all_user_attributes.ToArray();
            lock (arff_lock)
            {
                arff.AppendData(tmp);
                arff.AppendCategory("U" + session.User.UserID);
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < tmp.Length; i++)
            {
                sb.Append(tmp[i]);
                sb.Append(",");
            }

            sb.Append("U" + session.User.UserID);

            List<object> tmp2 = new List<object>();
            for (int i = 0; i < tmp.Length; i++)
                tmp2.Add(tmp[i]);

            tmp2.Add("U" + session.User.UserID);
            csv.WriteLine(tmp2.ToArray());
        }

        void Identify(Results results)
        {
            log.Info("Identification task started...");
            int[] user_ids = results.Datasets[0].SessionsByUser.Keys.ToArray();

            User[] users = new User[user_ids.Length];
            for (int i = 0; i < user_ids.Length; i++)
                users[i] = User.GetUser(user_ids[i]);

            CsvWriter csv = new CsvWriter("Output/LSIA_CSV", users.Length * Parameters.NumericAttributes.Count + 1);
            List<string> headers = new List<string>();

            StringBuilder categories = new StringBuilder();
            ARFF arff = new ARFF("Output/LSIA_IDENTIFY");
            arff.StartHeaders();
            for (int i = 0; i < users.Length; i++)
            {
                if (categories.Length != 0)
                    categories.Append(",");

                categories.Append("U");
                categories.Append(users[i].UserID);

                foreach (var kv in Parameters.NumericAttributes)
                {
                    string attr = "U" + users[i].UserID + "_" + kv.Key;
                    arff.AddNumericAttribute(attr);
                    headers.Add(attr);
                }
            }

            headers.Add("user");
            csv.WriteLine(headers.ToArray());

            arff.AddCategory("user", categories.ToString());
            arff.StartData();
            ExperimentParallelization.ForEachSession(results, (d, s) => IdentifyForEachSession(d, s, users, arff, csv));
            arff.Close();
        }

        Sample[] impostors;
        protected override void DoRun(Results results)
        {
            if (results.Datasets.Length == 1)
                impostors = results.Datasets[0].Samples;
            else
                impostors = results.Datasets[1].Samples;

            results.KeepFirst();
            log.Info("Running finite contexts distance experiment ({0})...", Configuration.Configuration);
            ExperimentParallelization.ForEachUser(results, ForEachUser);            

            if (Configuration.Action == "identify")
                Identify(results);
        }

        void FindEER(double[] legitimate, double[] impostor, out double eer, out double eer_value)
        {
            Array.Sort(legitimate);
            Array.Sort(impostor);

            int ipos = 0;
            for (int i = 0; i < legitimate.Length; i++)
            {
                while (ipos < impostor.Length && impostor[ipos] <= legitimate[i])
                    ipos++;

                double frr = (double) (legitimate.Length - i - 1) / (double) legitimate.Length;
                double far = (double) ipos / (double) impostor.Length;

                if ( far >= frr )
                {
                    eer = (far + frr) / 2.0;

                    if (ipos >= impostor.Length) ipos = impostor.Length - 1;
                    eer_value = (legitimate[i] + impostor[ipos]) / 2.0;
                    return;
                }
            }

            throw new ArgumentException("Unable to calculate EER");
        }

        int blg = 0;
        DateTime last = DateTime.Now;
        Dictionary<string, double> CalculateAttributes(Sample session, Builder[] builders)
        {
            int clusp = System.Threading.Interlocked.Increment(ref blg);
            if ((clusp % 100) == 0)
            {
                DateTime cnow = DateTime.Now;
                // log.Info("  {0} sessions/second", Math.Round(100.0 / (cnow - last).TotalSeconds,2));
                last = cnow;
            }

            Dictionary<string, object> features_dictionary = new Dictionary<string, object>();
            RunForTypingFeature(session, features_dictionary, builders);

            Dictionary<string, double> numeric_attribute_values = new Dictionary<string, double>();
            foreach (var attr in Parameters.NumericAttributes)
                // try
                {
                    double tmp = attr.Value.GetValue(features_dictionary, numeric_attribute_values);
                    numeric_attribute_values.Add(attr.Key, tmp);
                }
                /*
                catch (NotImplementedException)
                {

                }
                */

            return numeric_attribute_values;
        }

        void RunForTypingFeature
            (
                Sample session, 
                Dictionary<string,object> features_dictionary,
                Builder[] builders
            )
        {
            var patterns = builders.Rebuild(session);
            foreach (Feature feature in Parameters.Features)
            {
                string[] fields = feature.Configuration.Pattern.Split(';');
                foreach (string field in fields)
                {
                    FeatureParameters parameters = new FeatureParameters
                    (
                        features_dictionary,
                        session,
                        field,
                        patterns[field].Value,
                        patterns[field].Key,
                        session.Features[patterns[field].Key]
                    );

                    feature.CalculateFeatures(parameters);
                }
            }
        }

        StreamWriter sw;
        private void Builder_OnModelChosen(TypingFeature feature, Sample session, int pos, ulong context, AvgStdevModel[] candidates, AvgStdevModel chosen)
        {
            sw.Write(string.Format("{0,-8}", pos));
            sw.Write(session.VKs[pos].ToString("X").PadLeft(2, '0'));
            sw.Write("    ");
            sw.WriteLine(context.ToString("X"));

            sw.Write("        ");
            for (int i = 0; i < candidates.Length; i++)
                if (candidates[i] == null)
                    sw.Write("X  ");
                else
                {
                    sw.Write(candidates[i].ToString());
                    sw.Write("  ");
                }

            sw.WriteLine();
            sw.Write("        ");
            if (chosen != null)
                sw.WriteLine(chosen.ToString());
            else
                sw.WriteLine("XXX");            
        }
    }
}
