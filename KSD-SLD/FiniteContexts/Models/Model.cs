using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.FiniteContexts.Models
{
    public abstract class Model
    {
        public ulong Context { get; private set; }
        public short ContextOrder { get; protected set; }

        public int NGram { get; private set; }
        public short NGramOrder { get; private set; }

        public int Count { get; protected set; }

        public Model(ulong context, short context_order, int ngram, short ngram_order)
        {
            Context = context;
            ContextOrder = context_order;
            NGram = ngram;
            NGramOrder = ngram_order;

            Count = 0;
        }

        public ulong Hash
        {
            get
            {
                return ((ulong) NGram << 8 * (sizeof(ulong) - NGramOrder)) | Context;
            }
        }

        public abstract void Feed(int[] parameter_values, int pos);

        public virtual byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
