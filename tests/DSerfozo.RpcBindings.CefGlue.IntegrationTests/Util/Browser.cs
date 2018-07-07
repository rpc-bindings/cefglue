using System;
using System.Threading.Tasks;
using DSerfozo.RpcBindings.CefGlue.Browser;
using DSerfozo.RpcBindings.CefGlue.Common;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;
using DSerfozo.RpcBindings.Contract;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.IntegrationTests.Util
{
    public class Browser : IDisposable
    {
        public class Render : CefRenderHandler
        {
            protected override CefAccessibilityHandler GetAccessibilityHandler()
            {
                return null;
            }

            protected override bool GetRootScreenRect(CefBrowser browser, ref CefRectangle rect)
            {
                rect = new CefRectangle(0, 0, 300, 300);

                return true;
            }

            protected override bool GetViewRect(CefBrowser browser, ref CefRectangle rect)
            {
                rect = new CefRectangle(0, 0, 300, 300);

                return true;
            }

            protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo)
            {
                return false;
            }

            protected override void OnPopupSize(CefBrowser browser, CefRectangle rect)
            {
            }

            protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width,
                int height)
            {
            }

            protected override void OnCursorChange(CefBrowser browser, IntPtr cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo)
            {
            }

            protected override void OnScrollOffsetChanged(CefBrowser browser, double x, double y)
            {
            }

            protected override void OnImeCompositionRangeChanged(CefBrowser browser, CefRange selectedRange, CefRectangle[] characterBounds)
            {
            }
        }

        public class LoadHandler : CefLoadHandler
        {
            public event EventHandler Loaded;

            protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode)
            {
                base.OnLoadEnd(browser, frame, httpStatusCode);

                if (frame.IsMain)
                {
                    Loaded?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public class Request : CefRequestHandler
        {
            public event Action Crash;

            protected override void OnRenderProcessTerminated(CefBrowser browser, CefTerminationStatus status)
            {
                base.OnRenderProcessTerminated(browser, status);

                Crash?.Invoke();
            }
        }

        public class DialogHandler : CefJSDialogHandler
        {
            protected override bool OnJSDialog(CefBrowser browser, string originUrl, CefJSDialogType dialogType, string message_text,
                string default_prompt_text, CefJSDialogCallback callback, out bool suppress_message)
            {
                suppress_message = false;
                return false;
            }

            protected override bool OnBeforeUnloadDialog(CefBrowser browser, string messageText, bool isReload, CefJSDialogCallback callback)
            {
                return false;
            }

            protected override void OnResetDialogState(CefBrowser browser)
            {
            }

            protected override void OnDialogClosed(CefBrowser browser)
            {
            }
        }

        public class LifecycleHandler : CefLifeSpanHandler
        {
            private readonly TaskCompletionSource<CefBrowser> taskCompletionSource;

            public event Action<Browser> NewBrowser;

            public LifecycleHandler(TaskCompletionSource<CefBrowser> taskCompletionSource)
            {
                this.taskCompletionSource = taskCompletionSource;
            }

            protected override void OnAfterCreated(CefBrowser browser)
            {
                base.OnAfterCreated(browser);
                taskCompletionSource.TrySetResult(browser);
            }

            protected override bool OnBeforePopup(CefBrowser browser, CefFrame frame, string targetUrl, string targetFrameName,
                CefWindowOpenDisposition targetDisposition, bool userGesture, CefPopupFeatures popupFeatures,
                CefWindowInfo windowInfo, ref CefClient client, CefBrowserSettings settings, ref bool noJavascriptAccess)
            {
                //settings.WebSecurity = CefState.Disabled;
                windowInfo.SetAsWindowless(IntPtr.Zero, false);
                var newBrowser = new Browser(false);

                client = newBrowser.CefClient;

                WaitForBrowser(newBrowser);

                return false;
            }

            private async void WaitForBrowser(Browser browser)
            {
                await browser.browser;

                NewBrowser?.Invoke(browser);
            }
        }

        public class Client : MessageClient
        {
            private readonly LoadHandler loadHandler = new LoadHandler();
            private readonly DialogHandler dialogHandler = new DialogHandler();
            private readonly LifecycleHandler lifeSpanHandler;
            private readonly Request requestHandler = new Request();
            private readonly Render render = new Render();

            public Client(TaskCompletionSource<CefBrowser> taskCompletionSource)
            {
                lifeSpanHandler = new LifecycleHandler(taskCompletionSource);
            }

            public LoadHandler LoadHandler => loadHandler;

            public Request RequestHandler => requestHandler;

            public LifecycleHandler LifecycleHandler => lifeSpanHandler;

            protected override CefLifeSpanHandler GetLifeSpanHandler()
            {
                return lifeSpanHandler;
            }

            protected override CefLoadHandler GetLoadHandler()
            {
                return loadHandler;
            }

            protected override CefJSDialogHandler GetJSDialogHandler()
            {
                return dialogHandler;
            }

            protected override CefRenderHandler GetRenderHandler()
            {
                return render;
            }

            protected override CefRequestHandler GetRequestHandler()
            {
                return requestHandler;
            }
        }

        private readonly TaskCompletionSource<CefBrowser> browserTcs = new TaskCompletionSource<CefBrowser>();
        private readonly Task<CefBrowser> browser;
        private readonly Client client;
        private readonly CefGlueRpcBindingHost rpcBindingHost;
        private readonly ITypeSerializer objectDescriptorSerializer;

        public Client CefClient => client;

        public RpcBindingHost<CefValue> BindingHost => rpcBindingHost;

        public IBindingRepository Repository => rpcBindingHost.Repository;

        public event Action<Browser> NewBrowser
        {
            add => client.LifecycleHandler.NewBrowser += value;
            remove => client.LifecycleHandler.NewBrowser -= value;
        }

        public Browser(bool createBrowser = true)
        {
            client = new Client(browserTcs);
            browser = browserTcs.Task;
            var windowInfo = CefWindowInfo.Create();
            windowInfo.WindowlessRenderingEnabled = true;
            windowInfo.SetAsWindowless(IntPtr.Zero, false);
            var cefBrowserSettings = new CefBrowserSettings();
            cefBrowserSettings.WebSecurity = CefState.Disabled;
            if (createBrowser)
            {
                CefBrowserHost.CreateBrowser(windowInfo, client, cefBrowserSettings);
            }

            rpcBindingHost = new CefGlueRpcBindingHost(new Connection(new ObjectSerializer()));

            InitConnection();
        }

        private async void InitConnection()
        {
            var actualBrowser = await browser.ConfigureAwait(false);
            
            (rpcBindingHost.Connection as Connection).Initialize(actualBrowser, CefClient);
        }

        public async Task LoadAsync(string url)
        {
            var tcs = new TaskCompletionSource<object>();

            void Handler(object sender, EventArgs args)
            {
                client.LoadHandler.Loaded -= Handler;
                tcs.TrySetResult(null);
            }

            client.LoadHandler.Loaded += Handler;
            (await browser.ConfigureAwait(false)).GetMainFrame().LoadUrl(url);
            await tcs.Task.ConfigureAwait(false);
        }

        public async Task Reload()
        {
            var tcs = new TaskCompletionSource<object>();

            void Handler(object sender, EventArgs args)
            {
                client.LoadHandler.Loaded -= Handler;
                tcs.TrySetResult(null);
            }

            client.LoadHandler.Loaded += Handler;

            (await browser.ConfigureAwait(false)).Reload();
            await tcs.Task.ConfigureAwait(false);
        }

        public async Task RunTest(string name)
        {
            var tcs = new TaskCompletionSource<object>();

            var actualBrowser = await browser.ConfigureAwait(false);

            void Message(object sender, ProcessMessageReceivedArgs args)
            {
                if (args.Message.Name == "TestDone")
                {
                    client.ProcessMessageReceived -= Message;

                    var success = args.Message.Arguments.GetBool(0);
                    if (!success)
                    {
                        var msg = args.Message.Arguments.GetString(1);
                        tcs.TrySetException(new Exception(msg));
                    }
                    else
                    {
                        tcs.TrySetResult(null);
                    }

                    args.Handled = true;
                }
            }

            void Crash()
            {
                client.RequestHandler.Crash -= Crash;
                tcs.TrySetException(new Exception("Subprocess crash."));
            }

            client.RequestHandler.Crash += Crash;
            client.ProcessMessageReceived += Message;
            actualBrowser.GetMainFrame().ExecuteJavaScript($"test.run({name});", null, 0);

            await tcs.Task.ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (browser.IsCompleted)
            {
                browser.Result.GetHost().CloseBrowser(true);
                browser.Result.Dispose();
            }
        }
    }
}
