using System;
using DSerfozo.CefGlue.Contract.Renderer;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Util
{
    public class ContextHelper : IDisposable
    {
        private readonly ICefV8Context context;
        private readonly bool entered;

        public ContextHelper(ICefV8Context context)
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
