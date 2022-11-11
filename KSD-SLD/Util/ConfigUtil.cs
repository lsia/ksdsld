using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;


namespace KSDSLD.Util
{
    static class ConfigUtil
    {
        public static bool GetBoolSetting(string name, bool default_value)
        {
            string value = ConfigurationManager.AppSettings[name];
            if (value == null || value.Trim() == "")
                return default_value;

            return value.ToUpper() == "TRUE";
        }

        public static int GetIntSetting(string name, int default_value)
        {
            string value = ConfigurationManager.AppSettings[name];
            if (value == null || value.Trim() == "")
                return default_value;

            return int.Parse(value);
        }

        public static string GetStringSetting(string name, string default_value)
        {
            string value = ConfigurationManager.AppSettings[name];
            if (value == null || value.Trim() == "")
                return default_value;

            return value;
        }

        public static bool Contains(string name, string value, char separator = ',')
        {
            string tmp = ConfigurationManager.AppSettings[name];
            if (value == null || value.Trim() == "")
                return false;

            string[] fields = tmp.Split(separator);
            for (int i = 0; i < fields.Length; i++)
                if (fields[i].Trim().ToUpper() == name.Trim().ToUpper())
                    return true;

            return false;
        }

        public static bool ContainsOrAll(string name, string value, char separator = ',')
        {
            string tmp = ConfigurationManager.AppSettings[name];
            if (tmp == null || tmp.Trim() == "")
                return true;

            if (tmp.Trim().ToUpper() == "ALL")
                return true;

            string[] fields = tmp.Split(separator);
            for (int i = 0; i < fields.Length; i++)
            {
                string field = fields[i].Trim().ToUpper();
                if (field == value.Trim().ToUpper() || field == "ALL")
                    return true;
            }

            return false;
        }
    }
}
