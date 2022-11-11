using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.FiniteContexts;
using KSDSLD.FiniteContexts.Profiles;
using KSDSLD.Pipelines;
using KSDSLD.Util;


namespace KSDSLD.FiniteContexts.Util
{
    class SessionAgreggator : SessionAgreggatorBase
    {
        public SessionAgreggator()
            : base(Pipeline.CurrentPipeline.Configuration.Name, 
                  new string[2] { "legitimate", "impostor" })
        {
        }

        public void UpdateARFFs(Authentication auth, bool legitimate)
        {
            base.UpdateARFFs(auth, legitimate ? "legitimate" : "impostor");
        }
    }
}
