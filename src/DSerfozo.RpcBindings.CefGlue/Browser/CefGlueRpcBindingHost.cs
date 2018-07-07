using System.Reactive.Concurrency;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Browser
{
    public class CefGlueRpcBindingHost : RpcBindingHost<CefValue>
    {
        public CefGlueRpcBindingHost(Connection connection) : base(connection,
            new CefValueBinder(new ObjectSerializer()), new EventLoopScheduler())
        {
        }
    }
}
