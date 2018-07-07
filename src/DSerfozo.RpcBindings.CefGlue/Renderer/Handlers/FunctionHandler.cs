using System.Collections.Generic;
using System.Linq;
using DSerfozo.RpcBindings.CefGlue.Common;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;
using DSerfozo.RpcBindings.CefGlue.Renderer.Util;
using DSerfozo.RpcBindings.Contract.Communication.Model;
using DSerfozo.RpcBindings.Execution.Model;
using DSerfozo.RpcBindings.Model;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Handlers
{
    public class FunctionHandler : CefV8Handler
    {
        private readonly long objectId;
        private readonly MethodDescriptor descriptor;
        private readonly ObjectSerializer v8Serializer;
        private readonly SavedValueFactory<Promise> functionCallRegistry;

        public FunctionHandler(long objectId, MethodDescriptor descriptor, ObjectSerializer v8Serializer, SavedValueFactory<Promise> functionCallRegistry)
        {
            this.objectId = objectId;
            this.descriptor = descriptor;
            this.v8Serializer = v8Serializer;
            this.functionCallRegistry = functionCallRegistry;
        }

        protected override bool Execute(string name, CefV8Value obj, CefV8Value[] arguments, out CefV8Value returnValue,
            out string exception)
        {
            returnValue = null;
            exception = null;

            long frameId = 0;
            using (var context = CefV8Context.GetCurrentContext())
            {
                frameId = context.GetFrame().Identifier;
            }
            var executionId = functionCallRegistry.Save(frameId, out var promise);
            returnValue = promise.Object;

            var message = new RpcResponse<CefValue>
            {
                MethodExecution = new MethodExecution<CefValue>
                {
                    ExecutionId = executionId,
                    MethodId = descriptor.Id,
                    ObjectId = objectId,
                    Parameters = arguments.Select(a => v8Serializer.Serialize(a, new HashSet<object>())).ToArray()
                }
            };

            using (var context = CefV8Context.GetCurrentContext())
            {
                var msg = CefProcessMessage.Create(Messages.RpcResponseMessage);
                var serialized = v8Serializer.Serialize(message, new HashSet<object>());
                msg.Arguments.SetValue(0, serialized.Copy());

                context.GetBrowser().SendProcessMessage(CefProcessId.Browser, msg);
            }

            return true;
        }
    }
}
