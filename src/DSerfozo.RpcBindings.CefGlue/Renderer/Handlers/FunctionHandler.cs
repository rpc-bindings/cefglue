using System.Collections.Generic;
using System.Linq;
using DSerfozo.CefGlue.Contract.Common;
using DSerfozo.CefGlue.Contract.Renderer;
using DSerfozo.RpcBindings.CefGlue.Common;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;
using DSerfozo.RpcBindings.CefGlue.Renderer.Util;
using DSerfozo.RpcBindings.Contract.Communication.Model;
using DSerfozo.RpcBindings.Execution.Model;
using DSerfozo.RpcBindings.Model;
using static DSerfozo.CefGlue.Contract.Renderer.CefFactories;
using static DSerfozo.CefGlue.Contract.Common.CefFactories;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Handlers
{
    public class FunctionHandler : ICefV8Handler
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

        public bool Execute(string name, ICefV8Value obj, ICefV8Value[] arguments, out ICefV8Value returnValue,
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

            var message = new RpcResponse<ICefValue>
            {
                MethodExecution = new MethodExecution<ICefValue>
                {
                    ExecutionId = executionId,
                    MethodId = descriptor.Id,
                    ObjectId = objectId,
                    Parameters = arguments.Select(a => v8Serializer.Serialize(a)).ToArray()
                }
            };

            using (var context = CefV8Context.GetCurrentContext())
            {
                var msg = CefProcessMessage.Create(Messages.RpcResponseMessage);
                var serialized = v8Serializer.Serialize(message);
                msg.Arguments.SetValue(0, serialized.Copy());

                context.GetBrowser().SendProcessMessage(CefProcessId.Browser, msg);
            }

            return true;
        }
    }
}
