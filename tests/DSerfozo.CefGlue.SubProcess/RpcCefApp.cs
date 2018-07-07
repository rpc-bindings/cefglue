using DSerfozo.RpcBindings.CefGlue.Common;
using DSerfozo.RpcBindings.CefGlue.Renderer;
using Xilium.CefGlue;

namespace DSerfozo.CefGlue.SubProcess
{
    internal class TestHandler : CefV8Handler
    {
        protected override bool Execute(string name, CefV8Value obj, CefV8Value[] arguments, out CefV8Value returnValue, out string exception)
        {
            var message = CefProcessMessage.Create("TestDone");

            if (name == "success")
            {
                message.Arguments.SetBool(0, true);
            }
            else if (name == "fail")
            {
                message.Arguments.SetBool(0, false);
                if (arguments.Length == 1 && arguments[0].IsString)
                {
                    message.Arguments.SetString(1, arguments[0].GetStringValue());
                }
                else
                {
                    message.Arguments.SetString(1, "");
                }
            }
            CefV8Context.GetCurrentContext().GetBrowser().SendProcessMessage(CefProcessId.Browser, message);

            returnValue = null;
            exception = null;
            return true;
        }
    }

    internal class Handler : MessageRenderProcessHandler
    {
        public Handler(string bindingExtensionName) : base(bindingExtensionName)
        {
        }

        protected override void OnWebKitInitialized()
        {
            base.OnWebKitInitialized();
            CefRuntime.RegisterExtension("test",
                EmbeddedResourceReader.Read(typeof(Handler), "/Javascript/test_extension.js"), new TestHandler());
        }
    }

    internal class RpcCefApp : CefApp
    {
        private readonly CefRenderProcessHandler handler = new Handler("bindingExtension");

        protected override CefRenderProcessHandler GetRenderProcessHandler()
        {
            return handler;
        }
    }
}
