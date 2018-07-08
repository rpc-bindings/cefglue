using System;
using System.Collections.Generic;
using DSerfozo.CefGlue.Contract.Common;

namespace DSerfozo.RpcBindings.CefGlue.Common.Serialization
{
    public interface ITypeSerializer
    {
        bool CanHandle(Type type);

        ICefValue Serialize(object source, Stack<object> seen, ObjectSerializer objectSerializer);
    }
}
