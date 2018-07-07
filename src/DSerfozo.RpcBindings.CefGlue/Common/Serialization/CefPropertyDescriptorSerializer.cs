using System;
using System.Collections.Generic;
using DSerfozo.RpcBindings.CefGlue.Renderer.Model;
using DSerfozo.RpcBindings.Model;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public class CefPropertyDescriptorSerializer : ITypeSerializer, ITypeDeserializer
    {
        public bool CanHandle(Type type)
        {
            return type == typeof(PropertyDescriptor);
        }

        public bool CanHandle(CefValue cefValue, Type targetType)
        {
            return cefValue.GetValueType() == CefValueType.Dictionary && targetType == typeof(PropertyDescriptor);
        }

        public CefValue Serialize(object source, HashSet<object> seen, ObjectSerializer objectSerializer)
        {
            if (!CanHandle(source?.GetType()))
            {
                throw new InvalidOperationException();
            }

            var actualSource = (PropertyDescriptor) source;
            var result = CefValue.Create();
            using (var resultDict = CefDictionaryValue.Create())
            using (var value = CefDictionaryValue.Create())
            {
                resultDict.SetString(ObjectSerializer.TypeIdPropertyName, PropertyDescriptor.TypeId);

                value.SetInt64(nameof(actualSource.Id), actualSource.Id);
                value.SetString(nameof(PropertyDescriptor.Name), actualSource.Name);
                value.SetValue(nameof(PropertyDescriptor.Value),
                    objectSerializer.Serialize(actualSource.Value, new HashSet<object>()));

                resultDict.SetDictionary(ObjectSerializer.ValuePropertyName, value);
                result.SetDictionary(resultDict);
            }

            return result;
        }

        public object Deserialize(CefValue value, Type targetType, ObjectSerializer objectSerializer)
        {
            if (!CanHandle(value, targetType))
            {
                throw new InvalidOperationException();
            }

            using (var dict = value.GetDictionary())
            using (var val = dict.GetDictionary(ObjectSerializer.ValuePropertyName))
            {
                var id = val.GetInt64(nameof(PropertyDescriptor.Id));
                var name = val.GetString(nameof(PropertyDescriptor.Name));
                var propValue = val.GetValue(nameof(PropertyDescriptor.Value)).Copy();

                return new CefPropertyDescriptor(id, name, propValue);
            }
        }
    }
}
