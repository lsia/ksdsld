using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Datasets;
using KSDSLD.Pipelines;

using NLog;


namespace KSDSLD.Util
{
    static class ExperimentParallelization
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public delegate void ForEachDatasetDelegate(Dataset dataset);
        public static void ForEachDataset(Results results, ForEachDatasetDelegate f)
        {
            IndentLayoutRenderer.Add();

            foreach (Dataset dataset in results.Datasets)
            {
                if (results.Datasets.Length != 1)
                {
                    log.Info("DATASET {0}", dataset.Name);
                    IndentLayoutRenderer.Add();
                }

                f(dataset);

                if (results.Datasets.Length != 1)
                    IndentLayoutRenderer.Remove();
            }

            IndentLayoutRenderer.Remove();
        }

        public delegate void ForEachSessionDelegate(Dataset dataset, Sample session);
        public static void ForEachSession(Results results, ForEachSessionDelegate f)
        {
            IndentLayoutRenderer.Add();

            foreach (Dataset dataset in results.Datasets)
            {
                if (results.Datasets.Length != 1)
                {
                    log.Info("DATASET {0}", dataset.Name);
                    IndentLayoutRenderer.Add();
                }

                if (Parallel)
                {
                    System.Threading.Tasks.Parallel.ForEach(dataset.Samples, session =>
                    {
                        try
                        {
                            f(dataset, session);
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "UNHANDLED EXCEPTION: ");
                        }
                    });
                }
                else
                {
                    foreach(var session in dataset.Samples)
                    {
                        try
                        {
                            f(dataset, session);
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "UNHANDLED EXCEPTION: ");
                        }
                    }
                }

                if (results.Datasets.Length != 1)
                    IndentLayoutRenderer.Remove();
            }

            IndentLayoutRenderer.Remove();
        }

        public static bool Parallel = true;

        public delegate void ForEachUserDelegate(Dataset dataset, int user_id, Sample[] sessions);
        public static bool ForEachUser(Results results, ForEachUserDelegate f)
        {
            IndentLayoutRenderer.Add();

            bool there_were_errors = false;
            foreach (Dataset dataset in results.Datasets)
            {
                if (results.Datasets.Length != 1)
                {
                    log.Info("DATASET {0}", dataset.Name);
                    IndentLayoutRenderer.Add();
                }

                var sessions_per_user = dataset.Samples.GroupBy(s => s.User.UserID);

                if (Parallel)
                {
                    System.Threading.Tasks.Parallel.ForEach(sessions_per_user, (IGrouping<int, Sample> kv) =>
                    {
                        try
                        {
                            f(dataset, kv.Key, kv.ToArray());
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "UNHANDLED EXCEPTION: ");
                            there_were_errors = true;
                            throw;
                        }
                    });
                }
                else
                {
                    foreach (var kv in sessions_per_user)
                        try
                        {
                            f(dataset, kv.Key, kv.ToArray());
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "UNHANDLED EXCEPTION: ");
                            there_were_errors = true;
                            throw;
                        }
                }

                if (results.Datasets.Length != 1)
                    IndentLayoutRenderer.Remove();
            }

            IndentLayoutRenderer.Remove();
            return there_were_errors;
        }
    }
}
