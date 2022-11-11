using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using NLog;

using KSDSLD.Datasets;
using KSDSLD.Experiments;


namespace KSDSLD.Util
{
    public class CsvWriter
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public string Filename { get; private set; }
        public int Columns { get; private set; }

        public int Rows { get; private set; }
        public CsvWriter(string filename, int columns)
        {
            Filename = filename;
            Columns = columns;
            // Rows = new List<object[]>();

            lock (giant_lock)
                writers.Add(this);

            Filename = Filename.Replace("/", "\\");
            sw = File.CreateText(Filename);
        }

        public static CsvWriter Create(Dataset dataset, User user, string suffix, int columns = 0)
        {
            string filename = ExperimentUtil.OutputFolder
                + dataset.Name + "-" + user.Name + "-" + suffix + ".csv";

            return new CsvWriter(filename, columns);
        }

        public static CsvWriter Create(string preffix, Dataset dataset, string suffix = null, int columns = 0)
        {
            string filename = ExperimentUtil.OutputFolder + 
                preffix + "-" + dataset.Name + (suffix == null ? "" : ("-" + suffix)) + ".csv";

            return new CsvWriter(filename, columns);
        }

        public static CsvWriter Create(string filename)
        {
            return new CsvWriter(filename, 10);
        }

        StreamWriter sw;
        // public List<object[]> Rows { get; private set; }
        public void WriteLine(params object[] row)
        {
            lock ( giant_lock )
            {
                for (int i = 0; i < row.Length; i++)
                {
                    if (i != 0 || line_started)
                        sw.Write(",");

                    if (row[i].GetType() != typeof(string))
                        sw.Write(row[i]);
                    else
                    {
                        sw.Write("\"");
                        sw.Write(row[i].ToString().Replace("\"", "\"\""));
                        sw.Write("\"");
                    }
                }

                sw.WriteLine();
                line_started = false;
                Rows++;
            }

            is_dirty = true;
        }

        public void WriteLine(double[] row)
        {
            lock (giant_lock)
            {
                for (int i = 0; i < row.Length; i++)
                {
                    if (i != 0 || line_started)
                        sw.Write(",");

                    if (row[i].GetType() != typeof(string))
                        sw.Write(row[i]);
                    else
                    {
                        sw.Write("\"");
                        sw.Write(row[i].ToString().Replace("\"", "\"\""));
                        sw.Write("\"");
                    }
                }

                sw.WriteLine();
                line_started = false;
                Rows++;
            }

            is_dirty = true;
        }

        bool line_started = false;        
        public void Write(params object[] row)
        {
            lock (giant_lock)
            {
                for (int i = 0; i < row.Length; i++)
                {
                    if (line_started)
                        sw.Write(",");

                    if (row[i].GetType() != typeof(string))
                        sw.Write(row[i]);
                    else
                    {
                        sw.Write("\"");
                        sw.Write(row[i].ToString().Replace("\"", "\"\""));
                        sw.Write("\"");
                    }

                    line_started = true;
                }
            }

            is_dirty = true;
        }

        public void Flush()
        {
            sw.Flush();
        }

        bool is_dirty = false;
        public void Dump()
        {
            if (Rows == 0)
            {
                sw.Close();
                File.Delete(Filename);
            }
            else
            {
                log.Info("Dumping file {0}...", Filename);
                sw.Close();
            }

            is_dirty = false;
        }

        static object giant_lock = new object();
        static List<CsvWriter> writers = new List<CsvWriter>();
        public static void DumpAll()
        {
            foreach (var w in writers)
                if ( w.is_dirty)
                    w.Dump();
        }

        public void WriteSession(Sample session, bool legitimate)
        {
            for (int i = 0; i < session.Length; i++)
                Write(session.Features[TypingFeature.HT][i]);

            for (int i = 1; i < session.Length; i++)
                Write(session.Features[TypingFeature.FT][i]);

            if (legitimate)
                WriteLine("legitimate");
            else
                WriteLine("impostor");
        }

        public void WriteSession(string header, Sample session)
        {
            Write(header);

            for (int i = 0; i < session.Length; i++)
                Write(session.Features[TypingFeature.HT][i]);

            for (int i = 1; i < session.Length; i++)
                Write(session.Features[TypingFeature.FT][i]);

            WriteLine();
        }
    }
}
