using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
using DSerfozo.CefGlue.Contract.Common;
using DSerfozo.RpcBindings.CefGlue.Common;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;
using DSerfozo.RpcBindings.Contract.Communication;
using DSerfozo.RpcBindings.Contract.Communication.Model;
using static DSerfozo.CefGlue.Contract.Common.CefFactories;

namespace DSerfozo.RpcBindings.CefGlue.Browser
{
    public class Connection : IConnection<ICefValue>
    {
        private readonly ISubject<RpcResponse<ICefValue>> rpcResponseSubject = new Subject<RpcResponse<ICefValue>>();
        private readonly ObjectSerializer objectSerializer;
        private ICefBrowser browser;
        private MessageClient client;
        private volatile int browserDisposed;

        public bool IsOpen => browser != null && browserDisposed != 1;

        public Connection(ObjectSerializer objectSerializer)
        {
            this.objectSerializer = objectSerializer;
        }

        public void Initialize(ICefBrowser browser, MessageClient client)
        {
            this.browser = browser;
            this.client = client;

            client.ProcessMessageReceived += ClientOnProcessMessageReceived;
        }

        private void ClientOnProcessMessageReceived(object sender, ProcessMessageReceivedArgs e)
        {
            var message = e.Message;

            if (message.Name == Messages.RpcResponseMessage)
            {
                var response = message.Arguments.GetValue(0);

                var rpcResponse =
                    (RpcResponse<ICefValue>) objectSerializer.Deserialize(response, typeof(RpcResponse<ICefValue>));

                if (rpcResponse != null)
                {
                    e.Handled = true;
                    rpcResponseSubject.OnNext(rpcResponse);
                }
            }
        }

        public void Send(RpcRequest<ICefValue> rpcRequest)
        {
            if (browserDisposed == 1)
                return;

            var message = CefProcessMessage.Create(Messages.RpcRequestMessage);
            try
            {
                message.Arguments.SetValue(0, objectSerializer.Serialize(rpcRequest));

                browser.SendProcessMessage(CefProcessId.Renderer, message);
            }
            catch (NullReferenceException)
            {
                //when the browser is already disposed
                //this can happen from a finalized callback for example
            }
            finally
            {
                message.Dispose();
            }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref browserDisposed, 1, 0) == 0)
            {
                client.ProcessMessageReceived -= ClientOnProcessMessageReceived;
            }
        }

        public IDisposable Subscribe(IObserver<RpcResponse<ICefValue>> observer)
        {
            return rpcResponseSubject.Subscribe(observer);
        }
    }
}
