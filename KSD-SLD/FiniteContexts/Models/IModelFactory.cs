using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.FiniteContexts.Models
{
    public interface IModelFactory<T>
    {
        Type DefaultModelType { get; }
        T CreateModel(ulong context, short context_order, int ngram, short ngram_order);
    }
}
