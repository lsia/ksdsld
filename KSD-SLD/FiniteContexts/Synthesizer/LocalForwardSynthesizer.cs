using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.Partitions;
using KSDSLD.FiniteContexts.Profiles;
using KSDSLD.FiniteContexts.Models;
using KSDSLD.FiniteContexts.PatternVector;
using KSDSLD.Util;


namespace KSDSLD.FiniteContexts.Synthesizer
{
    abstract class LocalForwardSynthesizer<T> : KeystrokeDynamicsSynthesizer where T : Model
    {
        public LocalForwardSynthesizer(Profile profile)
            : base(profile)
        {
            foreach (var feature in (TypingFeature[]) Enum.GetValues(typeof(TypingFeature)))
            {
                var candidates = Profile.Builders.Where(b => 
                    b.Storage.Feature == feature &&
                    typeof(T).IsAssignableFrom(b.Storage.Factory.DefaultModelType));
                
                if (candidates.Count() > 1)
                    throw new Exception("More than one builder for models of type " + typeof(T).Name + " available.");
                else if (candidates.Count() == 1)
                    Builders.Add(feature, candidates.First());
            }
        }

        public Dictionary<TypingFeature, Builder> Builders { get; private set; } = new Dictionary<TypingFeature, Builder>();

        public abstract int OnNullModel(int pos);
        public abstract int OnModelFound(int pos, T model);

        public int[] ForEachModel(TypingFeature feature, Sample dummy) 
        {
            Model[] pattern_vector = Builders[feature].Rebuild(dummy);

            int[] retval = new int[dummy.Length];
            for (int i = 0; i < dummy.Length; i++)
            {
                T model = (T)pattern_vector[i];
                if (model == null)
                    retval[i] = OnNullModel(i);
                else
                {
                    retval[i] = OnModelFound(i, model);
                    if (retval[i] < 0)
                        retval[i] = 0;
                }
            }

            return retval;
        }

        public TypingFeature CurrentFeature { get; private set; }

        public virtual void OnSynthesizeFeatureStart(TypingFeature feature, Sample dummy) { }
        public override int[] SynthesizeFeature(TypingFeature feature, Sample dummy)
        {
            OnSynthesizeFeatureStart(feature, dummy);
            CurrentFeature = feature;
            return ForEachModel(feature, dummy);
        }
    }
}
