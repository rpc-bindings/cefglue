using System;
using System.Linq;
using DSerfozo.CefGlue.Contract.Common;
using DSerfozo.CefGlue.Contract.Renderer;
using DSerfozo.RpcBindings.CefGlue.Common;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;
using DSerfozo.RpcBindings.CefGlue.Renderer.Services;
using DSerfozo.RpcBindings.Contract.Communication.Model;
using DSerfozo.RpcBindings.Execution.Model;
using static DSerfozo.CefGlue.Contract.Common.CefFactories;
using static DSerfozo.CefGlue.Contract.Renderer.CefFactories;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Util
{
    public class Callback : IDisposable
    {
        private readonly ICefV8Value function;
        private readonly PromiseService promiseService;
        private readonly ObjectSerializer v8Serializer;
        private readonly ICefV8Context context;

        public ICefV8Context Context => context;

        public Callback(ICefV8Value function,  PromiseService promiseService, ObjectSerializer v8Serializer)
        {
            this.function = function;
            this.promiseService = promiseService;
            this.v8Serializer = v8Serializer;

            context = CefV8Context.GetCurrentContext();
        }

        public void Execute(CallbackExecution<ICefValue> execution)
        {
            ICefV8Exception exception = null;
            using (new ContextHelper(context))
            {
                var cefV8Values = execution.Parameters.Select(s => (ICefV8Value)v8Serializer.Deserialize(s, typeof(ICefV8Value))).ToArray();
                var result = function.ExecuteFunction(null, cefV8Values);


                var browser = context.GetBrowser();

                if (result == null && function.HasException)
                {
                    exception = function.GetException();
                    function.ClearException();
                }

                if (promiseService.IsPromise(result, context))
                {
                    promiseService.Then(result, context, a => CallbackDone(a, browser, execution.ExecutionId));
                }
                else
                {
                    CallbackDone(new PromiseResult
                    {
                        Success = result != null,
                        Result = result,
                        Error = exception?.Message
                    }, browser, execution.ExecutionId);
                }
            }
        }

        private void CallbackDone(PromiseResult promiseResult, ICefBrowser browser, long executionId)
        {
            var callbackResult = new CallbackResult<ICefValue>
            {
                ExecutionId = executionId,
                Success = promiseResult.Success
            };

            if (promiseResult.Success)
            {
                callbackResult.Result = v8Serializer.Serialize(promiseResult.Result);
            }
            else if(!string.IsNullOrWhiteSpace(promiseResult.Error))
            {
                callbackResult.Error = promiseResult.Error;
            }

            var response = new RpcResponse<ICefValue>
            {
                CallbackResult = callbackResult
            };
            var msg = CefProcessMessage.Create(Messages.RpcResponseMessage);
            var serialized = v8Serializer.Serialize(response);
            msg.Arguments.SetValue(0, serialized);

            browser.SendProcessMessage(CefProcessId.Browser, msg);
        }

        public void Dispose()
        {
            function.Dispose();
            context.Dispose();
        }
    }
}
