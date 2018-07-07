using System;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Util
{
    public class Promise : IDisposable
    {
        private readonly CefV8Value promise;
        private readonly CefV8Value resolve;
        private readonly CefV8Value reject;
        private readonly CefV8Context context;

        public CefV8Context Context => context;

        public CefV8Value Object => promise;

        public Promise(CefV8Value promise, CefV8Value resolve, CefV8Value reject, CefV8Context context)
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

        public void Resolve(CefV8Value val)
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
