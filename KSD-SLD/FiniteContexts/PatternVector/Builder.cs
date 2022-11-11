using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;

using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.ContextSelection;
using KSDSLD.FiniteContexts.Models;
using KSDSLD.FiniteContexts.ModelStorages;


namespace KSDSLD.FiniteContexts.PatternVector
{
    public class Builder
    {
        static Logger log = LogManager.GetCurrentClassLogger();
        public ModelStorage Storage { get; private set; }

        public Builder(ModelStorage feeder)
        {
            Storage = feeder;
        }


        public string Name
        {
            get
            {
                return Storage.Name + "_" + Storage.Feature;
            }
        }
        /*
        public static int MIN_OBSERVATIONS = 10;

        Model ChooseModel(Model[] models_found, int offset)
        {
            for (int i = offset + Storage.MaxContextOrder; i >= offset; i--)
                if (models_found[i] != null && models_found[i].Count >= MIN_OBSERVATIONS)
                    return models_found[i];

            return null;
        }
        */

        public ContextSelector ContextSelector { get; private set; } = new LongestAvailableContextSelector();


        public delegate void OnModelChosenDelegate(TypingFeature feature, Sample session, int pos, ulong context, Model[] candidates, Model chosen);
        public event OnModelChosenDelegate OnModelChosen;

//        public Model[] ChosenModels;

        public Model[] Rebuild(Sample session)
        {
            Model[] retval = new Model[session.VKs.Length];
            ulong[] model_hashes = new ulong[(Storage.MaxContextOrder + 1) * session.VKs.Length];

            int partition_pos = 0;
            int context_order = 1;
            ulong context = 0xFF;
            for (int i = 0; i < session.VKs.Length; i++)
            {
                // Restart contexts on partition boundaries
                if (partition_pos < session.PartitionOffsets.Length && session.PartitionOffsets[partition_pos] == i)
                {
                    partition_pos++;
                    context_order = 1;
                    context = 0xFF;
                }

                int ngram_length = 1;
                ulong ngram = (ulong) session.VKs[i];

                int order = Math.Min(Storage.MaxContextOrder, context_order);
                ulong order_mask = ((ulong) 1 << (8 * order)) - 1;
                do
                {
                    ulong current_context = context & order_mask;
                    // ulong model_hash = (ulong)((current_context << (8 * ngram_length)) | ngram);
                    ulong model_hash = (ngram << 8 * (sizeof(ulong) - ngram_length)) | current_context;

                    model_hashes[order + i * (Storage.MaxContextOrder + 1)] = model_hash;

                    order--;
                    order_mask >>= 8;
                }
                while (order >= 0);

                context_order++;
                context <<= 8;
                context |= (ulong) session.VKs[i];
            }

            Model[] models_found = Storage.GetBulk(Storage.MaxContextOrder, model_hashes);
            List<Model> chosen_models = new List<Model>();

            for (int i = 0; i < session.VKs.Length; i++)
            {
                // Model best_model = ChooseModel(models_found, i * (Storage.MaxContextOrder + 1));
                int offset = i * (Storage.MaxContextOrder + 1);
                Model best_model = ContextSelector.ChooseModel(models_found, Storage.MaxContextOrder, offset);
                OnModelChosen?.Invoke(Storage.Feature, session, i, context, models_found, best_model);

                chosen_models.Add(best_model);
            }

            return chosen_models.ToArray();
        }
    }
}
