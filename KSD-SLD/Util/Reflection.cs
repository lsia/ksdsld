using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;


namespace KSDSLD.Util
{
    static class Reflection
    {
        public static Type GetInterfaceType<T>(string name)
        {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type type in types)
                if (type.Name == name)
                {
                    if ( !type.GetInterfaces().Contains(typeof(T)))
                        throw new ArgumentException("Type " + name + " does not implement " + typeof(T).Name + ".");

                    return type;
                }

            throw new ArgumentException("No such type (" + name + ").");
        }
        
        public static T CreateInterfaceType<T>(string name)
        {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type type in types)
                if (type.Name == name)
                {
                    if (!type.GetInterfaces().Contains(typeof(T)))
                        throw new ArgumentException("Type " + name + " does not implement " + typeof(T).Name + ".");

                    return (T) Activator.CreateInstance(type);
                }

            throw new ArgumentException("No such type (" + name + ").");
        }
    }
}
