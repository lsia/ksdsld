using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using KSDSLD.Util;


namespace KSDSLD.Datasets
{
    public class Sample : IComparable<Sample>
    {
        public int ID { get; private set; }
        public DateTime Timestamp { get; private set; }
        public User User { get; private set; }

        public void SetUser(User user)
        {
            User = user;
        }

        public string UserAgent { get; private set; }
        public string Filename { get; set; }

        public byte[] VKs { get; private set; }

        public Dictionary<string, object> Properties { get; private set; }

        public bool HasAbnormalValues
        {
            get
            {
                for (int i = 0; i < Length; i++)
                    if (Features[TypingFeature.HT][i] < 0 || Features[TypingFeature.HT][i] > 3000)
                        return true;

                for (int i = 1; i < Length; i++)
                    if (Features[TypingFeature.FT][i] < 0 || Features[TypingFeature.FT][i] > 3000)
                        return true;

                return false;
            }
        }


        public static Sample CreateDummy(byte[] vks, int session_id = int.MinValue)
        {
            Sample retval = new Sample(vks, null, null);
            retval.ID = session_id;
            retval.PartitionOffsets = new int[0];
            return retval;
        }

        public int Length
        {
            get
            {
                return VKs.Length;
            }
        }

        public int[] PartitionOffsets { get; private set; }
        public void SetPartitionOffsets(int[] offsets)
        {
            PartitionOffsets = offsets;
        }

        public Sample Split(int start)
        {
            return Split(start, VKs.Length - 1);
        }
        public Sample Split(int start, int end)
        {
            if (start < 0 || start >= VKs.Length || end < 0 || end >= VKs.Length || end < start)
                throw new ArgumentException("Invalid start/end values.");

            int len = end - start + 1;
            byte[] vks = new byte[len];
            Array.Copy(VKs, start, vks, 0, len);

            int[] hts = new int[len];
            Array.Copy(Features[TypingFeature.HT], start, hts, 0, len);

            int[] fts = new int[len];
            Array.Copy(Features[TypingFeature.FT], start, fts, 0, len);

            Sample retval = new Sample(ID, User, Timestamp, UserAgent, vks, hts, fts);
            if (PartitionOffsets != null)
                retval.PartitionOffsets = PartitionOffsets.Where(n => n >= start && n <= end).ToArray();
            
            return retval;
        }

        public Sample ReplaceVKs(byte[] vks)
        {
            if (vks.Length != VKs.Length)
                throw new ArgumentException("Cannot replace VKs with different length array.");

            Sample retval = new Sample(ID, User, Timestamp, UserAgent, vks,
                (int[])Features[TypingFeature.HT].Clone(), (int[])Features[TypingFeature.FT].Clone());

            retval.PartitionOffsets = PartitionOffsets;
            retval.Properties = Properties;
            return retval;
        }

        public Sample ReplaceVKs(Sample session)
        {
            return ReplaceVKs(session.VKs);
        }

        public Sample(byte[] vks, int[] hts, int[] fts)
            : this(int.MinValue, User.GetUser(int.MinValue), DateTime.Now, "", vks, hts, fts)
        {
        }

        public Sample(int id, User user, DateTime date, string user_agent, byte[] vks, int[] hts, int[] fts)
        {
            ID = id;
            User = user;
            Timestamp = date;
            UserAgent = user_agent;

            VKs = vks;

            Properties = new Dictionary<string, object>();

            Features = new Dictionary<TypingFeature, int[]>();
            Features.Add(TypingFeature.HT, hts);
            Features.Add(TypingFeature.FT, fts);
        }

        public Sample ToImpostor()
        {
            User impostor = User.GetUser(-666, "IMPOSTOR", DateTime.MinValue, Gender.Unknown);
            return new Sample(ID, impostor, Timestamp, UserAgent, VKs, Features[TypingFeature.HT], Features[TypingFeature.FT]);
        }

        public Dictionary<TypingFeature, int[]> Features { get; private set; }

        public static string GetVKString(byte vk, bool expand_vks = false)
        {            
            StringBuilder sb_text = new StringBuilder();

            if (vk >= 0x41 && vk <= 0x5A)
                sb_text.Append((char)vk);
            else if (vk >= 0x30 && vk <= 0x39)
                sb_text.Append((char)vk);
            else if (vk >= 0x60 && vk <= 0x69)
                sb_text.Append((char)(vk - 0x30));

            else if (vk == (byte)VirtualKeys.VK_SPACE)
            {
                sb_text.Append(" ");
                /*
                if (!expand_vks)
                    sb_text.Append(" ");
                else
                    sb_text.Append("[SPACE]");
                */
            }
            else if (vk == (byte)VirtualKeys.VK_BACK)
                sb_text.Append("[BACK]");
            else if (vk == (byte)VirtualKeys.VK_RETURN)
                sb_text.Append("[ENTER]");

            else if (vk == (byte)VirtualKeys.VK_SHIFT)
                sb_text.Append("[SHIFT]");
            else if (vk == (byte)VirtualKeys.VK_LSHIFT)
                sb_text.Append("[LSHIFT]");
            else if (vk == (byte)VirtualKeys.VK_RSHIFT)
                sb_text.Append("[RSHIFT]");
            else if (vk == (byte)VirtualKeys.VK_RCONTROL)
                sb_text.Append("[RCONTROL]");
            else if (vk == (byte)VirtualKeys.VK_LCONTROL)
                sb_text.Append("[LCONTROL]");
            else if (vk == (byte)VirtualKeys.VK_LMENU)
                sb_text.Append("[LMENU]");
            else if (vk == (byte)VirtualKeys.VK_RMENU)
                sb_text.Append("[RMENU]");

            else if (vk == (byte)VirtualKeys.VK_HOME)
                sb_text.Append("[HOME]");
            else if (vk == (byte)VirtualKeys.VK_END)
                sb_text.Append("[END]");
            else if (vk == (byte)VirtualKeys.VK_PRIOR)
                sb_text.Append("[PGUP]");
            else if (vk == (byte)VirtualKeys.VK_NEXT)
                sb_text.Append("[PGDN]");

            else if (vk == (byte)VirtualKeys.VK_LEFT)
                sb_text.Append("[L]");
            else if (vk == (byte)VirtualKeys.VK_RIGHT)
                sb_text.Append("[R]");
            else if (vk == (byte)VirtualKeys.VK_UP)
                sb_text.Append("[U]");
            else if (vk == (byte)VirtualKeys.VK_DOWN)
                sb_text.Append("[D]");

            else if (vk == 0xBA)
                sb_text.Append(";");
            else if (vk == 0xBB)
                sb_text.Append("+");
            else if (vk == 0xBC)
                sb_text.Append(",");
            else if (vk == 0xBD)
                sb_text.Append("-");
            else if (vk == 0xBE)
                sb_text.Append(".");
            else if (vk == 0xBF)
                sb_text.Append("/");
            else if (vk == 0xC0)
                sb_text.Append("~");
            else if (vk == 0xDB)
                sb_text.Append("[");
            else if (vk == 0xDC)
                sb_text.Append("\\");
            else if (vk == 0xDD)
                sb_text.Append("]");
            else if (vk == 0xDE)
                sb_text.Append("\"");
            else if (vk == 0x6A)
                sb_text.Append("*");
            else if (vk == 0x6B)
                sb_text.Append("+");
            else if (vk == 0x6C)
                sb_text.Append(".");
            else if (vk == 0x6D)
                sb_text.Append("-");
            else if (vk == 0x6F)
                sb_text.Append("/");
            else if (expand_vks)
                sb_text.Append("[vk=" + vk + "]");
            else
                sb_text.Append("¿");

            return sb_text.ToString();
        }

        public int[] FTs
        {
            get
            {
                return Features[TypingFeature.FT];
            }
        }

        public int[] HTs
        {
            get
            {
                return Features[TypingFeature.HT];
            }
        }

        public string Text
        {
            get
            {
                return GetSessionText();
            }
        }

        public string GetFeatureText(TypingFeature feature)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Features[feature].Length; i++)
            {
                if (sb.Length != 0)
                    sb.Append("-");

                if (Features[feature][i] < 0)
                    sb.Append("X");
                else
                    sb.Append(Features[feature][i]);
            }

            return sb.ToString();
        }

        StringBuilder sb_text;
        public string GetSessionText(bool expand_vks = false)
        {
            /*
            if (sb_text != null)
                return sb_text.ToString();
            */

            sb_text = new StringBuilder();
            for (int i = 0; i < this.VKs.Length; i++)
            {
                byte vk = this.VKs[i];
                sb_text.Append(GetVKString(vk, expand_vks));
            }

            return sb_text.ToString();
        }

        public void Serialize(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(ID);
            writer.Write(User.UserID);
            writer.Write(Timestamp.Ticks);
            writer.Write(UserAgent.GetHashCode());

            writer.Write(VKs.Length);
            writer.Write(VKs, 0, VKs.Length);

            int[] HTs = Features[TypingFeature.HT];
            writer.Write(HTs.Length);
            byte[] tmp = new byte[sizeof(int) * HTs.Length];
            Buffer.BlockCopy(HTs, 0, tmp, 0, sizeof(int) * HTs.Length);
            writer.Write(tmp, 0, tmp.Length);

            int[] FTs = Features[TypingFeature.FT];
            writer.Write(FTs.Length);
            tmp = new byte[sizeof(int) * HTs.Length];
            Buffer.BlockCopy(FTs, 0, tmp, 0, sizeof(int) * HTs.Length);
            writer.Write(tmp, 0, tmp.Length);
        }

        public static Sample Deserialize(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            int id = reader.ReadInt32();
            int user_id = reader.ReadInt32();
            long ticks = reader.ReadInt64();

            int hash = reader.ReadInt32();

            int size = reader.ReadInt32();

            byte[] vks = new byte[size];
            vks = reader.ReadBytes(size);

            size = reader.ReadInt32();

            int[] hts = new int[size];
            byte[] buffer = reader.ReadBytes(sizeof(int) * size);
            Buffer.BlockCopy(buffer, 0, hts, 0, sizeof(int) * size);

            size = reader.ReadInt32();

            int[] fts = new int[size];
            buffer = reader.ReadBytes(sizeof(int) * size);
            Buffer.BlockCopy(buffer, 0, fts, 0, sizeof(int) * size);

            return new Sample
                (
                    id,
                    User.GetUser(user_id, user_id.ToString(), DateTime.MinValue, Gender.Unknown),
                    new DateTime(ticks),
                    "",
                    vks,
                    hts,
                    fts
                );
        }

        public Sample[] SplitByVK(VirtualKeys key)
        {
            List<Sample> retval = new List<Sample>();

            int start = 0;
            for (int i = 0; i < this.VKs.Length; i++)
                if (this.VKs[i] == (byte)key)
                {
                    if (start < i - 1)
                    {
                        Sample split = this.Split(start, i - 1);
                        retval.Add(split);
                    }

                    start = i + 1;
                }

            if (start != VKs.Length)
            {
                Sample split = this.Split(start, VKs.Length - 1);
                retval.Add(split);
            }

            return retval.ToArray();
        }

        public static Sample Join(List<Sample> sessions)
        {
            int total_length = sessions.Select(s => s.VKs.Length).Sum();

            byte[] vks = new byte[total_length];
            int[] hts = new int[total_length];
            int[] fts = new int[total_length];

            int pos = 0;
            foreach (Sample session in sessions)
            {
                for (int i = 0; i < session.VKs.Length; i++)
                {
                    vks[pos + i] = session.VKs[i];
                    hts[pos + i] = session.Features[TypingFeature.HT][i];
                    fts[pos + i] = session.Features[TypingFeature.FT][i];
                }

                pos += session.VKs.Length;
            }

            return new Sample(vks, hts, fts);
        }

        public static Sample SingleSpace
        {
            get
            {
                return new Sample(new byte[1] { (byte)VirtualKeys.VK_SPACE },
                    new int[1],
                    new int[1]
                    );
            }
        }

        public static Sample SinglePeriod
        {
            get
            {
                return new Sample(new byte[1] { (byte)VirtualKeys.VK_OEM_PERIOD },
                    new int[1],
                    new int[1]
                    );
            }
        }

        public int CompareTo(Sample session)
        {
            if (VKs.Length < session.VKs.Length)
                return -1;
            if (VKs.Length > session.VKs.Length)
                return 1;

            for (int i = 0; i < VKs.Length; i++)
            {
                if (VKs[i] < session.VKs[i])
                    return -1;
                if (VKs[i] > session.VKs[i])
                    return 1;
            }

            return 0;
        }

        public override string ToString()
        {
            string retval = GetSessionText(true);
            if (retval.Length > 100)
                return retval.Substring(0, 100);
            else
                return retval;
        }

        #region SERIALIZATION
        public object[] GetCsvHeader()
        {
            object[] row = new object[2 * Length];
            row[0] = "USER";

            for (int i = 1; i < Length; i++)
                row[i] = "FT" + i;
            for (int i = 0; i < Length; i++)
                row[Length + i] = "HT" + i;

            return row;
        }
        public object[] GetCsvRow()
        {
            object[] row = new object[2 * Length];
            row[0] = User.UserID;

            for (int i = 1; i < Length; i++)
                row[i] = Features[TypingFeature.FT][i];
            for (int i = 0; i < Length; i++)
                row[Length + i] = Features[TypingFeature.HT][i];

            return row;
        }

        public object[] GetCsvRowAuthentication(bool is_legitimate)
        {
            object[] row = new object[2 * Length];
            row[0] = is_legitimate ? "legitimate" : "impostor";

            for (int i = 1; i < Length; i++)
                row[i] = Features[TypingFeature.FT][i];
            for (int i = 0; i < Length; i++)
                row[Length + i] = Features[TypingFeature.HT][i];

            return row;
        }

        public void WriteToCsv(CsvWriter writer)
        {
            if (writer.Rows == 0)
                writer.WriteLine(GetCsvHeader());

            writer.WriteLine(GetCsvRow());
        }

        public void WriteToCsvAuthentication(CsvWriter writer, bool is_legitimate)
        {
            if (writer.Rows == 0)
                writer.WriteLine(GetCsvHeader());

            writer.WriteLine(GetCsvRowAuthentication(is_legitimate));
        }
        #endregion

        #region STATISTICS
        public static bool IsAlphanumeric(byte vk)
        {
            return (vk >= 0x30 && vk <= 0x39) || (vk >= 0x41 && vk <= 0x5A) || (vk >= 0x60 && vk <= 0x69);
        }

        public double AlphanumericRate
        {
            get
            {
                int valid = 0;
                foreach (byte vk in VKs)
                    if (IsAlphanumeric(vk))
                        valid++;

                return 100.0 * valid / VKs.Length;
            }
        }

        public double AlphanumericVariety
        {
            get
            {
                HashSet<byte> an_vks = new HashSet<byte>();
                foreach (byte vk in VKs)
                    if (IsAlphanumeric(vk))
                        if (!an_vks.Contains(vk))
                            an_vks.Add(vk);

                return an_vks.Count;
            }
        }
        #endregion

        #region STATISTICAL PARAMETERS OF FEATURES
        public double GetFeatureMean(TypingFeature feature)
        {
            double sum = 0.0;
            int count = 0;
            for (int i = 0; i < this.VKs.Length; i++)
                if (Features[feature][i] != int.MinValue && Features[feature][i] != int.MaxValue)
                {
                    sum += Features[feature][i];
                    count++;
                }

            return sum / count;
        }

        public double GetFeatureCovariance(TypingFeature feature, double mean, int offset)
        {
            double sum = 0.0;
            int count = 0;
            
            for (int i = 0; i < VKs.Length - offset; i++)
                if (Features[feature][i] != int.MinValue && Features[feature][i + offset] != int.MinValue &&
                    Features[feature][i] != int.MaxValue && Features[feature][i + offset] != int.MaxValue)
                {
                    double a = Features[feature][i] - mean;
                    double b = Features[feature][i + offset] - mean;
                    sum += a * b;
                    count++;
                }

            return sum / count;
        }

        public double GetFeatureCovariance(TypingFeature feature, int offset)
        {
            return GetFeatureCovariance(feature, GetFeatureMean(feature), offset);
        }

        public double GetFeatureVariance(TypingFeature feature, double mean)
        {
            return GetFeatureCovariance(feature, mean, 0);
        }

        public double GetFeatureVariance(TypingFeature feature)
        {
            return GetFeatureCovariance(feature, GetFeatureMean(feature), 0);
        }

        #endregion

        public bool IsAlphabetic(byte vk)
        {
            return (vk >= (byte)VirtualKeys.VK_A && vk <= (byte)VirtualKeys.VK_Z);
        }

        public bool IsAlphaNumericVK(byte vk)
        {
            return (vk >= (byte) VirtualKeys.VK_A && vk <= (byte) VirtualKeys.VK_Z) || 
                (vk >= (byte) VirtualKeys.VK_0 && vk <= (byte) VirtualKeys.VK_9);
        }

        public bool IsWordEndVK(byte vk)
        {
            return vk == (byte)VirtualKeys.VK_SPACE || 
                (vk >= (byte)VirtualKeys.VK_OEM_1 && vk <= (byte)VirtualKeys.VK_OEM_7);
        }

        public Sample[] SplitWords()
        {
            List<Sample> retval = new List<Sample>();

            int start_pos = 0;
            int end_pos = 0;

            while (end_pos < VKs.Length)
            { 
                bool word_found = false;
                for (end_pos = start_pos; end_pos < VKs.Length; end_pos++)
                    if (!IsAlphabetic(VKs[end_pos]))
                    {
                        if (IsWordEndVK(VKs[end_pos]))
                            word_found = true;

                        break;
                    }


                if (word_found && start_pos != end_pos)
                {
                    Sample word = Split(start_pos, end_pos - 1);
                    retval.Add(word);
                }

                start_pos = end_pos + 1;
            }

            return retval.ToArray();
        }

        #region EVALUATE SESSION QUALITY
        public bool FeatureIsGood(TypingFeature feature, int pos)
        {
            int val = Features[feature][pos];
            return val != int.MinValue && val != int.MaxValue && val > 0 && val < 1500;
        }

        public bool IsGood
        {
            get
            {
                if (!FeatureIsGood(TypingFeature.HT, 0))
                    return false;

                for (int i = 1; i < Length; i++)
                {
                    if (!FeatureIsGood(TypingFeature.HT, i))
                        return false;

                    if (!FeatureIsGood(TypingFeature.FT, i))
                        return false;
                }

                return true;
            }
        }

        public double GetAlphanumericAverage(TypingFeature feature)
        {
            double sum = 0.0;
            int count = 0;

            for (int i = 0; i < VKs.Length; i++)
                if (Sample.IsAlphanumeric(VKs[i]) && Features[feature][i] >= 0 && Features[feature][i] <= 1500)
                {
                    sum += Features[feature][i];
                    count++;
                }

            return sum / count;
        }
        #endregion
    }
}
