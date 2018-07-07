using System;
using DSerfozo.RpcBindings.CefGlue.Common;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Browser
{
    public class MessageClient : CefClient
    {
        public event EventHandler<ProcessMessageReceivedArgs> ProcessMessageReceived;

        protected override bool OnProcessMessageReceived(CefBrowser browser, CefProcessId sourceProcess, CefProcessMessage message)
        {
            var args = new ProcessMessageReceivedArgs(browser, message);

            OnProcessMessageReceived(args);

            return args.Handled;
        }


        protected virtual void OnProcessMessageReceived(ProcessMessageReceivedArgs e)
        {
            ProcessMessageReceived?.Invoke(this, e);
        }
    }
}
