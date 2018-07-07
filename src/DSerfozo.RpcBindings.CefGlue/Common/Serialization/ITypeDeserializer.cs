using System;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public interface ITypeDeserializer
    {
        bool CanHandle(CefValue cefValue, Type targetType);

        object Deserialize(CefValue value, Type targetType, ObjectSerializer objectSerializer);
    }
}
