using System;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Common
{
    public class ProcessMessageReceivedArgs : EventArgs
    {
        public CefProcessMessage Message { get; }

        public CefBrowser Browser { get; }

        public bool Handled { get; set; }

        public ProcessMessageReceivedArgs(CefBrowser browser, CefProcessMessage message)
        {
            Browser = browser;
            Message = message;
        }
    }
}
