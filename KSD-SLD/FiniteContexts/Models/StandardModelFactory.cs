using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;


namespace KSDSLD.FiniteContexts.Models
{
    class StandardModelFactory : IModelFactory<Model>
    {
        public string Type { get; private set; }
        public StandardModelFactory(string type)
        {
            Type = type;
        }

        public bool Initialize()
        {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach ( Type t in types )
                if ( t.Name == Type )
                {
                    Verify(t);
                    model_type = t;
                    return true;
                }

            return false;
        }

        bool Verify(Type t)
        {
            bool is_model = false;
            Type current = t;
            for (int i = 0; i < 30 && current != typeof(object) && current != null; i++)
            {
                if (current == typeof(Model))
                {
                    is_model = true;
                    break;
                }

                current = current.BaseType;
            }

            if (!is_model)
                throw new ArgumentException("The type " + Type + " is not a Model.");

            return true;
        }

        Type model_type;
        public Model CreateModel(ulong context, short context_order, int ngram, short ngram_order)
        {
            if (Type == "AvgStdevModelLinear")
                return new AvgStdevModelLinear(context, context_order, ngram, ngram_order);
            else if (Type == "SimpleLinearDirectionalityModel")
                return new Directionality.SimpleLinearDirectionalityModel(context, context_order, ngram, ngram_order);
            else if (Type == "SimpleExponentialDirectionalityModel")
                return new Directionality.SimpleExponentialDirectionalityModel(context, context_order, ngram, ngram_order);
            else if (Type == "HistogramModel")
                return new Histogram.HistogramModel(context, context_order, ngram, ngram_order);
            else
                return (Model)Activator.CreateInstance(model_type, context, context_order, ngram, ngram_order);
        }

        public Type DefaultModelType
        {
            get
            {
                return model_type;
            }
        }
    }
}
