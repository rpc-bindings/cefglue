using System;
using System.Collections.Generic;
using DSerfozo.CefGlue.Contract.Common;
using static DSerfozo.CefGlue.Contract.Common.CefFactories;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public sealed class StringSerializer : ITypeSerializer, ITypeDeserializer
    {
        public bool CanHandle(Type type)
        {
            return type == typeof(string);
        }

        public bool CanHandle(ICefValue cefValue, Type targetType)
        {
            return cefValue.GetValueType() == CefValueType.String &&
                   (targetType == typeof(string) || targetType == typeof(object));
        }

        public ICefValue Serialize(object source, HashSet<object> seen, ObjectSerializer objectSerializer)
        {
            if (!CanHandle(source.GetType()))
            {
                throw new InvalidOperationException();
            }

            var result = CefValue.Create();
            result.SetString((string) source);

            return result;
        }

        public object Deserialize(ICefValue value, Type targetType, ObjectSerializer objectSerializer)
        {
            if (!CanHandle(value, targetType))
            {
                throw new InvalidOperationException();
            }

            return value.GetString();
        }
    }
}
