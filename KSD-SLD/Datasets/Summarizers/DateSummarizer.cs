using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;


namespace KSDSLD.Datasets.Summarizers
{
    class DateSummarizer : IDatasetSummarizer
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        const int BUCKETS = 20;

        public void Summarize(Dataset dataset)
        {
            log.Info("Date buckets...");

            DateTime min_date = dataset.Samples.Min(s => s.Timestamp);
            DateTime max_date = dataset.Samples.Max(s => s.Timestamp);
            double interval = (max_date - min_date).TotalSeconds / BUCKETS;
            max_date = min_date.AddSeconds(interval);

            for (int i = 0; i < BUCKETS; i++)
            {
                int count = dataset.Samples.Where(s => s.Timestamp >= min_date && s.Timestamp < max_date).Count();
                log.Info("  < {0,10} {1,6} {2,6:0.00}%", max_date.ToString("dd/MM/yyyy"), count, 
                    Math.Round(100.0 * count / dataset.Samples.Length, 2));

                min_date = min_date.AddSeconds(interval);
                max_date = max_date.AddSeconds(interval);
            }
        }
    }
}
