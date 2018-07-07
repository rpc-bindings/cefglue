using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public class DictionarySerializer : ITypeSerializer, ITypeDeserializer
    {
        public bool CanHandle(Type type)
        {
            return type != null && IsDictionaryType(type);
        }

        public bool CanHandle(CefValue cefValue, Type targetType)
        {
            return IsDictionary(cefValue) && 
                  (IsDictionaryType(targetType) || targetType == typeof(object));
        }

        private static bool IsDictionary(CefValue value)
        {
            var result = value.GetValueType() == CefValueType.Dictionary;

            if (result)
            {
                using (var dict = value.GetDictionary())
                {
                    result = dict.HasKey(ObjectSerializer.TypeIdPropertyName) &&
                             dict.GetString(ObjectSerializer.TypeIdPropertyName) == ObjectSerializer.DictionaryTypeId;
                }
            }

            return result;
        }

        public CefValue Serialize(object source, HashSet<object> seen, ObjectSerializer objectSerializer)
        {
            if (!CanHandle(source?.GetType()))
            {
                throw new InvalidOperationException();
            }

            using (var value = CefDictionaryValue.Create())
            using (var dict = CefDictionaryValue.Create())
            {
                value.SetString(ObjectSerializer.TypeIdPropertyName, ObjectSerializer.DictionaryTypeId);

                foreach (DictionaryEntry dictionaryEntry in (IDictionary)source)
                {
                    var cefValue = objectSerializer.Serialize(dictionaryEntry.Value, seen);
                    dict.SetValue(dictionaryEntry.Key.ToString(), cefValue);
                }
                value.SetDictionary(ObjectSerializer.ValuePropertyName, dict);

                var result = CefValue.Create();
                result.SetDictionary(value);

                return result;
            }
        }

        public object Deserialize(CefValue value, Type targetType, ObjectSerializer objectSerializer)
        {
            if (!CanHandle(value, targetType))
            {
                throw new InvalidOperationException();
            }

            object result = null;
            using (var dictVal = value.GetDictionary())
                using(var actualVal = dictVal.GetDictionary(ObjectSerializer.ValuePropertyName))
            {
                var typeId = dictVal.GetString(ObjectSerializer.TypeIdPropertyName);
                if (typeId == ObjectSerializer.DictionaryTypeId)
                {
                    var keyType = typeof(string);
                    var valueType = typeof(object);

                    if (targetType == typeof(object))
                    {
                        targetType = typeof(Dictionary<string, object>);
                    }
                    else if (targetType.IsInterface)
                    {
                        if (targetType == typeof(IDictionary))
                        {
                            targetType = typeof(Dictionary<string, object>);
                        }
                        else
                        {
                            var generics = targetType.GetGenericArguments();
                            targetType = typeof(Dictionary<,>).MakeGenericType(generics);
                            keyType = generics[0];
                            valueType = generics[1];
                        }
                    }
                    else
                    {
                        var interfaces = targetType.GetInterfaces();
                        var generic = interfaces.FirstOrDefault(i =>
                            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
                        if (generic != null)
                        {
                            var genericArguments = generic.GetGenericArguments();
                            keyType = genericArguments[0];
                            valueType = genericArguments[1];
                        }
                    }

                    try
                    {
                        result = Activator.CreateInstance(targetType);
                        var keys = actualVal.GetKeys();
                        var addMethod = targetType.GetMethod("Add", new[] { keyType, valueType });
                        foreach (var key in keys)
                        {
                            object actualKey = key;
                            if (keyType == typeof(long))
                            {
                                actualKey = Convert.ToInt64(key);
                            }

                            addMethod.Invoke(result,
                                new[]
                                    {actualKey, objectSerializer.Deserialize(actualVal.GetValue(key), valueType)});
                        }
                    }
                    catch
                    {
                        //TODO: logging
                    }
                }
            }

            return result;
        }

        private bool IsDictionaryType(Type candidateType)
        {
            return typeof(IDictionary).IsAssignableFrom(candidateType) ||
                   (candidateType.IsGenericType &&
                    candidateType.GetGenericTypeDefinition() == typeof(IDictionary<,>)) || candidateType.GetInterfaces()
                       .Where(i => i.IsGenericType)
                       .Select(i => i.GetGenericTypeDefinition()).Any(i => typeof(IDictionary<,>) == i);
        }
    }
}
