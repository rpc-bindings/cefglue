using System;
using System.Collections.Generic;
using System.Linq;
using DSerfozo.CefGlue.Contract.Renderer;
using DSerfozo.RpcBindings.CefGlue.Renderer.Services;
using DSerfozo.RpcBindings.CefGlue.Renderer.Util;
using static DSerfozo.CefGlue.Contract.Renderer.CefFactories;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Handlers
{
    public class ExtensionHandler : ICefV8Handler
    {
        private readonly List<Tuple<ICefV8Context, ICefV8Value, ICefV8Value, ICefV8Value>> promiseFunc =
            new List<Tuple<ICefV8Context, ICefV8Value, ICefV8Value, ICefV8Value>>();
        private readonly IDictionary<int, BrowserController> browserControllers;

        public event Action<PromiseResult> PromiseResult;

        public Tuple<ICefV8Value, ICefV8Value, ICefV8Value> TryGetPromiseFunc(ICefV8Context ctx)
        {
            var res = promiseFunc.FirstOrDefault(f => f.Item1.IsSame(ctx));
            if (res != null)
            {
                promiseFunc.Remove(res);
                res.Item1.Dispose();
            }
            return Tuple.Create(res.Item2, res.Item3, res.Item4);
        }

        public ExtensionHandler(IDictionary<int, BrowserController> browserControllers)
        {
            this.browserControllers = browserControllers;
        }

        public bool Execute(string name, ICefV8Value obj, ICefV8Value[] arguments, out ICefV8Value returnValue, out string exception)
        {
            returnValue = CefV8Value.CreateNull();
            exception = null;
            var result = false;

            if (name == "bindingRequire")
            {
                var browser = CefV8Context.GetCurrentContext().GetBrowser();

                if (browserControllers.TryGetValue(browser.Identifier, out var browserController))
                {
                    returnValue = browserController.OnBindingRequire(arguments[0]);
                }

                result = true;
            }
            else if (name == "setPromiseInteractions")
            {
                var context = CefV8Context.GetCurrentContext();
                var global = context.GetGlobal();
                var promiseDataStorage = CefV8Value.CreateObject();
                global.SetValue(PromiseService.HelperObjectName, promiseDataStorage,
                    CefV8PropertyAttribute.DontDelete | CefV8PropertyAttribute.DontEnum |
                    CefV8PropertyAttribute.ReadOnly);

                promiseDataStorage.SetUserData(new PromiseUserData(arguments[0], arguments[1], arguments[2]));

                result = true;
            }
            else if (name == "promiseDone")
            {
                using (var id = arguments[0])
                using (var success = arguments[1])
                using (var error = arguments[3])
                {
                    var res = arguments[2];
                    PromiseResult?.Invoke(new PromiseResult
                    {
                        Id = id.GetStringValue(),
                        Success = success.GetBoolValue(),
                        Error = error.IsString ? error.GetStringValue() : null,
                        Result = res,
                        Context = CefV8Context.GetCurrentContext()
                    });
                }

                result = true;
            }

            return result;
        }
    }
}
