using DSerfozo.RpcBindings.CefGlue.Common.Serialization;
using DSerfozo.RpcBindings.CefGlue.Renderer.Handlers;
using DSerfozo.RpcBindings.CefGlue.Renderer.Util;
using DSerfozo.RpcBindings.Model;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Binding
{
    public class FunctionBinder
    {
        private readonly long objectId;
        private readonly MethodDescriptor descriptor;
        private readonly ObjectSerializer v8Serializer;
        private readonly SavedValueFactory<Promise> functionCallRegistry;

        public FunctionBinder(long objectId, MethodDescriptor descriptor, ObjectSerializer v8Serializer, SavedValueFactory<Promise> functionCallRegistry)
        {
            this.objectId = objectId;
            this.descriptor = descriptor;
            this.v8Serializer = v8Serializer;
            this.functionCallRegistry = functionCallRegistry;
        }

        public void Bind(CefV8Value cefV8Value)
        {
            using (var func = CefV8Value.CreateFunction(descriptor.Name, new FunctionHandler(objectId, descriptor, v8Serializer, functionCallRegistry)))
            {
                cefV8Value.SetValue(descriptor.Name, func);
            }
        }
    }
}
