using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;


namespace KSDSLD.Util
{
    public class ARFF
    {
        public string Filename { get; private set; }
        public ARFF(string filename)
        {
            Filename = filename;
        }

        StreamWriter sw;
        public void StartHeaders()
        {
            sw = File.CreateText(Filename + ".arff");
            sw.WriteLine("@RELATION ksd");
        }

        public void AddNumericAttribute(string name)
        {
            sw.WriteLine("@ATTRIBUTE " + name + " NUMERIC");
        }

        public void AddNominalAttribute(string name, params string[] enumeration)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("@ATTRIBUTE ");
            sb.Append(name);
            sb.Append(" {");

            for (int i = 0; i < enumeration.Length; i++)
            {
                if (i != 0) sb.Append(",");
                sb.Append(enumeration[i]);
            }

            sb.Append("}");
            sw.WriteLine(sb.ToString());
        }

        public void StartData()
        {
            if ( !has_category)
                sw.WriteLine("@ATTRIBUTE cls {legitimate, impostor}");

            sw.WriteLine();
            sw.WriteLine("@DATA");
        }

        bool has_category = false;
        public void AddCategory(string name, string enumeration)
        {
            sw.WriteLine("@ATTRIBUTE " + name + " {" + enumeration + "}");
            has_category = true;
        }

        public void CopyTo(string destiny)
        {
            File.Copy(Filename + ".arff", destiny, true);
        }

        public void AddCategory(string name, params string[] enumeration)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("@ATTRIBUTE ");
            sb.Append(name);
            sb.Append(" {");

            for (int i = 0; i < enumeration.Length; i++)
            {
                if (i != 0) sb.Append(",");
                sb.Append(enumeration[i]);
            }

            sb.Append("}");
            sw.WriteLine(sb.ToString());
            has_category = true;
        }

        int pos = 0;
        public void AppendData(double[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (i != 0)
                    sw.Write(",");

                sw.Write(values[i]);
            }

            pos += values.Length;
        }

        public void AppendData(params object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (i != 0)
                    sw.Write(",");

                sw.Write(values[i]);
            }

            pos += values.Length;
        }

        public void AppendCategory(string category)
        {
            InstanceCount++;
            sw.Write(",");
            sw.WriteLine(category);
            pos = 0;
        }

        public void AppendLineVerbatim(string line)
        {
            InstanceCount++;
            sw.WriteLine(line);
            pos = 0;
        }

        public int InstanceCount { get; private set; }

        public void Flush()
        {
            sw.Flush();
        }

        public void Close()
        {
            sw.Close();
        }
    }
}
