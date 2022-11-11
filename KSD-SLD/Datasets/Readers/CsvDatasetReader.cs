using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using NLog;


namespace KSDSLD.Datasets.Readers
{
    public class CsvDatasetReader : IDatasetReader
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public Dataset ReadDataset(string filename, string dataset_name, string dataset_source)
        {
            log.Info("Reading single user CSV dataset at {0}...", dataset_name);
            User user = User.GetUser(1, "DEFAULT", DateTime.MinValue, Gender.Unknown);

            int id = 0;
            List<Sample> sessions = new List<Sample>();
            foreach (var session_file in Directory.GetFiles(filename))
            {
                log.Info("    {0}", session_file);

                id++;
                List<byte> vks = new List<byte>();
                List<int> hts = new List<int>();
                List<int> fts = new List<int>();

                string[] lines = File.ReadAllLines(session_file);
                for (int i = 1; i < lines.Length; i++)
                    if (lines[i].Trim() != "")
                    {
                        string[] fields = lines[i].Trim().Split(',');

                        try
                        {
                            int vk = int.Parse(fields[0]);
                            int ht = int.Parse(fields[1]);
                            int ft = int.Parse(fields[2]);

                            vks.Add((byte)vk);
                            hts.Add(ht);
                            fts.Add(ft);
                        }
                        catch
                        {
                            // int k = 9;
                        }
                    }

                Sample session = new Sample(id, user, DateTime.MinValue, "N/A", vks.ToArray(), hts.ToArray(), fts.ToArray());
                session.Filename = session_file;
                sessions.Add(session);
            }

            return new Dataset(dataset_name, dataset_source, filename, sessions.ToArray());
        }
    }
}
