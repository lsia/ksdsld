using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


namespace KSDSLD.FiniteContexts.Profiles
{
    public class Profile
    {
        public User User { get; private set; }
        public FiniteContextsConfiguration Parameters { get; private set; }
        public Classifier Classifier { get; private set; }
        public Sample[] Impostors { get; private set; }

        public Profile
        (
            User user, 
            FiniteContextsConfiguration parameters, 
            Classifier classifier,
            Sample[] impostors
        )
        {
            User = user;
            Parameters = parameters;
            Classifier = classifier;
            Impostors = impostors;

            EERs = new Dictionary<string, ErrorMetrics>();
            TrainingSessions = new HashSet<Sample>();
        }

        public ModelFeeder Feeder { get; private set; }
        public Builder[] Builders { get; private set; }

        public Builder GetModelBuilder(TypingFeature feature, string name)
        {
            return Builders.Where(b => b.Storage.Feature == feature && b.Storage.Name == name).First();
        }

        public HashSet<Sample> TrainingSessions { get; private set; }
        public void BuildInitialProfile(Sample[] sessions, int training_sessions)
        {
            Feeder = Parameters.CreateFeeder(User);
            for (int i = 0; i < training_sessions; i++)
            {
                Feeder.Feed(sessions[i], true);
                TrainingSessions.Add(sessions[i]);
            }

            Builders = Feeder.GetBuilders();
        }

        public void BuildEmptyInitialProfile()
        {
            Feeder = Parameters.CreateFeeder(User);
            Builders = Feeder.GetBuilders();
        }

        public void EvaluateLegitimate(Sample[] sessions, int training_sessions, bool retrain)
        {
            for (int i = training_sessions; i < sessions.Length; i++)
            {
                Sample session = sessions[i];
                var numeric_attribute_values = EvaluateUserSession(true, true, session);
                legitimate_numeric_attribute_values.Add(numeric_attribute_values);
                if (legitimate_numeric_attribute_values.Count > 100)
                    legitimate_numeric_attribute_values.RemoveAt(0);

                LegitimateTraining.Add(new KeyValuePair<Sample, Dictionary<string, double>>(session, numeric_attribute_values));

                if (retrain)
                {
                    Feeder.Feed(session, false);
                    TrainingSessions.Add(sessions[i]);
                }

                Classifier.AppendTrainingSession(true, numeric_attribute_values);
            }
        }

        RNG rng = new RNG();
        Sample GetRandomImpostor(User user)
        {
            int pos = rng.Next(Impostors.Length);
            while (Impostors[pos].User.UserID == User.UserID)
                pos = rng.Next(Impostors.Length);

            return Impostors[pos];
        }

        List<Dictionary<string, double>> legitimate_numeric_attribute_values = new List<Dictionary<string, double>>();

        public List<KeyValuePair<Sample, Dictionary<string, double>>> LegitimateTraining { get; private set; }
            = new List<KeyValuePair<Sample, Dictionary<string, double>>>();

        List<Dictionary<string, double>> impostor_numeric_attribute_values = new List<Dictionary<string, double>>();

        public List<KeyValuePair<Sample, Dictionary<string, double>>> ImpostorTraining { get; private set; }
            = new List<KeyValuePair<Sample, Dictionary<string, double>>>();


        const int MAX_METAMODEL_SESSIONS = 100;
        public List<Sample> EvaluateImpostors(int count)
        {
            List<Sample> selected_sessions = new List<Sample>();

            for (int i = 0; i < count; i++)
            {
                int pos = rng.Next(Impostors.Length);
                while (Impostors[pos].User.UserID == User.UserID)
                    pos = rng.Next(Impostors.Length);

                selected_sessions.Add(Impostors[pos]);
                var numeric_attribute_values = EvaluateUserSession(false, true, Impostors[pos]);
                impostor_numeric_attribute_values.Add(numeric_attribute_values);
                if (impostor_numeric_attribute_values.Count > MAX_METAMODEL_SESSIONS)
                    impostor_numeric_attribute_values.RemoveAt(0);

                ImpostorTraining.Add(new KeyValuePair<Sample, Dictionary<string, double>>(Impostors[pos], numeric_attribute_values));

                Classifier.AppendTrainingSession(false, numeric_attribute_values);
            }

            return selected_sessions;
        }

        public void EvaluateFixedImpostors(Sample[] impostor_sessions)
        {
            foreach (var session in impostor_sessions)
            {
                var numeric_attribute_values = EvaluateUserSession(false, true, session);
                impostor_numeric_attribute_values.Add(numeric_attribute_values);
                if (impostor_numeric_attribute_values.Count > MAX_METAMODEL_SESSIONS)
                    impostor_numeric_attribute_values.RemoveAt(0);

                ImpostorTraining.Add(new KeyValuePair<Sample, Dictionary<string, double>>(session, numeric_attribute_values));

                Classifier.AppendTrainingSession(false, numeric_attribute_values);
            }
        }

        Dictionary<string,double> EvaluateUserSession(bool legitimate, bool add_session_vector, Sample session)
        {
            Dictionary<string, KeyValuePair<TypingFeature, Model[]>> pattern_vectors = null;
            var numeric_attribute_values = CalculateAttributes(session, out pattern_vectors);
            if (add_session_vector)
                AddSessionVector(session, legitimate, numeric_attribute_values, pattern_vectors);

            return numeric_attribute_values;
        }

        Authentication AddSessionVector
            (
                Sample session, 
                bool legitimate, 
                Dictionary<string,double> numeric_attribute_values,
                Dictionary<string, KeyValuePair<TypingFeature, Model[]>> pattern_vectors
            )
        {
            if (legitimate == true)
            {
                SessionsFed++;
                SessionsFedWithoutModelRetraining++;
            }

            Authentication authentication = new Authentication(session, legitimate, 
                numeric_attribute_values, pattern_vectors);

            return authentication;
        }

        public Dictionary<string, double> CalculateAttributes
            (
                Sample session,
                out Dictionary<string, KeyValuePair<TypingFeature, Model[]>> pattern_vectors
            )
        {
            Dictionary<string, object> features_dictionary = new Dictionary<string, object>();
            pattern_vectors = RunForTypingFeature(session, features_dictionary);

            Dictionary<string, double> numeric_attribute_values = new Dictionary<string, double>();
            foreach (var attr in Parameters.NumericAttributes)
            {
                double tmp = attr.Value.GetValue(features_dictionary, numeric_attribute_values);
                numeric_attribute_values.Add(attr.Key, tmp);
            }

            return numeric_attribute_values;
        }

        public static bool ExportPatternVectors { get; internal set; }

        Dictionary<string, KeyValuePair<TypingFeature, Model[]>> RunForTypingFeature
        (
            Sample session,
            Dictionary<string, object> features_dictionary
        )
        {
            var patterns = Builders.Rebuild(session);
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

            if (ExportPatternVectors)
                return patterns;
            else
                return null;
        }

        public Dictionary<string, ErrorMetrics> EERs { get; private set; }

        public void RetrainClassifier()
        {
            Classifier.Retrain();
            SessionsFedWithoutModelRetraining = 0;
        }

        public int SessionsFed { get; private set; }
        public int SessionsFedWithoutModelRetraining { get; private set; }
        public Authentication Authenticate(Sample session, RetrainBehavior retrain = RetrainBehavior.TrainOnLegitimate)
        {
            Dictionary<string, KeyValuePair<TypingFeature, Model[]>> pattern_vectors = null;
            var numeric_attribute_values = CalculateAttributes(session, out pattern_vectors);

            bool legitimate = true;
            if (Classifier.CanAuthenticate)
                legitimate = Classifier.IsLegitimate(numeric_attribute_values);

            Authentication authentication = null;
            if (!Classifier.CanAuthenticate || 
                retrain == RetrainBehavior.AlwaysTrain || 
                (retrain == RetrainBehavior.TrainOnLegitimate && legitimate))
            {
                legitimate_numeric_attribute_values.Add(numeric_attribute_values);
                if (legitimate_numeric_attribute_values.Count > 100)
                    legitimate_numeric_attribute_values.RemoveAt(0);

                LegitimateTraining.Add(new KeyValuePair<Sample, Dictionary<string, double>>(session, numeric_attribute_values));

                authentication = AddSessionVector(session, true, numeric_attribute_values, pattern_vectors);
                Classifier.AppendTrainingSession(true, numeric_attribute_values);
                Feeder.Feed(session, false);

                EvaluateImpostors(1);
            }
            else
                authentication = new Authentication(session, legitimate, numeric_attribute_values, pattern_vectors);

            return authentication;
        }

        public Authentication[] AuthenticateWithoutRetrain(Sample[] sessions)
        {
            List<Dictionary<string, double>> values = new List<Dictionary<string, double>>();
            List<Dictionary<string, KeyValuePair<TypingFeature, Model[]>>> pattern_vectors_values = new List<Dictionary<string, KeyValuePair<TypingFeature, Model[]>>>();
            foreach (var session in sessions)
            {
                Dictionary<string, KeyValuePair<TypingFeature, Model[]>> pattern_vectors = null;
                var numeric_attribute_values = CalculateAttributes(session, out pattern_vectors);
                values.Add(numeric_attribute_values);
                pattern_vectors_values.Add(pattern_vectors);
            }

            var values_arr = values.ToArray();
            var pattern_vectors_arr = pattern_vectors_values.ToArray();

            Authentication[] retval = new Authentication[sessions.Length];
            bool[] legitimate = new bool[sessions.Length];
            if (Classifier.CanAuthenticate)
                legitimate = Classifier.AreLegitimate(values_arr);
            
            for (int i = 0; i < sessions.Length; i++)
            {
                var numeric_attribute_values = values_arr[i];
                retval[i] = new Authentication(sessions[i], legitimate[i], numeric_attribute_values, pattern_vectors_arr[i]);
            }

            return retval;
        }

        public static Dictionary<string, ErrorMetrics> CalculateEERs(Authentication[] legitimate, Authentication[] impostor)
        {
            Dictionary<string, ErrorMetrics> retval = new Dictionary<string, ErrorMetrics>();
            foreach (var kv in legitimate[0].MethodValues)
            {
                double[] vl = legitimate.Select(t => t.MethodValues[kv.Key]).ToArray();
                double[] vi = impostor.Select(t => t.MethodValues[kv.Key]).ToArray();
                retval.Add(kv.Key, new ErrorMetrics(kv.Key, vl, vi));
            }

            return retval;
        }

        public string SummarizeEERs()
        {
            StringBuilder retval = new StringBuilder();
            foreach (var kv in EERs)
            {
                if (retval.Length != 0) retval.Append(", ");
                retval.Append(kv.Key);
                retval.Append("=");
                retval.Append(Math.Round(100.0 * kv.Value.EER, 2));
                retval.Append("% at ");
                retval.Append(Math.Round(kv.Value.Threshold, 2));
            }

            return retval.ToString();
        }

        public double GetP(string method, double value)
        {
            int lc = legitimate_numeric_attribute_values.Count -
                     legitimate_numeric_attribute_values.Where(s => s[method] <= value).Count();

            int ic = impostor_numeric_attribute_values.Where(s => s[method] <= value).Count();

            return (double)lc / (double)(lc + ic + 1);
        }

        public void CalculateEERs()
        {
            foreach (var kv in Parameters.NumericAttributes)
            {
                double[] legitimate = legitimate_numeric_attribute_values.Select(t => t[kv.Key]).ToArray();
                double[] impostor = impostor_numeric_attribute_values.Select(t => t[kv.Key]).ToArray();

                ErrorMetrics eer = new ErrorMetrics(kv.Key, legitimate, impostor);
                if (EERs.ContainsKey(kv.Key))
                    EERs[kv.Key] = eer;
                else
                    EERs.Add(kv.Key, eer);
            }

            Classifier.AddEERs(EERs);
        }

        public List<double> GetAverageDifferenceDistribution(TypingFeature feature)
        {
            Builder builder = null;
            var candidates = Builders.Where(b =>
                b.Storage.Feature == feature &&
                typeof(AvgStdevModel).IsAssignableFrom(b.Storage.Factory.DefaultModelType));

            if (candidates.Count() > 1)
                throw new Exception("More than one builder for models of type " + typeof(AvgStdevModel).Name + " available.");
            else if (candidates.Count() == 1)
                builder = candidates.First();
            else
                throw new Exception("No builders for models of type " + typeof(AvgStdevModel).Name + " available.");

            List<double> avg = new List<double>();
            foreach (var session in TrainingSessions)
            {
                double avg_r = session.GetAlphanumericAverage(feature);

                int avg_s_count = 0;
                double avg_s = 0.0;
                Model[] models = builder.Rebuild(session);
                for (int i = 0; i < models.Length; i++)
                {
                    AvgStdevModel avgstdev = models[i] as AvgStdevModel;
                    if (avgstdev != null)
                    {
                        avg_s_count++;
                        avg_s += avgstdev.Average;
                    }
                }

                avg_s /= avg_s_count;
                avg.Add(avg_s - avg_r);
            }

            return avg;
        }

        public User[] Users
        {
            get
            {
                return TrainingSessions.Select(s => s.User).Distinct().ToArray();
            }
        }

        public bool IsSingleUserProfile
        {
            get
            {
                return Users.Length == 1;
            }
        }

        public double AverageTrainingLength
        {
            get
            {
                return Math.Round(TrainingSessions.Select(s => s.Length).Average(),2);
            }
        }

        /*
        static void FindEER(double[] legitimate, double[] impostor, out double eer, out double eer_value)
        {
            Array.Sort(legitimate);
            Array.Sort(impostor);

            int ipos = 0;
            for (int i = 0; i < legitimate.Length; i++)
            {
                while (ipos < impostor.Length && impostor[ipos] <= legitimate[i])
                    ipos++;

                double frr = (double)(legitimate.Length - i - 1) / (double)legitimate.Length;
                double far = (double)ipos / (double)impostor.Length;

                if (far >= frr)
                {
                    eer = (far + frr) / 2.0;

                    if (ipos >= impostor.Length) ipos = impostor.Length - 1;
                    eer_value = (legitimate[i] + impostor[ipos]) / 2.0;
                    return;
                }
            }

            throw new ArgumentException("Unable to calculate EER");
        }
        */
    }
}
