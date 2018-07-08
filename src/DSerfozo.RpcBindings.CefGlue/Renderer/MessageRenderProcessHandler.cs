using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using DSerfozo.CefGlue.Contract.Common;
using DSerfozo.CefGlue.Contract.Renderer;
using DSerfozo.RpcBindings.CefGlue.Common;
using DSerfozo.RpcBindings.CefGlue.Renderer.Handlers;
using DSerfozo.RpcBindings.CefGlue.Renderer.Services;
using DSerfozo.RpcBindings.CefGlue.Renderer.Util;
using static DSerfozo.CefGlue.Contract.Renderer.CefGlobals;

namespace DSerfozo.RpcBindings.CefGlue.Renderer
{
    public class MessageRenderProcessHandler : ICefRenderProcessHandler
    {
        private readonly IDictionary<int, BrowserController> browsers = new Dictionary<int, BrowserController>();
        private readonly ExtensionHandler handler;
        private readonly string bindingExtensionName;

        private readonly PromiseService promiseService;

        public event EventHandler<ProcessMessageReceivedArgs> ProcessMessageReceived;

        public MessageRenderProcessHandler(string bindingExtensionName)
        {
            this.bindingExtensionName = bindingExtensionName;

            handler = new ExtensionHandler(browsers);
            var promiseResults = Observable.FromEvent<PromiseResult>(h => handler.PromiseResult += h,
                h => handler.PromiseResult -= h);
            promiseService = new PromiseService(promiseResults);
        }

        public bool OnBeforeNavigation(ICefBrowser browser, ICefFrame frame, ICefRequest request, CefNavigationType navigation_type,
            bool isRedirect)
        {
            if (browsers.TryGetValue(browser.Identifier, out var browserController))
            {
                browserController.OnBeforeNavigate(frame, request, navigation_type, isRedirect);
            }

            return false;
        }

        public void OnContextReleased(ICefBrowser browser, ICefFrame frame, ICefV8Context context)
        {
            if (browsers.TryGetValue(browser.Identifier, out var browserController))
            {
                browserController.OnContextReleased(frame, context);
            }
        }

        public void OnBrowserCreated(ICefBrowser browser)
        {
            browsers.Add(browser.Identifier, new BrowserController(browser, promiseService));
        }

        public void OnBrowserDestroyed(ICefBrowser browser)
        {
            if (browsers.TryGetValue(browser.Identifier, out var browserController))
            {
                browsers.Remove(browser.Identifier);
                browserController.Dispose();
            }
        }

        public void OnContextCreated(ICefBrowser browser, ICefFrame frame, ICefV8Context context)
        {
            if (browsers.TryGetValue(browser.Identifier, out var browserController))
            {
                browserController.OnContextCreated(frame, context);
            }
        }

        public void OnWebKitInitialized()
        {
            CefRuntime.RegisterExtension(bindingExtensionName,
                EmbeddedResourceReader.Read(typeof(MessageRenderProcessHandler), "/Renderer/Javascript/extension.js"),
                handler);
            //base.OnWebKitInitialized();
        }

        public bool OnProcessMessageReceived(ICefBrowser browser, CefProcessId sourceProcess,
            ICefProcessMessage message)
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
