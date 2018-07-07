using System;
using System.Collections.Generic;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;
using DSerfozo.RpcBindings.CefGlue.Renderer.Services;
using DSerfozo.RpcBindings.CefGlue.Renderer.Util;
using DSerfozo.RpcBindings.Contract.Marshaling.Model;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Serialization
{
    public class V8Serializer : ITypeSerializer
    {
        private readonly PromiseService promiseService;
        private readonly SavedValueRegistry<Callback> callbackRegistry;

        public V8Serializer(PromiseService promiseService, SavedValueRegistry<Callback> callbackRegistry)
        {
            this.promiseService = promiseService;
            this.callbackRegistry = callbackRegistry;
        }

        public bool CanHandle(Type type)
        {
            return type == typeof(CefV8Value);
        }

        public CefValue Serialize(object source, HashSet<object> seen, ObjectSerializer objectSerializer)
        {
            if (!CanHandle(source?.GetType()))
            {
                throw new InvalidOperationException();
            }

            long frameId = 0;
            using (var context = CefV8Context.GetCurrentContext())
            {
                frameId = context.GetFrame().Identifier;
            }
            var result = CefValue.Create();
            result.SetNull();

            var value = (CefV8Value)source;
            if (value.IsString)
            {
                result.SetString(value.GetStringValue());
            }
            else if (value.IsBool)
            {
                result.SetBool(value.GetBoolValue());
            }
            else if (value.IsDouble)
            {
                result.SetDouble(value.GetDoubleValue());
            }
            else if (value.IsInt)
            {
                result.SetInt(value.GetIntValue());
            }
            else if (value.IsUInt)
            {
                result.SetDouble(value.GetUIntValue());
            }
            else if (value.IsDate)
            {
                result.SetTime(value.GetDateValue());
            }
            else if (value.IsArray)
            {
                using (var list = CefListValue.Create())
                {
                    list.SetSize(value.GetArrayLength());
                    for (var i = 0; i < value.GetArrayLength(); i++)
                    {
                        list.SetValue(i, objectSerializer.Serialize(value.GetValue(i), seen));
                    }
                    result.SetList(list);
                }
            }
            else if (value.IsFunction)
            {
                var callback = new Callback(value, promiseService, objectSerializer);
                var id = callbackRegistry.Save(frameId, callback);

                using (var list = CefDictionaryValue.Create())
                using (var actualValue = CefDictionaryValue.Create())
                {
                    list.SetString(ObjectSerializer.TypeIdPropertyName, CallbackDescriptor.TypeId);

                    actualValue.SetInt64(nameof(CallbackDescriptor.FunctionId), id);

                    list.SetDictionary(ObjectSerializer.ValuePropertyName, actualValue);
                    result.SetDictionary(list);
                }
            }
            else if (value.IsObject)
            {
                using (var dict = CefDictionaryValue.Create())
                using (var actualValue = CefDictionaryValue.Create())
                {
                    dict.SetString(ObjectSerializer.TypeIdPropertyName, ObjectSerializer.DictionaryTypeId);
                    if (value.TryGetKeys(out var keys))
                    {
                        foreach (var key in keys)
                        {
                            actualValue.SetValue(key, objectSerializer.Serialize(value.GetValue(key), seen));
                        }
                    }

                    dict.SetDictionary(ObjectSerializer.ValuePropertyName, actualValue);
                    result.SetDictionary(dict);
                }
            }


            return result;
        }
    }
}
