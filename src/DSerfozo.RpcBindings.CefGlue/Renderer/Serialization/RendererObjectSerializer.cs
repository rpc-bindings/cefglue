using System;
using System.Collections.Generic;
using System.Linq;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Serialization
{
    public class RendererObjectSerializer : ObjectSerializer
    {
        protected override bool HandleSeen(HashSet<object> seen, object current, Type currentType)
        {
            if (currentType == typeof(CefV8Value))
            {
                var result = !seen.OfType<CefV8Value>().Any(v => v.IsSame(current as CefV8Value));
                if (result)
                {
                    seen.Add(current);
                }

                return result;
            }

            return base.HandleSeen(seen, current, currentType);
        }
    }
}
