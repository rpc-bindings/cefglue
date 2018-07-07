using System;
using System.Collections.Generic;
using System.Linq;
using DSerfozo.RpcBindings.CefGlue.Common;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;
using DSerfozo.RpcBindings.CefGlue.Renderer.Services;
using DSerfozo.RpcBindings.Contract.Communication.Model;
using DSerfozo.RpcBindings.Execution.Model;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Util
{
    public class Callback : IDisposable
    {
        private readonly CefV8Value function;
        private readonly PromiseService promiseService;
        private readonly ObjectSerializer v8Serializer;
        private readonly CefV8Context context;

        public CefV8Context Context => context;

        public Callback(CefV8Value function,  PromiseService promiseService, ObjectSerializer v8Serializer)
        {
            this.function = function;
            this.promiseService = promiseService;
            this.v8Serializer = v8Serializer;

            context = CefV8Context.GetCurrentContext();
        }

        public void Execute(CallbackExecution<CefValue> execution)
        {
            CefV8Exception exception = null;
            using (new ContextHelper(context))
            {
                var cefV8Values = execution.Parameters.Select(s => (CefV8Value)v8Serializer.Deserialize(s, typeof(CefV8Value))).ToArray();
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

        private void CallbackDone(PromiseResult promiseResult, CefBrowser browser, long executionId)
        {
            var callbackResult = new CallbackResult<CefValue>
            {
                ExecutionId = executionId,
                Success = promiseResult.Success
            };

            if (promiseResult.Success)
            {
                callbackResult.Result = v8Serializer.Serialize(promiseResult.Result, new HashSet<object>());
            }
            else if(!string.IsNullOrWhiteSpace(promiseResult.Error))
            {
                callbackResult.Error = promiseResult.Error;
            }

            var response = new RpcResponse<CefValue>
            {
                CallbackResult = callbackResult
            };
            var msg = CefProcessMessage.Create(Messages.RpcResponseMessage);
            var serialized = v8Serializer.Serialize(response, new HashSet<object>());
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
