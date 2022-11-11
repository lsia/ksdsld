using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.Models;


namespace KSDSLD.FiniteContexts.ModelStorages
{
    class MemoryOptimizedStorage : ModelStorage
    {
        public MemoryOptimizedStorage(User user, string name, TypingFeature feature, int max_context_order, int max_ngram_order, IModelFactory<Model> factory)
            : base(user, name, feature, max_context_order, max_ngram_order, factory)
        {
        }

        public override bool MustFeedManually { get { return false; } }

        object lock_sessions = new object();
        List<Sample> sessions = new List<Sample>();
        public override void FeedSession(Sample session)
        {
            lock (lock_sessions)
                sessions.Add(session);
        }

        public override Model GetModel(bool create, int context_order, int ngram_order, ulong model_hash)
        {
            throw new NotImplementedException();
        }

        public override Model[] GetBulk(int context_order, ulong[] model_hashes)
        {
            int ngram_length = 1;
            Dictionary<ulong, Model> models = new Dictionary<ulong, Model>();
            foreach (var candidate_hash in model_hashes)
                if (!models.ContainsKey(candidate_hash))
                {
                    Model model = Factory.CreateModel(candidate_hash, (short)context_order, (int)candidate_hash, (short) ngram_length);
                    models.Add(candidate_hash, model);
                }

            Sample[] sessions = null;
            lock (lock_sessions)
                sessions = this.sessions.ToArray();

            foreach (Sample session in sessions)
            {
                int partition_pos = 0;
                ulong context = 0xFF;
                for (int i = 0; i < session.VKs.Length; i++)
                {
                    // Restart contexts on partition boundaries
                    if (partition_pos < session.PartitionOffsets.Length && session.PartitionOffsets[partition_pos] == i)
                    {
                        partition_pos++;
                        context = 0xFF;
                    }

                    ulong ngram = (ulong)session.VKs[i];

                    for (int current_context_order = context_order; current_context_order >= 0; current_context_order--)
                    {
                        ulong order_mask = ((ulong)1 << (8 * current_context_order)) - 1;
                        ulong current_context = context & order_mask;
                        ulong model_hash = (ngram << 8 * (sizeof(ulong) - ngram_length)) | current_context;

                        if (models.ContainsKey(model_hash))
                        {
                            int[] parameter_values = session.Features[Feature];
                            if (parameter_values[i] != int.MinValue)
                                models[model_hash].Feed(parameter_values, i);
                        }
                    }

                    context <<= 8;
                    context |= (ulong)session.VKs[i];
                }
            }

            Model[] retval = new Model[model_hashes.Length];
            for (int i = 0; i < model_hashes.Length; i++)
            {
                ulong current_hash = model_hashes[i];
                retval[i] = models[current_hash];
            }

            return retval;
        }
    }
}
