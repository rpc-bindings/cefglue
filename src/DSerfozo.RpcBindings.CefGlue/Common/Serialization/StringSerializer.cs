using System;
using System.Collections.Generic;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public sealed class StringSerializer : ITypeSerializer, ITypeDeserializer
    {
        public bool CanHandle(Type type)
        {
            return type == typeof(string);
        }

        public bool CanHandle(CefValue cefValue, Type targetType)
        {
            return cefValue.GetValueType() == CefValueType.String &&
                   (targetType == typeof(string) || targetType == typeof(object));
        }

        public CefValue Serialize(object source, HashSet<object> seen, ObjectSerializer objectSerializer)
        {
            if (!CanHandle(source.GetType()))
            {
                throw new InvalidOperationException();
            }

            var result = CefValue.Create();
            result.SetString((string) source);

            return result;
        }

        public object Deserialize(CefValue value, Type targetType, ObjectSerializer objectSerializer)
        {
            if (!CanHandle(value, targetType))
            {
                throw new InvalidOperationException();
            }

            return value.GetString();
        }
    }
}
