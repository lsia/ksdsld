using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Datasets.Summarizers
{
    interface IDatasetSummarizer
    {
        void Summarize(Dataset sessions);
    }
}
