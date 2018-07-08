using System.Reactive.Concurrency;
using DSerfozo.CefGlue.Contract.Common;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;

namespace DSerfozo.RpcBindings.CefGlue.Browser
{
    public class CefGlueRpcBindingHost : RpcBindingHost<ICefValue>
    {
        public CefGlueRpcBindingHost(Connection connection) : base(connection,
            new CefValueBinder(new ObjectSerializer()), new EventLoopScheduler())
        {
        }
    }
}
