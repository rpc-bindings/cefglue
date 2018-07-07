using System;
using System.Collections.Generic;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public sealed class ValueTypeSerializer : ITypeSerializer, ITypeDeserializer
    {
        public bool CanHandle(Type type)
        {
            return type == typeof(decimal) || type == typeof(DateTime);
        }

        public bool CanHandle(CefValue cefValue, Type targetType)
        {
            return (targetType == typeof(decimal) && (cefValue.GetValueType() == CefValueType.Double ||
                                                      cefValue.GetValueType() == CefValueType.Int)) ||
                   (targetType == typeof(DateTime) && cefValue.IsType(CefTypes.Time));
        }

        public CefValue Serialize(object obj, HashSet<object> seen, ObjectSerializer objectSerializer)
        {
            var type = obj.GetType();
            if (!CanHandle(type))
            {
                throw new InvalidOperationException();
            }

            var result = CefValue.Create();
            if (type == typeof(DateTime))
            {
                result.SetTime((DateTime)obj);
            }
            else if (type == typeof(decimal))
            {
                result.SetDouble(Convert.ToDouble((decimal)obj));
            }
            return result;
        }

        public object Deserialize(CefValue value, Type targetType, ObjectSerializer objectSerializer)
        {
            if (!CanHandle(value, targetType))
            {
                throw new InvalidOperationException();
            }

            object result = null;
            if (value.IsType(CefTypes.Time) && targetType == typeof(DateTime))
            {
                result = value.GetTime();
            }
            else if(targetType == typeof(decimal))
            {
                var cefValueType = value.GetValueType();
                switch (cefValueType)
                {
                    case CefValueType.Int:
                        result = Convert.ToDecimal(value.GetInt());
                        break;
                    case CefValueType.Double:
                        result = Convert.ToDecimal(value.GetDouble());
                        break;
                }
            }

            return result;
        }
    }
}
