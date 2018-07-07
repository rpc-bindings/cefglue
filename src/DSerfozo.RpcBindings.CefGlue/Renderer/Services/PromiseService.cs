using System;
using DSerfozo.RpcBindings.CefGlue.Renderer.Util;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Services
{
    public class PromiseService
    {
        private class PendingPromise : IDisposable
        {
            public Action<PromiseResult> Continuation { get; set; }

            public void Dispose()
            {
            }
        }

        public const string HelperObjectName = "997433B3-9479-456C-B479-202DFB8C9BB6";
        private readonly SavedValueRegistry<PendingPromise> pendingPromises = new SavedValueRegistry<PendingPromise>(); 

        public PromiseService(IObservable<PromiseResult> promiseResult)
        {
            promiseResult.Subscribe(OnNextPromiseResult);
        }

        public Promise CreatePromise()
        {
            using(var context = CefV8Context.GetCurrentContext())
            using (var global = context.GetGlobal())
                using(var s = global.GetValue(HelperObjectName))
            {
                var userData = s.GetUserData() as PromiseUserData;
                var promiseCreator = userData.PromiseCreator;
                using (var promiseData = promiseCreator.ExecuteFunction(null, new CefV8Value[] { }))
                {
                    if (promiseData == null && promiseCreator.HasException)
                    {
                        var exc = promiseCreator.GetException();
                        var msg = exc.Message;
                        Console.WriteLine(msg);
                        promiseCreator.ClearException();
                    }

                    return new Promise(promiseData.GetValue("promise"), promiseData.GetValue("resolve"),
                        promiseData.GetValue("reject"),
                        CefV8Context.GetCurrentContext());
                }
            }
        }

        public bool IsPromise(CefV8Value v8Value, CefV8Context context)
        {
            using (new ContextHelper(context))
            using (var global = context.GetGlobal())
            using (var s = global.GetValue(HelperObjectName))
            {
                var userData = s.GetUserData() as PromiseUserData;
                var isPromise = userData.IsPromise;
                using (var result = isPromise.ExecuteFunction(null, new[] { v8Value }))
                {
                    return result.GetBoolValue();
                }
            }
        }

        public void Then(CefV8Value promise, CefV8Context context, Action<PromiseResult> continuation)
        {
            var id = pendingPromises.Save(context.GetFrame().Identifier, new PendingPromise
            {
                Continuation = continuation
            });

            using(new ContextHelper(context))
            using (var global = context.GetGlobal())
            using (var s = global.GetValue(HelperObjectName))
            {
                var userData = s.GetUserData() as PromiseUserData;
                var waitForPromise = userData.WaitForPromise;
                using (var idValue = CefV8Value.CreateString(id.ToString()))
                {
                    waitForPromise.ExecuteFunctionWithContext(context, null, new[] { promise, idValue }).Dispose();
                }
            }
        }

        private void OnNextPromiseResult(PromiseResult promiseResult)
        {
            var id = Int64.Parse(promiseResult.Id);
            if (pendingPromises.Has(id))
            {
                var pendingPromise = pendingPromises.Get(id);
                pendingPromise.Continuation(promiseResult);
            }
        }
    }
}
