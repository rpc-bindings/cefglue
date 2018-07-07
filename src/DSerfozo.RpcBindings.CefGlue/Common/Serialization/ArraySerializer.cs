using System;
using System.Collections.Generic;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public sealed class ArraySerializer : ITypeSerializer, ITypeDeserializer
    {
        public bool CanHandle(Type type)
        {
            return type?.IsArray == true;
        }

        public bool CanHandle(CefValue cefValue, Type targetType)
        {
            return cefValue.GetValueType() == CefValueType.List &&
                   (targetType?.IsArray == true || targetType == typeof(object));
        }

        public CefValue Serialize(object obj, HashSet<object> seen, ObjectSerializer objectSerializer)
        {
            var type = obj?.GetType();

            if (!CanHandle(type))
            {
                throw new InvalidOperationException();
            }

            var array = (Array)obj;
            using (var value = CefListValue.Create())
            {
                for (var i = 0; i < array.Length; i++)
                {
                    value.SetValue(i, objectSerializer.Serialize(array.GetValue(i), seen));
                }

                var result = CefValue.Create();
                result.SetList(value);
                return result;
            }
        }

        public object Deserialize(CefValue source, Type targetType, ObjectSerializer objectSerializer)
        {
            if (!CanHandle(source, targetType))
            {
                throw new InvalidOperationException();
            }

            using (var lstVal = source.GetList())
            {
                var elementType = targetType.GetElementType();
                var array = Activator.CreateInstance(targetType, lstVal.Count) as Array;

                for (var i = 0; i < lstVal.Count; i++)
                {
                    array.SetValue(objectSerializer.Deserialize(lstVal.GetValue(i), elementType), i);
                }

                return array;
            }
        }
    }
}
