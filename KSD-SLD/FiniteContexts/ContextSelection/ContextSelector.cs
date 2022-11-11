using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.FiniteContexts;
using KSDSLD.FiniteContexts.Models;


namespace KSDSLD.FiniteContexts.ContextSelection
{
    public abstract class ContextSelector
    {
        public const int DEFAULT_MIN_OBSERVATIONS = 10;

        public int MinimumObservations { get; private set; } = DEFAULT_MIN_OBSERVATIONS;


        public const int DEFAULT_MAX_CONTEXT_ORDER = int.MaxValue;
        public int MaximumContextOrder { get; private set; } = DEFAULT_MAX_CONTEXT_ORDER;
        public void SetMaximumContextOrder(int max_context_order)
        {
            MaximumContextOrder = max_context_order;
        }


        public int[] ContextOrderCount { get; private set; } = new int[8];
        public void ResetContextOrderCount()
        {
            for (int i = 0; i < ContextOrderCount.Length; i++)
                ContextOrderCount[i] = 0;
        }

        public abstract Model ChooseModel(Model[] models_found, int max_context_order, int offset);
    }
}
