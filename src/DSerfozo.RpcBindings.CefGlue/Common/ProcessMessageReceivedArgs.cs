using System;
using DSerfozo.CefGlue.Contract.Common;

namespace DSerfozo.RpcBindings.CefGlue.Common
{
    public class ProcessMessageReceivedArgs : EventArgs
    {
        public ICefProcessMessage Message { get; }

        public ICefBrowser Browser { get; }

        public bool Handled { get; set; }

        public ProcessMessageReceivedArgs(ICefBrowser browser, ICefProcessMessage message)
        {
            Browser = browser;
            Message = message;
        }
    }
}
