using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using DSerfozo.RpcBindings.CefGlue.Renderer.Model;
using DSerfozo.RpcBindings.Contract.Communication.Model;
using DSerfozo.RpcBindings.Contract.Marshaling;
using DSerfozo.RpcBindings.Contract.Marshaling.Model;
using DSerfozo.RpcBindings.Model;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public sealed class ComplexTypeSerializer : ITypeSerializer, ITypeDeserializer
    {
        public static readonly IDictionary<string, Type> KnownTypes = new Dictionary<string, Type>()
        {
            {ObjectDescriptor.TypeId, typeof(ObjectDescriptor)},
            {RpcRequest<CefValue>.TypeId, typeof(RpcRequest<CefValue>)},
            {RpcResponse<CefValue>.TypeId, typeof(RpcResponse<CefValue>)},
            {CallbackDescriptor.TypeId, typeof(CallbackDescriptor)},
            {ObjectSerializer.DictionaryTypeId, typeof(Dictionary<string, object>)},
            {MethodDescriptor.TypeId, typeof(MethodDescriptor) },
        };

        public bool CanHandle(Type type)
        {
            return type?.IsPrimitive == false && !type.IsValueType && !type.IsEnum && type != typeof(Type);
        }

        public bool CanHandle(CefValue cefValue, Type targetType)
        {
            return cefValue.GetValueType() == CefValueType.Dictionary && targetType?.IsPrimitive == false &&
                   !targetType.IsValueType &&
                   !targetType.IsEnum;
        }

        public CefValue Serialize(object obj, HashSet<object> seen, ObjectSerializer objectSerializer)
        {
            var type = obj?.GetType();

            if (!CanHandle(type))
            {
                throw new InvalidOperationException();
            }

            if (type == typeof(string))
            {
                var result = CefValue.Create();
                result.SetString((string) obj);

                return result;
            }

            using (var value = CefDictionaryValue.Create())
            using (var dict = CefDictionaryValue.Create())
            {
                value.SetString(ObjectSerializer.TypeIdPropertyName, GetTypeId(type));
                var properties = type.GetProperties();
                var shouldFilter = type.GetCustomAttribute<DataContractAttribute>() != null;

                foreach (var propertyDesc in properties
                    .Where(p => p.GetIndexParameters().Length <= 0)
                    .Select(p => new {Property = p, DataMember = p.GetCustomAttribute<DataMemberAttribute>()})
                    .Where(p => p.DataMember != null || !shouldFilter))
                {
                    var propertyValue = propertyDesc.Property.GetValue(obj);
                    var name = propertyDesc.DataMember?.Name ?? propertyDesc.Property.Name;

                    var cefValue = objectSerializer.Serialize(propertyValue, seen);
                    dict.SetValue(name, cefValue);
                }

                value.SetDictionary(ObjectSerializer.ValuePropertyName, dict);

                var result = CefValue.Create();
                result.SetDictionary(value);

                return result;
            }
        }

        private static string GetTypeId(Type type)
        {
            var typeIdAttribute = type.GetCustomAttribute<TypeIdAttribute>();
            return typeIdAttribute?.Id ?? ObjectSerializer.DictionaryTypeId;
        }

        public object Deserialize(CefValue value, Type targetType, ObjectSerializer objectSerializer)
        {
            if (!CanHandle(value, targetType))
            {
                throw new InvalidOperationException();
            }

            object result = null;
            using (var dictVal = value.GetDictionary())
            {
                var typeId = dictVal.GetString(ObjectSerializer.TypeIdPropertyName);
                using (var actualValue = dictVal.GetDictionary(ObjectSerializer.ValuePropertyName))
                {
                    KnownTypes.TryGetValue(typeId, out var type);
                    if (type != null && (targetType == typeof(object) || targetType.IsAssignableFrom(type)))
                    {
                        targetType = type;
                    }

                    try
                    {
                        result = Activator.CreateInstance(targetType);
                        var properties = targetType.GetProperties()
                            .Select(p =>
                                new {Prop = p, DataMember = p.GetCustomAttribute<DataMemberAttribute>()})
                            .ToDictionary(k => k.DataMember?.Name ?? k.Prop.Name, v => v.Prop);
                        var keys = actualValue.GetKeys();
                        foreach (var dictKey in keys)
                        {
                            if (properties.TryGetValue(dictKey, out var matchingProperty))
                            {
                                matchingProperty.SetValue(result,
                                    objectSerializer.Deserialize(actualValue.GetValue(dictKey),
                                        matchingProperty.PropertyType));
                            }
                        }
                    }
                    catch
                    {
                        //TODO:logging
                    }
                }
            }

            return result;
        }
    }
}
