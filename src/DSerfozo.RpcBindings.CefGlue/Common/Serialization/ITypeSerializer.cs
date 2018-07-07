using System;
using System.Collections.Generic;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public interface ITypeSerializer
    {
        bool CanHandle(Type type);

        CefValue Serialize(object source, HashSet<object> seen, ObjectSerializer objectSerializer);
    }
}
