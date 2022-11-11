using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.IO;
using System.Threading;

using NLog;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.Classifiers;
using KSDSLD.FiniteContexts.Partitions;
using KSDSLD.FiniteContexts.Profiles;
using KSDSLD.Pipelines;


namespace KSDSLD.FiniteContexts.Util
{
    class FiniteContextsHelper
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public FiniteContextsConfiguration Parameters { get; private set; }

        public void InitializeParameters(string finite_contexts_section_name)
        {
            var section = (FiniteContextsExperimentConfigurationSection)ConfigurationManager.GetSection(finite_contexts_section_name);

            Parameters = new FiniteContextsConfiguration
                (
                    section.BiometricParameters,
                    section.Models,
                    section.Features,
                    section.Attributes
                );

            Parameters.Initialize();
        }

        public enum Stage
        {
            InitializeClassifier,
            BuildInitialProfile,
            EvaluateLegitimate,
            EvaluateImpostors,
            RetrainClassifier
        }

        public delegate void OnCreateProfileProgressDelegate(Stage stage);

        public Profile CreateProfile(Sample[] initial_training, bool clear_classifier_cache = true, OnCreateProfileProgressDelegate progress = null)
        {
            User user = initial_training[0].User;
            return CreateProfile(user, initial_training, clear_classifier_cache, progress);
        }

        public Profile CreateProfile(User user, Sample[] initial_training, bool clear_classifier_cache = true, OnCreateProfileProgressDelegate progress = null)
        {
            Sample[] impostors = Pipeline.CurrentDataset.Samples.Where(s => s.User.UserID != user.UserID).ToArray();
            return CreateProfile(user, initial_training, impostors, clear_classifier_cache, progress);
        }

        public Profile CreateProfile(User user, Sample[] initial_training, Sample[] impostors, bool clear_classifier_cache = true, OnCreateProfileProgressDelegate progress = null)
        {
            if (progress != null) progress(Stage.InitializeClassifier);

            Classifier classifier = new ClassifierWEKA(user, Parameters, new DirectoryInfo(Environment.CurrentDirectory));
            classifier.Initialize();
            if (clear_classifier_cache)
                classifier.ClearClassifierCache();

            Profile profile = new Profile(user, Parameters, classifier, impostors);
            if (progress != null) progress(Stage.BuildInitialProfile);
            profile.BuildInitialProfile(initial_training, initial_training.Length);
            if (progress != null) progress(Stage.EvaluateLegitimate);
            profile.EvaluateLegitimate(initial_training, 0, false);
            if (progress != null) progress(Stage.EvaluateImpostors);
            profile.EvaluateImpostors(initial_training.Length);
            if (progress != null) progress(Stage.RetrainClassifier);
            profile.RetrainClassifier();
            return profile;
        }

        public double TrainTestSplit { get; set; } = 0.75;

        public Profile CreateProfile(Sample[] legitimate, Sample[] impostors, bool clear_classifier_cache = true, OnCreateProfileProgressDelegate progress = null)
        {
            User user = legitimate[0].User;
            if (progress != null) progress(Stage.InitializeClassifier);

            ClassifierWEKA classifier = new ClassifierWEKA(user, Parameters, new DirectoryInfo(Environment.CurrentDirectory));
            classifier.Initialize();
            classifier.TrainTestSplit = TrainTestSplit;
            if (clear_classifier_cache)
                classifier.ClearClassifierCache();

            Profile profile = new Profile(user, Parameters, classifier, impostors);
            if (progress != null) progress(Stage.BuildInitialProfile);
            profile.BuildInitialProfile(legitimate, legitimate.Length);
            if (progress != null) progress(Stage.EvaluateLegitimate);
            profile.EvaluateLegitimate(legitimate, 0, false);
            if (progress != null) progress(Stage.EvaluateImpostors);
            profile.EvaluateFixedImpostors(impostors);
            if (progress != null) progress(Stage.RetrainClassifier);
            profile.RetrainClassifier();
            return profile;
        }

        public Profile CreateProfileWithNullClassifier(User user, Sample[] initial_training)
        {
            Classifier classifier = new NullClassifier(user, Parameters, new DirectoryInfo(Environment.CurrentDirectory));
            Profile profile = new Profile(user, Parameters, classifier, Pipeline.CurrentPipeline.CurrentResults.Datasets[0].Samples);
            profile.BuildInitialProfile(initial_training, initial_training.Length);

            /*
            profile.EvaluateLegitimate(initial_training, 0, true);
            profile.EvaluateImpostors(initial_training.Length);
            profile.RetrainClassifier();
            */

            return profile;
        }

        object giant_lock = new object();
        Dictionary<int, Profile> profiles_with_null_classifier = new Dictionary<int, Profile>();
        public Profile GetOrCreateProfileWithNullClassifier(Sample[] initial_training)
        {
            User user = initial_training[0].User;

            Profile retval = null;
            bool found = false;
            while (retval == null)
            {
                found = false;
                lock (giant_lock)
                {
                    if (!profiles_with_null_classifier.ContainsKey(user.UserID))
                        profiles_with_null_classifier.Add(user.UserID, null);
                    else
                    {
                        retval = profiles_with_null_classifier[user.UserID];
                        log.Info("FOUND!!!");
                        found = true;
                    }
                }

                if (retval == null)
                {
                    if (found)
                        Thread.Sleep(1000);
                    else
                    {
                        Profile profile = CreateProfileWithNullClassifier(user, initial_training);
                        profiles_with_null_classifier[user.UserID] = profile;
                    }
                }
            }

            return retval;
        }

        public void SetupSession(Sample session)
        {
            ThresholdPartitioner.ProcessSessionWithDefaultValues(session);
            CleanFTs.ProcessSession(session);
        }
    }
}
