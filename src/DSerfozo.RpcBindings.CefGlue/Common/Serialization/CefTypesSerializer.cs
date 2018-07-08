using System;
using System.Collections.Generic;
using System.Linq;
using DSerfozo.CefGlue.Contract.Common;
using static DSerfozo.CefGlue.Contract.Common.CefFactories;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public sealed class CefTypesSerializer : ITypeSerializer, ITypeDeserializer
    {
        private static readonly HashSet<Type> CefTypes = new HashSet<Type>
        {
            typeof(ICefValue),
            typeof(ICefDictionaryValue),
            typeof(ICefListValue)
        };

        public bool CanHandle(Type type)
        {
            return CefTypes.Any(t => t.IsAssignableFrom(type));
        }

        public bool CanHandle(ICefValue cefValue, Type targetType)
        {
            return targetType == typeof(ICefValue);
        }

        public ICefValue Serialize(object source, HashSet<object> seen, ObjectSerializer objectSerializer)
        {
            if (source is ICefValue value)
            {
                return value;
            }
            else if (source is ICefDictionaryValue)
            {
                var result = CefValue.Create();
                result.SetDictionary((ICefDictionaryValue) source);
                return result;
            }
            else if (source is ICefListValue)
            {
                var result = CefValue.Create();
                result.SetList((ICefListValue) source);
                return result;
            }

            return null;
        }

        public object Deserialize(ICefValue value, Type targetType, ObjectSerializer objectSerializer)
        {
            if (!CanHandle(value, targetType))
            {
                throw new InvalidOperationException();
            }

            return value.Copy();
        }
    }
}
