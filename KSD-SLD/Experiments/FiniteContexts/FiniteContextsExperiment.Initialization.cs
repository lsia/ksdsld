using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.IO;
using System.Reflection;

using NLog;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.Experiments.Distances;
using KSDSLD.FiniteContexts.Attributes;
using KSDSLD.FiniteContexts.Features;
using KSDSLD.FiniteContexts.Models;
using KSDSLD.FiniteContexts.ModelStorages;
using KSDSLD.FiniteContexts.PatternVector;
using KSDSLD.Pipelines;
using KSDSLD.Util;


namespace KSDSLD.Experiments.FiniteContexts
{
//    /*
//    partial class FiniteContextsExperiment
//    {
//        static Logger log = LogManager.GetCurrentClassLogger();

//        public FiniteContextsExperiment(Pipeline pipeline, PipelineStageConfigurationElement configuration)
//            : base(pipeline, configuration)
//        {
//        }

//        FiniteContextsExperimentConfigurationSection section;

//        int TRAINING_SESSIONS = 50;
//        public override void Initialize()
//        {
//            section = (FiniteContextsExperimentConfigurationSection)ConfigurationManager.GetSection(Configuration.Configuration);

//            TRAINING_SESSIONS = section.InitialTrainingSessions;

//            /*
//            InitializeModels();
//            InitializeFeatures();
//            InitializeAttributes();
//            */
//}

//        /*
//        string[] names;
//        string[] biometric_parameters;
//        StandardModelFactory[] factories;
//        int[] max_context_size;
//        int[] max_ngram_size;

//        void InitializeModels()
//        {
//            log.Info("Initialing model storage...");
//            List<string> names = new List<string>();
//            List<string> biometric_parameters = new List<string>();
//            List<StandardModelFactory> factories = new List<StandardModelFactory>();
//            List<int> max_context_size = new List<int>();
//            List<int> max_ngram_size = new List<int>();

//            ModelFeederConfigurationSection section = ModelFeederConfigurationSection.GetSection(this.section.Models);
//            foreach (ModelSetConfigurationElement element in section.ModelSets)
//            {
//                log.Info("  {0}", element.Name);
//                StandardModelFactory factory = new StandardModelFactory(element.Type);
//                if (!factory.Initialize())
//                    throw new ArgumentException("Model '" + element.Type + "' does not exist.");

//                names.Add(element.Name);
//                biometric_parameters.Add(element.Parameter);
//                factories.Add(factory);
//                max_context_size.Add(element.MaxContextSize);
//                max_ngram_size.Add(element.MaxNGramSize);
//            }

//            this.names = names.ToArray();
//            this.biometric_parameters = biometric_parameters.ToArray();
//            this.factories = factories.ToArray();
//            this.max_context_size = max_context_size.ToArray();
//            this.max_ngram_size = max_ngram_size.ToArray();
//        }

//        ModelFeeder CreateFeeder(User user)
//        {
//            if (user.Properties.ContainsKey("feeder"))
//                return (ModelFeeder) user.Properties["feeder"];

//            BiometricParametersConfigurationSection bpcs = BiometricParametersConfigurationSection.GetSection(section.BiometricParameters);
//            List<TypingFeature> all = new List<TypingFeature>();
//            foreach (BiometricParameterConfigurationElement bpce in bpcs.Parameters)
//                all.Add((TypingFeature) Enum.Parse(typeof(TypingFeature), bpce.Name));

//            List<ModelStorage> mss = new List<ModelStorage>();
//            for (int i = 0; i < factories.Length; i++)
//            {
//                List<TypingFeature> bps = new List<TypingFeature>();
//                if (biometric_parameters[i] == "HT")
//                    bps.Add(TypingFeature.HT);
//                else if (biometric_parameters[i] == "FT")
//                    bps.Add(TypingFeature.FT);
//                else if (biometric_parameters[i] == "ALL")
//                {
//                    bps.Add(TypingFeature.HT);
//                    bps.Add(TypingFeature.FT);
//                }

//                foreach (TypingFeature feature in bps)
//                {
//                    /*
//                    if (i == 0)
//                    {
//                        // ModelStorage tmp = new SqlServerStorage(user, names[i], feature, max_context_size[i], max_ngram_size[i], factories[i]);
//                        RedisStorage tmp = new RedisStorage(user, names[i], feature, max_context_size[i], max_ngram_size[i], factories[i]);
//                        tmp.Initialize();
//                        mss.Add(tmp);
//                    }
//                    else * /
//                    {
//                        ModelStorage tmp = new MemoryStorage(user, names[i], feature, max_context_size[i], max_ngram_size[i], factories[i]);
//                        mss.Add(tmp);
//                    }
//                }
//            }

//            ModelFeeder retval = new ModelFeeder(max_context_size[0], mss.ToArray());
//            user.Properties.Add("feeder", retval);
//            return retval;
//        }
//        */

//        /*
//        Feature[] features;
//        void InitializeFeatures()
//        {
//            log.Info("Initialing features...");
//            List<Feature> features = new List<Feature>();
//            Type[] types = Assembly.GetExecutingAssembly().GetTypes();

//            FeaturesConfigurationSection section = FeaturesConfigurationSection.GetSection(this.section.Features);
//            foreach (FeatureConfigurationElement feature in section.Features)
//            {
//                log.Info("  {0}", feature.Name);
//                Type type = types.Where(t => t.Name == feature.Type).First();
//                if (type == null)
//                    throw new ArgumentException("The type '" + feature.Type + "' does not exist.");

//                if (type.BaseType != typeof(Feature))
//                    throw new ArgumentException("The type '" + feature.Type + "' is not a Feature.");

//                Feature tmp = (Feature)Activator.CreateInstance(type, feature);
//                features.Add(tmp);
//            }

//            this.features = features.ToArray();
//        }
//        */

//            /*
//        public Dictionary<string, INumericAttribute> NumericAttributes { get; private set; }
//        void InitializeAttributes()
//        {
//            log.Info("Initialing attributes...");
//            NumericAttributes = new Dictionary<string, INumericAttribute>();
//            Type[] types = Assembly.GetExecutingAssembly().GetTypes();

//            AttributesConfigurationSection section = AttributesConfigurationSection.GetSection(this.section.Attributes);
//            foreach (AttributeConfigurationElement attr in section.Attributes)
//            {
//                log.Info("  {0}", attr.Name);
//                Type type = types.Where(t => t.Name == attr.Type).First();
//                if (type == null)
//                    throw new ArgumentException("The type '" + attr.Type + "' does not exist.");

//                if (type.GetInterface(typeof(INumericAttribute).FullName) == null)
//                    throw new ArgumentException("The type '" + attr.Type + "' is not an attribute.");

//                INumericAttribute tmp = (INumericAttribute)Activator.CreateInstance(type, attr);
//                NumericAttributes.Add(attr.Name, tmp);
//            }
//        }
//    }
//        */
}
