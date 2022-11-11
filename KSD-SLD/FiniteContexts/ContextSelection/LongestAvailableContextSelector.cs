using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.FiniteContexts.Models;


namespace KSDSLD.FiniteContexts.ContextSelection
{
    class LongestAvailableContextSelector : ContextSelector
    {
        public override Model ChooseModel(Model[] models_found, int storage_context_order, int offset)
        {
            int max_context_order = Math.Min(MaximumContextOrder, storage_context_order);

            for (int i = offset + max_context_order; i >= offset; i--)
                if (models_found[i] != null && models_found[i].Count >= MinimumObservations)
                {
                    ContextOrderCount[i - offset]++;
                    return models_found[i];
                }

            return null;
        }

    }
}
