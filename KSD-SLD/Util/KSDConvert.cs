using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Util
{
    static class KSDConvert
    {
        public static double[] IntArrayToDouble(int[] values)
        {
            double[] retval = new double[values.Length];
            for (int i = 0; i < retval.Length; i++)
                retval[i] = values[i];

            return retval;
        }

        public static double[] Invert(double[] values)
        {
            double[] retval = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
                retval[i] = 1.0 / values[i];

            return retval;
        }

        public static string TranslateVK(int vk, out string color)
        {
            color = null;

            if ((vk >= 0x41 && vk <= 0x5A) ||
                (vk >= 0x30 && vk <= 0x39))
            {
                return ((char)vk).ToString();
            }
            else if (vk >= 0x60 && vk <= 0x69)
            {
                color = "brown";
                return ((char)(vk - 0x30)).ToString();
            }
            else if (vk == 0x20)
                return " "; // @"{\Spacebar}";
            else if (vk == 0x0D)
                return @"{\Return}";
            else if (vk == 0x08)
                return @"{\BSpace}";
            else if (vk == 0x09)
                return @"{\Tab}";
            else if (vk == 0x1B)
                return @"{\Esc}";

            else if (vk == 0x10)
                return @"{\Shift}";
            else if (vk == 0xA0)
                return @"{\textcolor{cyan}{{\Shift}}}";
            else if (vk == 0xA1)
                return @"{\textcolor{magenta}{{\Shift}}}";

            else if (vk == 0x11)
                return @"{\Ctrl}";
            else if (vk == 0xA2)
                return @"{\textcolor{cyan}{{\Ctrl}}}";
            else if (vk == 0xA3)
                return @"{\textcolor{magenta}{{\Ctrl}}}";

            else if (vk == 0x12)
                return @"{\Ctrl}";
            else if (vk == 0xA4)
                return @"{\textcolor{cyan}{{\Alt}}}";
            else if (vk == 0xA5)
                return @"{\textcolor{magenta}{{\Alt}}}";

            else if (vk == 0x21)
                return @"{\PgUp}";
            else if (vk == 0x22)
                return @"{\PgDown}";
            else if (vk == 0x23)
                return @"{\End}";
            else if (vk == 0x24)
                return @"{\Home}";

            else if (vk == 0x25)
                return @"{\LArrow}";
            else if (vk == 0x26)
                return @"{\UArrow}";
            else if (vk == 0x27)
                return @"{\RArrow}";
            else if (vk == 0x28)
                return @"{\DArrow}";

            else if (vk == 0x2D)
                return @"{\Ins}";
            else if (vk == 0x2E)
                return @"{\Del}";

            else if (vk >= 0x70 && vk <= 0x87)
                return @"{\keystroke{F" + (vk - 0x6F).ToString() + "}}";

            else if (vk == 0xBA)
                return ";";
            else if (vk == 0xBB)
                return "+";
            else if (vk == 0xBC)
                return ",";
            else if (vk == 0xBD)
                return "-";
            else if (vk == 0xBE)
                return ".";
            else if (vk == 0xBF)
                return "/";
            else if (vk == 0xC0)
                return "~";
            else if (vk == 0xDB)
                return "[";
            else if (vk == 0xDC)
                return "\\";
            else if (vk == 0xDD)
                return "]";
            else if (vk == 0xDE)
                return "\"";

            else if (vk == 0x6A)
            {
                color = "brown";
                return "*";
            }
            else if (vk == 0x6B)
            {
                color = "brown";
                return "+";
            }
            else if (vk == 0x6C)
            {
                color = "brown";
                return ".";
            }
            else if (vk == 0x6D)
            {
                color = "brown";
                return "-";
            }
            else if (vk == 0x6F)
            {
                color = "brown";
                return "/";
            }

            else
            {
                color = "blue";
                return ".";
            }
        }

        public static string GetLatexFromVK(int vk)
        {
            string color = null;
            string translated = TranslateVK(vk, out color);
            if (color == null)
                return translated;
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(@"{\textcolor{");
                sb.Append(color);
                sb.Append("}{");
                sb.Append(translated);
                sb.Append("}}");

                return sb.ToString();
            }
        }
    }
}
