using System;
using DSerfozo.CefGlue.Contract.Common;
using DSerfozo.CefGlue.Contract.Renderer;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;
using DSerfozo.RpcBindings.CefGlue.Renderer.Binding;
using DSerfozo.RpcBindings.CefGlue.Renderer.Util;
using DSerfozo.RpcBindings.Model;
using static DSerfozo.CefGlue.Contract.Renderer.CefFactories;
using static DSerfozo.CefGlue.Contract.Common.CefFactories;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Serialization
{
    public class V8Deserializer : ITypeDeserializer
    {
        private readonly SavedValueFactory<Promise> functionCallPromiseRegistry;

        public V8Deserializer(SavedValueFactory<Promise> functionCallPromiseRegistry)
        {
            this.functionCallPromiseRegistry = functionCallPromiseRegistry;
        }

        public bool CanHandle(ICefValue cefValue, Type targetType)
        {
            return targetType == typeof(ICefV8Value);
        }

        public object Deserialize(ICefValue value, Type targetType, ObjectSerializer objectSerializer)
        {
            if (!CanHandle(value, targetType))
            {
                throw new InvalidOperationException();
            }

            var valueType = value.GetValueType();

            if (valueType == CefValueType.String)
            {
                return CefV8Value.CreateString(value.GetString());
            }

            if (valueType == CefValueType.Int)
            {
                return CefV8Value.CreateInt(value.GetInt());
            }

            if (valueType == CefValueType.Double)
            {
                return CefV8Value.CreateDouble(value.GetDouble());
            }

            if (value.IsType(CefTypes.Int64))
            {
                return CefV8Value.CreateDouble(value.GetInt64());
            }

            if (value.IsType(CefTypes.Time))
            {
                return CefV8Value.CreateDate(value.GetTime());
            }

            if (valueType == CefValueType.Bool)
            {
                return CefV8Value.CreateBool(value.GetBool());
            }

            if (valueType == CefValueType.List)
            {
                using (var list = value.GetList())
                {
                    if (list.Count > 0)
                    {
                        var array = CefV8Value.CreateArray(list.Count);
                        for (var i = 0; i < list.Count; i++)
                        {
                            using (var cefValue = list.GetValue(i))
                            {
                                array.SetValue(i,
                                    (ICefV8Value) objectSerializer.Deserialize(cefValue, typeof(ICefV8Value)));
                            }
                        }

                        return array;
                    }
                }
            }

            if (valueType == CefValueType.Dictionary)
            {
                using (var dictionary = value.GetDictionary())
                using (var valDict = dictionary.GetDictionary(ObjectSerializer.ValuePropertyName))
                {
                    var typeId = dictionary.GetString(ObjectSerializer.TypeIdPropertyName);
                    if (typeId == ObjectSerializer.DictionaryTypeId)
                    {
                        var obj = CefV8Value.CreateObject();
                        foreach (var key in valDict.GetKeys())
                        {
                            obj.SetValue(key, (ICefV8Value)objectSerializer.Deserialize(valDict.GetValue(key), typeof(ICefV8Value)));
                        }
                        return obj;
                    }
                    else
                    {
                        var deserialized = objectSerializer.Deserialize(value, typeof(object));
                        if (deserialized is ObjectDescriptor descriptor)
                        {
                            return new ObjectBinder(descriptor, objectSerializer, functionCallPromiseRegistry).BindToNew();
                        }
                    }
                }
            }

            return CefV8Value.CreateNull();
        }
    }
}
