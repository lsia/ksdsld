using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.FiniteContexts.Models;


namespace KSDSLD.FiniteContexts.Store
{
    interface IProfileStore
    {
        void StoreProfile(int user_id, ModelFeeder feeders);
    }
}
