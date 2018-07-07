using System;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Util
{
    public class ContextHelper : IDisposable
    {
        private readonly CefV8Context context;
        private readonly bool entered;

        public ContextHelper(CefV8Context context)
        {
            this.context = context;
            entered = context.Enter();
        }

        public void Dispose()
        {
            if (entered)
            {
                context.Exit();
            }
        }
    }
}
