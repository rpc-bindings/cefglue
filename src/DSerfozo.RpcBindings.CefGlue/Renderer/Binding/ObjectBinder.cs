using System.Collections.Generic;
using System.Linq;
using DSerfozo.CefGlue.Contract.Renderer;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;
using DSerfozo.RpcBindings.CefGlue.Renderer.Model;
using DSerfozo.RpcBindings.CefGlue.Renderer.Util;
using DSerfozo.RpcBindings.Model;
using static DSerfozo.CefGlue.Contract.Renderer.CefFactories;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Binding
{
    public class ObjectBinder
    {
        private readonly ObjectSerializer v8Serializer;
        private readonly IDictionary<long, FunctionBinder> functions;
        private readonly List<CefPropertyDescriptor> propertyDescriptors;

        public ObjectBinder(ObjectDescriptor descriptor, ObjectSerializer serializer, SavedValueFactory<Promise> functionCallRegistry)
        {
            v8Serializer = serializer;
            functions = descriptor.Methods?.Select(m => new {m.Key, Value = new FunctionBinder(descriptor.Id, m.Value, serializer, functionCallRegistry)})
                .ToDictionary(k => k.Key, v => v.Value);

            propertyDescriptors = descriptor.Properties?.Select(p => p.Value).OfType<CefPropertyDescriptor>().ToList();
        }

        public ICefV8Value BindToNew()
        {
            var obj = CefV8Value.CreateObject();

            functions?.Values.ToList().ForEach(m => m.Bind(obj));

            propertyDescriptors?.ForEach(c =>
            {
                var value = (ICefV8Value) v8Serializer.Deserialize(c.ListValue, typeof(ICefV8Value));
                obj.SetValue(c.Name, value, CefV8PropertyAttribute.ReadOnly);
                c.ListValue.Dispose();
            });
            propertyDescriptors?.Clear();

            return obj;
        }
    }
}
