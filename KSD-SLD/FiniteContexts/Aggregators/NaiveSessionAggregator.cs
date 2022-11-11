using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Datasets;


namespace KSDSLD.FiniteContexts.Aggregators
{
    public class NaiveSessionAggregator : SessionAgreggator
    {
        protected override void DoAddSession(Sample session)
        {
            byte[] vks = new byte[AggregatedSession.VKs.Length + session.VKs.Length];
            int[] hts = new int[AggregatedSession.VKs.Length + session.VKs.Length];
            int[] fts = new int[AggregatedSession.VKs.Length + session.VKs.Length];

            Array.Copy(AggregatedSession.VKs, vks, AggregatedSession.VKs.Length);
            Array.Copy(AggregatedSession.Features[TypingFeature.HT], hts, AggregatedSession.VKs.Length);
            Array.Copy(AggregatedSession.Features[TypingFeature.FT], fts, AggregatedSession.VKs.Length);

            Array.Copy(session.VKs, 0, vks, AggregatedSession.VKs.Length, session.VKs.Length);
            Array.Copy(session.Features[TypingFeature.HT], 0, hts, AggregatedSession.VKs.Length, session.VKs.Length);
            Array.Copy(session.Features[TypingFeature.FT], 0, fts, AggregatedSession.VKs.Length, session.VKs.Length);

            fts[AggregatedSession.VKs.Length] = int.MinValue;
            AggregatedSession = new Sample(session.ID, session.User, session.Timestamp, session.UserAgent, vks, hts, fts);
        }
    }
}
