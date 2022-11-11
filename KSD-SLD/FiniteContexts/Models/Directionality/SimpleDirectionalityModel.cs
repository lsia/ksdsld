using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.FiniteContexts.Models.Directionality
{
    abstract class SimpleDirectionalityModel : Model
    {
        public SimpleDirectionalityModel(ulong context, short context_order, int ngram, short ngram_order)
            : base(context, context_order, ngram, ngram_order)              
        {
            if (ngram_order > 1)
                throw new ArgumentException("ngram_order > 1");
        }

        public double Directionality { get; protected set; }

        public override string ToString()
        {
            return "DI " + Math.Round(Directionality,2).ToString() + "/" + Count;
        }
    }
}
