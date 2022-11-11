using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Datasets;


namespace KSDSLD.Pipelines
{
    public class Results
    {
        public Dataset[] Datasets
        {
            get;
            private set;
        }

        public void ReorderDatasets(string[] new_order)
        {
            List<Dataset> old_order = Datasets.ToList();
            List<Dataset> tmp = new List<Dataset>();

            for (int i = 0; i < new_order.Length; i++)
            {
                Dataset dataset = old_order.Where(d => d.Name == new_order[i]).FirstOrDefault();
                if (dataset != null)
                    tmp.Add(dataset);

                old_order.Remove(dataset);
            }

            tmp.AddRange(old_order);
            Datasets = tmp.ToArray();
        }

        List<Dataset> datasets = new List<Dataset>();
        public void AddDataset(Dataset dataset)
        {
            datasets.Add(dataset);
            Datasets = datasets.ToArray();
        }

        public void KeepFirst()
        {
            Dataset[] tmp = new Dataset[1];
            tmp[0] = Datasets[0];
            Datasets = tmp;
        }

        public void ClearDatasets()
        {
            datasets.Clear();
            Datasets = new Dataset[0];
        }
    }
}
