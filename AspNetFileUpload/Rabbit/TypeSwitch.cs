using System;
using System.Collections.Generic;

namespace AspNetFileUpload.Rabbit
{
    public class TypeSwitch
    {
        Dictionary<Type, Func<object, bool>> matches = new Dictionary<Type, Func<object, bool>>();

        public TypeSwitch Case<T>(Func<T, bool> action)
        {
            matches.Add(typeof(T), (x) => action((T)x)); return this;
        }

        public bool Switch(object x)
        {
            return matches[x.GetType()](x);
        }
    }
}