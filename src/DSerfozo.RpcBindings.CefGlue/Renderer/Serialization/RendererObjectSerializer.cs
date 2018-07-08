using System;
using System.Collections.Generic;
using System.Linq;
using DSerfozo.CefGlue.Contract.Renderer;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Serialization
{
    public class RendererObjectSerializer : ObjectSerializer
    {
        protected override IDisposable HandleSeen(Stack<object> seen, object current, Type currentType, out bool go)
        {
            if (typeof(ICefV8Value).IsAssignableFrom(currentType))
            {
                go = !seen.OfType<ICefV8Value>().Any(v => v.IsSame(current as ICefV8Value));
                if (go)
                {
                    seen.Push(current);
                }

                return new SeenDisposable(seen, go);
            }

            return base.HandleSeen(seen, current, currentType, out go);
        }
    }
}
