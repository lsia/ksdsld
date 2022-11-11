using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Datasets.Readers
{
    public interface IDatasetReader
    {
        Dataset ReadDataset(string filename, string dataset_name, string dataset_source);
    }
}
