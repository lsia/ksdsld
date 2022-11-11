using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Compression;

using NLog;


namespace KSDSLD.Datasets
{
    class BinaryDatasetWriter
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public string Filename { get; private set; }
        public BinaryDatasetWriter(string filename)
        {
            Filename = filename;
        }

        public void Write(Dataset dataset)
        {
            log.Info("Serializing session set...");
            FileStream fs = File.Create(Filename);
            GZipStream zip = new GZipStream(fs, CompressionLevel.Optimal, false);

            var users = dataset.Samples.Select(s => s.User).Distinct();

            // Write users
            BinaryWriter writer = new BinaryWriter(zip);
            writer.Write((int) users.Count());
            foreach ( User user in users )
            {
                writer.Write(user.UserID);
                writer.Write((int) user.Gender);
                writer.Write(user.BirthDate.Ticks);
                writer.Write(user.Name);
            }

            // Write sessions
            for (int i = 0; i < dataset.Samples.Length; i++)
            {
                if ((i % 1000) == 0)
                    Console.Write(".");

                dataset.Samples[i].Serialize(zip);
            }

            Console.WriteLine();
            writer.Close();
            zip.Close();
            fs.Close();
            log.Info("  Ready.");
        }
    }
}
