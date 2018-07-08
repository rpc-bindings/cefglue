using System;
using System.Collections.Generic;
using System.Linq;
using DSerfozo.CefGlue.Contract.Renderer;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Serialization
{
    public class RendererObjectSerializer : ObjectSerializer
    {
        protected override bool HandleSeen(HashSet<object> seen, object current, Type currentType)
        {
            if (typeof(ICefV8Value).IsAssignableFrom(currentType))
            {
                var result = !seen.OfType<ICefV8Value>().Any(v => v.IsSame(current as ICefV8Value));
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
