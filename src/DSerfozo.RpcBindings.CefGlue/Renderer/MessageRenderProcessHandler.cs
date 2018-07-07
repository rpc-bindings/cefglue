using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using DSerfozo.RpcBindings.CefGlue.Common;
using DSerfozo.RpcBindings.CefGlue.Renderer.Handlers;
using DSerfozo.RpcBindings.CefGlue.Renderer.Services;
using DSerfozo.RpcBindings.CefGlue.Renderer.Util;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Renderer
{
    public class MessageRenderProcessHandler : CefRenderProcessHandler
    {
        private readonly IDictionary<int, BrowserController> browsers = new Dictionary<int, BrowserController>();
        private readonly ExtensionHandler handler;
        private readonly string bindingExtensionName;

        private readonly PromiseService promiseService;
        //private PromiseService promiseService;

        public event EventHandler<ProcessMessageReceivedArgs> ProcessMessageReceived;

        public MessageRenderProcessHandler(string bindingExtensionName)
        {
            this.bindingExtensionName = bindingExtensionName;

            handler = new ExtensionHandler(browsers);
            var promiseResults = Observable.FromEvent<PromiseResult>(h => handler.PromiseResult += h,
                h => handler.PromiseResult -= h);
            promiseService = new PromiseService(promiseResults);
        }

        protected override bool OnBeforeNavigation(CefBrowser browser, CefFrame frame, CefRequest request, CefNavigationType navigation_type,
            bool isRedirect)
        {
            if (browsers.TryGetValue(browser.Identifier, out var browserController))
            {
                browserController.OnBeforeNavigate(frame, request, navigation_type, isRedirect);
            }

            return false;
        }

        protected override void OnContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            if (browsers.TryGetValue(browser.Identifier, out var browserController))
            {
                browserController.OnContextReleased(frame, context);
            }
        }

        protected override void OnBrowserCreated(CefBrowser browser)
        {
            browsers.Add(browser.Identifier, new BrowserController(browser, promiseService));
        }

        protected override void OnBrowserDestroyed(CefBrowser browser)
        {
            if (browsers.TryGetValue(browser.Identifier, out var browserController))
            {
                browsers.Remove(browser.Identifier);
                browserController.Dispose();
            }
        }

        protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            if (browsers.TryGetValue(browser.Identifier, out var browserController))
            {
                browserController.OnContextCreated(frame, context);
            }
        }

        protected override void OnWebKitInitialized()
        {
            CefRuntime.RegisterExtension(bindingExtensionName,
                EmbeddedResourceReader.Read(typeof(MessageRenderProcessHandler), "/Renderer/Javascript/extension.js"),
                handler);
            base.OnWebKitInitialized();
        }

        protected override bool OnProcessMessageReceived(CefBrowser browser, CefProcessId sourceProcess,
            CefProcessMessage message)
        {
            var handled = false;
            if (browsers.TryGetValue(browser.Identifier, out var browserController))
            {
                handled = browserController.OnProcessMessage(message);
            }

            if (!handled)
            {
                var args = new ProcessMessageReceivedArgs(browser, message);
                ProcessMessageReceived?.Invoke(this, args);
                handled = args.Handled;
            }

            return handled;
        }

        private void HandlerOnInitialized(object sender, EventArgs eventArgs)
        {
            //handler.Initialized -= HandlerOnInitialized;

            //promiseService = new PromiseService(handler.IsPromise, handler.WaitForPromise, handler.PromiseCreator,
            //    Observable.FromEvent<PromiseResult>(action => handler.PromiseResult += action,
            //        action => handler.PromiseResult -= action));
        }
    }
}
