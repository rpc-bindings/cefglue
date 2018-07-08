using System;
using System.Collections.Generic;
using System.Linq;
using DSerfozo.CefGlue.Contract.Common;
using DSerfozo.CefGlue.Contract.Renderer;
using DSerfozo.RpcBindings.CefGlue.Common;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;
using DSerfozo.RpcBindings.CefGlue.Renderer.Binding;
using DSerfozo.RpcBindings.CefGlue.Renderer.Serialization;
using DSerfozo.RpcBindings.CefGlue.Renderer.Services;
using DSerfozo.RpcBindings.CefGlue.Renderer.Util;
using DSerfozo.RpcBindings.Contract.Communication.Model;
using DSerfozo.RpcBindings.Execution.Model;
using DSerfozo.RpcBindings.Model;
using static DSerfozo.CefGlue.Contract.Common.CefFactories;
using static DSerfozo.CefGlue.Contract.Renderer.CefFactories;

namespace DSerfozo.RpcBindings.CefGlue.Renderer
{
    public class BrowserController : IDisposable
    {
        private readonly Dictionary<long, IDictionary<string, ICefV8Value>> objectCache = new Dictionary<long, IDictionary<string, ICefV8Value>>();
        private readonly Dictionary<string, ObjectDescriptor> objectDescriptorCache =
            new Dictionary<string, ObjectDescriptor>();

        private readonly ObjectSerializer objectSerializer = new RendererObjectSerializer();
        private readonly SavedValueRegistry<Callback> callbackRegistry = new SavedValueRegistry<Callback>();
        private readonly SavedValueFactory<Promise> functionCallPromiseRegistry;
        private readonly SavedValueFactory<Promise> dynamicObjectPromiseRegistry;
        private readonly ICefBrowser browser;

        public BrowserController(ICefBrowser browser, PromiseService promiseService)
        {
            this.browser = browser;
            functionCallPromiseRegistry = new SavedValueFactory<Promise>(promiseService.CreatePromise);
            dynamicObjectPromiseRegistry = new SavedValueFactory<Promise>(promiseService.CreatePromise);

            objectSerializer.Serializers.Insert(0, new V8Serializer(promiseService, callbackRegistry));
            objectSerializer.Deserializers.Insert(0, new V8Deserializer(functionCallPromiseRegistry));
        }

        public bool OnProcessMessage(ICefProcessMessage message)
        {
            if (message.Name != Messages.RpcRequestMessage)
            {
                return false;
            }

            var response = (RpcRequest<ICefValue>)objectSerializer.Deserialize(message.Arguments.GetValue(0), typeof(RpcRequest<ICefValue>));
            var handled = false;
            if (response?.CallbackExecution != null)
            {
                HandledCallbackExecution(response);
                handled = true;
            }
            else if (response?.DeleteCallback != null)
            {
                if (callbackRegistry.Has(response.DeleteCallback.FunctionId))
                {
                    callbackRegistry.Get(response.DeleteCallback.FunctionId);
                }

                handled = true;
            }
            else if (response?.MethodResult != null)
            {
                HandleMethodResult(response);

                handled = true;
            }
            else if (response?.DynamicObjectResult != null)
            {
                HandleBindingResult(response);
            }

            return handled;
        }

        public ICefV8Value OnBindingRequire(ICefV8Value cefV8Value)
        {
            using (var context = CefV8Context.GetCurrentContext())
            using (var frame = context.GetFrame())
            {
                var frameId = frame.Identifier;
                var frameObjectCache = objectCache[frameId];

                var id = dynamicObjectPromiseRegistry.Save(frameId, out var promise);

                var objectName = cefV8Value.GetStringValue();
                if (!frameObjectCache.TryGetValue(objectName, out var cached))
                {
                    if (objectDescriptorCache.TryGetValue(objectName, out var descriptor))
                    {
                        CreateV8Value(descriptor, dynamicObjectPromiseRegistry.Get(id));
                    }
                    else
                    {
                        var response = new RpcResponse<ICefValue>
                        {
                            DynamicObjectRequest = new DynamicObjectRequest
                            {
                                ExecutionId = id,
                                Name = objectName
                            }
                        };

                        SendResponse(response);
                    }
                }
                else
                {
                    dynamicObjectPromiseRegistry.Get(id).Resolve(cached);
                }
                return promise.Object;
            }
        }

        public void OnBeforeNavigate(ICefFrame frame, ICefRequest request, CefNavigationType navigation_type, bool isRedirect)
        {
            if (objectCache.TryGetValue(frame.Identifier, out var frameObjectCache))
            {
                frameObjectCache.Values.ToList().ForEach(f => f.Dispose());
                frameObjectCache.Clear();
            }
        }

        public void OnContextCreated(ICefFrame frame, ICefV8Context context)
        {
            if (!objectCache.ContainsKey(frame.Identifier))
            {
                objectCache.Add(frame.Identifier, new Dictionary<string, ICefV8Value>());
            }
        }

        public void OnContextReleased(ICefFrame frame, ICefV8Context context)
        {
            if (objectCache.TryGetValue(frame.Identifier, out var frameObjectCache))
            {
                frameObjectCache.Values.ToList().ForEach(f => f.Dispose());
                frameObjectCache.Clear();
                objectCache.Remove(frame.Identifier);
            }
            callbackRegistry.Clear(frame.Identifier);
            functionCallPromiseRegistry.Clear(frame.Identifier);
            dynamicObjectPromiseRegistry.Clear(frame.Identifier);
        }

        public void Dispose()
        {
            callbackRegistry.Dispose();
            functionCallPromiseRegistry.Dispose();
            dynamicObjectPromiseRegistry.Dispose();
            browser.Dispose();
        }

        private void SendResponse(RpcResponse<ICefValue> response)
        {
            var msg = CefProcessMessage.Create(Messages.RpcResponseMessage);
            var serialized = objectSerializer.Serialize(response);
            msg.Arguments.SetValue(0, serialized);

            browser.SendProcessMessage(CefProcessId.Browser, msg);
        }


        private void HandleBindingResult(RpcRequest<ICefValue> response)
        {
            var dynamicObjectResult = response.DynamicObjectResult;
            if (dynamicObjectPromiseRegistry.Has(dynamicObjectResult.ExecutionId))
            {
                using (var promise = dynamicObjectPromiseRegistry.Get(dynamicObjectResult.ExecutionId))
                using (new ContextHelper(promise.Context))
                {
                    if (dynamicObjectResult.Success)
                    {
                        var objectDescriptor = dynamicObjectResult.ObjectDescriptor;
                        //this can happen when multiple frames are requesting the same object and the previous request was not answered yet
                        if (!objectDescriptorCache.ContainsKey(objectDescriptor.Name))
                        {
                            objectDescriptorCache.Add(objectDescriptor.Name, objectDescriptor);
                        }

                        CreateV8Value(objectDescriptor, promise);
                    }
                    else
                    {
                        promise.Reject(dynamicObjectResult.Exception);
                    }
                }
            }
        }

        private void CreateV8Value(ObjectDescriptor objectDescriptor, Promise promise)
        {
            //this can happen when multiple frames are requesting the same object and the previous request was not answered yet
            using (var frame = promise.Context.GetFrame())
            {
                var frameObjectCache = objectCache[frame.Identifier];
                if (!frameObjectCache.TryGetValue(objectDescriptor.Name, out var result))
                {
                    result = new ObjectBinder(objectDescriptor,
                        objectSerializer,
                        functionCallPromiseRegistry).BindToNew();
                    frameObjectCache.Add(objectDescriptor.Name, result);
                }

                promise.Resolve(result);
            }
        }

        private void HandleMethodResult(RpcRequest<ICefValue> response)
        {
            if (functionCallPromiseRegistry.Has(response.MethodResult.ExecutionId))
            {
                using (var promise = functionCallPromiseRegistry.Get(response.MethodResult.ExecutionId))
                using (new ContextHelper(promise.Context))
                {
                    if (response.MethodResult.Success)
                    {
                        promise.Resolve((ICefV8Value)objectSerializer.Deserialize(response.MethodResult.Result, typeof(ICefV8Value)));
                    }
                    else
                    {
                        promise.Reject(response.MethodResult.Error);
                    }
                }
            }
        }

        private void HandledCallbackExecution(RpcRequest<ICefValue> response)
        {
            if (callbackRegistry.Has(response.CallbackExecution.FunctionId))
            {
                var callback = callbackRegistry.Get(response.CallbackExecution.FunctionId);
                callback.Execute(response.CallbackExecution);
            }
            else
            {
                var rpcResponse = new RpcResponse<ICefValue>
                {
                    CallbackResult = new CallbackResult<ICefValue>
                    {
                        ExecutionId = response.CallbackExecution.ExecutionId,
                        Success = false,
                        Error = "Callback does not exist."
                    }
                };

                SendResponse(rpcResponse);
            }
        }
    }
}
