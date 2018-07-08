using System;
using DSerfozo.CefGlue.Contract.Renderer;
using static DSerfozo.CefGlue.Contract.Renderer.CefFactories;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Util
{
    public class Promise : IDisposable
    {
        private readonly ICefV8Value promise;
        private readonly ICefV8Value resolve;
        private readonly ICefV8Value reject;
        private readonly ICefV8Context context;

        public ICefV8Context Context => context;

        public ICefV8Value Object => promise;

        public Promise(ICefV8Value promise, ICefV8Value resolve, ICefV8Value reject, ICefV8Context context)
        {
            this.promise = promise;
            this.resolve = resolve;
            this.reject = reject;
            this.context = context;
        }

        public void Reject(string error)
        {
            using (var cefV8Value = CefV8Value.CreateString(error))
            {
                reject.ExecuteFunction(null, new[] {cefV8Value});
            }
        }

        public void Resolve(ICefV8Value val)
        {
            resolve.ExecuteFunction(null, new[] {val});
        }

        public void Dispose()
        {
            resolve.Dispose();
            reject.Dispose();
            context.Dispose();
        }
    }
}
