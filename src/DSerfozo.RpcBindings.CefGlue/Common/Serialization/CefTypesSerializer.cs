using System;
using System.Collections.Generic;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public sealed class CefTypesSerializer : ITypeSerializer, ITypeDeserializer
    {
        private static readonly HashSet<Type> CefTypes = new HashSet<Type>
        {
            typeof(CefValue),
            typeof(CefDictionaryValue),
            typeof(CefListValue)
        };

        public bool CanHandle(Type type)
        {
            return CefTypes.Contains(type);
        }

        public bool CanHandle(CefValue cefValue, Type targetType)
        {
            return targetType == typeof(CefValue);
        }

        public CefValue Serialize(object source, HashSet<object> seen, ObjectSerializer objectSerializer)
        {
            if (source is CefValue value)
            {
                return value;
            }
            else if (source is CefDictionaryValue)
            {
                var result = CefValue.Create();
                result.SetDictionary((CefDictionaryValue) source);
                return result;
            }
            else if (source is CefListValue)
            {
                var result = CefValue.Create();
                result.SetList((CefListValue) source);
                return result;
            }

            return null;
        }

        public object Deserialize(CefValue value, Type targetType, ObjectSerializer objectSerializer)
        {
            if (!CanHandle(value, targetType))
            {
                throw new InvalidOperationException();
            }

            return value.Copy();
        }
    }
}
