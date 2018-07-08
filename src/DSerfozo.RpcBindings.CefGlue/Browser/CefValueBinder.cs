using System.Collections.Generic;
using DSerfozo.CefGlue.Contract.Common;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;
using DSerfozo.RpcBindings.Contract.Marshaling;

namespace DSerfozo.RpcBindings.CefGlue.Browser
{
    public class CefValueBinder : IPlatformBinder<ICefValue>
    {
        private readonly ObjectSerializer objectSerializer;

        public CefValueBinder(ObjectSerializer objectSerializer)
        {
            this.objectSerializer = objectSerializer;
        }

        public ICefValue BindToWire(object obj)
        {
            return objectSerializer.Serialize(obj, new HashSet<object>());
        }

        public object BindToNet(Binding<ICefValue> binding)
        {
            return objectSerializer.Deserialize(binding.Value, binding.TargetType);
        }
    }
}
