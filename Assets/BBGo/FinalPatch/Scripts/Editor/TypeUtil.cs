using System;
using System.Collections.Generic;
using System.Reflection;

namespace BBGo.FinalPatch
{
    public static class TypeUtil
    {
        private static Dictionary<Type, List<Type>> s_subTypes = new Dictionary<Type, List<Type>>();
        public static List<Type> GetSubTypes(Type type, Assembly assembly)
        {
            List<Type> subTypes;
            if (!s_subTypes.TryGetValue(type, out subTypes))
            {
                subTypes = new List<Type>();
                foreach (var t in assembly.GetTypes())
                {
                    if (t.IsAbstract || !t.IsClass)
                        continue;

                    if (!type.IsAssignableFrom(t))
                        continue;

                    if (t.GetConstructor(new Type[] { }) == null)
                        continue;

                    subTypes.Add(t);
                }
                s_subTypes.Add(type, subTypes);
            }
            return subTypes;
        }
    }
}