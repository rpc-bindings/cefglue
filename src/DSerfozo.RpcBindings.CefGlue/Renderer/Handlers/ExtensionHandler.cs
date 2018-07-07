using System;
using System.Collections.Generic;
using System.Linq;
using DSerfozo.RpcBindings.CefGlue.Renderer.Services;
using DSerfozo.RpcBindings.CefGlue.Renderer.Util;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Handlers
{
    public class ExtensionHandler : CefV8Handler
    {
        private readonly List<Tuple<CefV8Context, CefV8Value, CefV8Value, CefV8Value>> promiseFunc =
            new List<Tuple<CefV8Context, CefV8Value, CefV8Value, CefV8Value>>();
        private readonly IDictionary<int, BrowserController> browserControllers;

        public event Action<PromiseResult> PromiseResult;

        public Tuple<CefV8Value, CefV8Value, CefV8Value> TryGetPromiseFunc(CefV8Context ctx)
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

        protected override bool Execute(string name, CefV8Value obj, CefV8Value[] arguments, out CefV8Value returnValue, out string exception)
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
