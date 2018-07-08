using System;
using DSerfozo.CefGlue.Contract.Browser;
using DSerfozo.CefGlue.Contract.Common;
using DSerfozo.RpcBindings.CefGlue.Common;

namespace DSerfozo.RpcBindings.CefGlue.Browser
{
    public class MessageClient : ICefClient
    {
        public event EventHandler<ProcessMessageReceivedArgs> ProcessMessageReceived;

        bool ICefClient.OnProcessMessageReceived(ICefBrowser browser, CefProcessId sourceProcess, ICefProcessMessage message)
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
