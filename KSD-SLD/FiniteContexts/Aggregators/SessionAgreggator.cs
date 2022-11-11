using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Datasets;


namespace KSDSLD.FiniteContexts.Aggregators
{
    public abstract class SessionAgreggator
    {
        public Sample AggregatedSession { get; protected set; }
        public int SessionCount { get; protected set; }

        protected abstract void DoAddSession(Sample session);

        public void AddSession(Sample session)
        {
            SessionCount++;

            if (AggregatedSession == null)
                AggregatedSession = session;
            else
                DoAddSession(session);
        }

        public void Clear()
        {
            AggregatedSession = null;
            SessionCount = 0;
        }
    }
}
