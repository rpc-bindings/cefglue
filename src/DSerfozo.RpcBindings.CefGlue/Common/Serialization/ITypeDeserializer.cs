using System;
using DSerfozo.CefGlue.Contract.Common;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public interface ITypeDeserializer
    {
        bool CanHandle(ICefValue cefValue, Type targetType);

        object Deserialize(ICefValue value, Type targetType, ObjectSerializer objectSerializer);
    }
}
