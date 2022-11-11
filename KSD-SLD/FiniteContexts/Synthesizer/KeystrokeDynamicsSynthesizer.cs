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
    abstract class KeystrokeDynamicsSynthesizer
    {
        public Profile Profile { get; private set; }
        public KeystrokeDynamicsSynthesizer(Profile profile)
        {
            Profile = profile;
        }

        public virtual void Initialize()
        {
        }

        public abstract int[] SynthesizeFeature(TypingFeature feature, Sample dummy);

        public Sample Synthesize(byte[] vks, int session_id = int.MinValue)
        {
            Sample dummy = Sample.CreateDummy(vks, session_id);

            int[] hts = SynthesizeFeature(TypingFeature.HT, dummy);
            int[] fts = SynthesizeFeature(TypingFeature.FT, dummy);

            Sample retval = new Sample(session_id, null, DateTime.Now, GetType().Name, vks, hts, fts);

            ThresholdPartitioner.ProcessSessionWithDefaultValues(retval);
            CleanFTs.ProcessSession(retval);
            
            return retval;
        }

        public Sample Synthesize(Sample session, User user = null)
        {
            Sample retval = Synthesize(session.VKs, session.ID);
            retval.SetUser(user);
            return retval;
        }
    }
}
