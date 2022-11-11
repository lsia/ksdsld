using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Datasets;
using KSDSLD.Pipelines;
using KSDSLD.Util;


namespace KSDSLD.Util
{
    static class SentenceUtil
    {
        public static Dictionary<int, Sample[]> SplitBySentenceSize
            (
                Results results, 
                int min_sentence_size = -1
            )
        {
            List<Sample> sentences = new List<Sample>();
            ExperimentParallelization.ForEachSession(results, (d, s) =>
            {
                int start = 0;
                for (int i = 0; i < s.VKs.Length; i++)
                    if (s.VKs[i] == (byte) VirtualKeys.VK_OEM_PERIOD &&
                        (min_sentence_size == -1 || (i - start) >= min_sentence_size))
                    {
                        Sample split = s.Split(start, i);
                        sentences.Add(split);
                        start = i + 1;
                    }
            });

            return sentences.GroupBy(s => s.VKs.Length).ToDictionary(g => g.Key, g => g.ToArray());
        }

        public static Sample[] Split(Sample[] sessions)
        {
            var sentences = SplitBySentenceSize(sessions);

            List<Sample> retval = new List<Sample>();
            foreach (var kv in sentences)
                foreach (var sentence in kv.Value)
                    retval.Add(sentence);

            return retval.ToArray();
        }

        public static Dictionary<int, Sample[]> SplitBySentenceSize(Sample[] sessions)
        {
            object lock_sentences = new object();
            List<Sample> sentences = new List<Sample>();
            Parallel.ForEach(sessions, s =>
            {
                int start = 0;
                for (int i = 0; i < s.VKs.Length; i++)
                    if (s.VKs[i] == (byte) VirtualKeys.VK_OEM_PERIOD)
                    {
                        Sample split = s.Split(start, i);
                        lock (lock_sentences)
                            sentences.Add(split);

                        start = i + 1;
                    }

                if (start < (s.VKs.Length - 5))
                    lock (lock_sentences)
                        sentences.Add(s.Split(start));
            });

            Dictionary<int, List<Sample>> tmp = new Dictionary<int, List<Sample>>();
            foreach (var sentence in sentences)
            {
                if (!tmp.ContainsKey(sentence.Length))
                    tmp.Add(sentence.Length, new List<Sample>());

                tmp[sentence.Length].Add(sentence);
            }

            Dictionary<int, Sample[]> retval = new Dictionary<int, Sample[]>();
            foreach (var kv in tmp)
                retval.Add(kv.Key, kv.Value.ToArray());

            return retval;
            /*
            return sentences.GroupBy(s => s.VKs.Length).ToDictionary(g => {
                if (g == null)
                {
                    int k = 9;
                }


                return g.Key;
            }, 
                
                g => {
                    if (g == null)
                    {
                        int k = 9;
                    }

                    return g.ToArray();
                    });
            */
        }
    }
}
