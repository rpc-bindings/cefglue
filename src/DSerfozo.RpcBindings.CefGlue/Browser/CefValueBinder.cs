using System.Collections.Generic;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;
using DSerfozo.RpcBindings.Contract.Marshaling;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Browser
{
    public class CefValueBinder : IPlatformBinder<CefValue>
    {
        private readonly ObjectSerializer objectSerializer;

        public CefValueBinder(ObjectSerializer objectSerializer)
        {
            this.objectSerializer = objectSerializer;
        }

        public CefValue BindToWire(object obj)
        {
            return objectSerializer.Serialize(obj, new HashSet<object>());
        }

        public object BindToNet(Binding<CefValue> binding)
        {
            return objectSerializer.Deserialize(binding.Value, binding.TargetType);
        }
    }
}
