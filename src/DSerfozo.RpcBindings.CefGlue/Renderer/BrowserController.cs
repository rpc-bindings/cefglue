using System;
using System.Collections.Generic;
using System.Linq;
using DSerfozo.RpcBindings.CefGlue.Common;
using DSerfozo.RpcBindings.CefGlue.Common.Serialization;
using DSerfozo.RpcBindings.CefGlue.Renderer.Binding;
using DSerfozo.RpcBindings.CefGlue.Renderer.Serialization;
using DSerfozo.RpcBindings.CefGlue.Renderer.Services;
using DSerfozo.RpcBindings.CefGlue.Renderer.Util;
using DSerfozo.RpcBindings.Contract.Communication.Model;
using DSerfozo.RpcBindings.Execution.Model;
using DSerfozo.RpcBindings.Model;
using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Renderer
{
    public class BrowserController : IDisposable
    {
        private readonly Dictionary<long, IDictionary<string, CefV8Value>> objectCache = new Dictionary<long, IDictionary<string, CefV8Value>>();
        private readonly Dictionary<string, ObjectDescriptor> objectDescriptorCache =
            new Dictionary<string, ObjectDescriptor>();

        private readonly ObjectSerializer objectSerializer = new RendererObjectSerializer();
        private readonly SavedValueRegistry<Callback> callbackRegistry = new SavedValueRegistry<Callback>();
        private readonly SavedValueFactory<Promise> functionCallPromiseRegistry;
        private readonly SavedValueFactory<Promise> dynamicObjectPromiseRegistry;
        private readonly CefBrowser browser;

        public BrowserController(CefBrowser browser, PromiseService promiseService)
        {
            this.browser = browser;
            functionCallPromiseRegistry = new SavedValueFactory<Promise>(promiseService.CreatePromise);
            dynamicObjectPromiseRegistry = new SavedValueFactory<Promise>(promiseService.CreatePromise);

            objectSerializer.Serializers.Insert(0, new V8Serializer(promiseService, callbackRegistry));
            objectSerializer.Deserializers.Insert(0, new V8Deserializer(functionCallPromiseRegistry));
        }

        public bool OnProcessMessage(CefProcessMessage message)
        {
            if (message.Name != Messages.RpcRequestMessage)
            {
                return false;
            }

            var response = (RpcRequest<CefValue>)objectSerializer.Deserialize(message.Arguments.GetValue(0), typeof(RpcRequest<CefValue>));
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

        public CefV8Value OnBindingRequire(CefV8Value cefV8Value)
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
                        var response = new RpcResponse<CefValue>
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

        public void OnBeforeNavigate(CefFrame frame, CefRequest request, CefNavigationType navigation_type, bool isRedirect)
        {
            if (objectCache.TryGetValue(frame.Identifier, out var frameObjectCache))
            {
                frameObjectCache.Values.ToList().ForEach(f => f.Dispose());
                frameObjectCache.Clear();
            }
        }

        public void OnContextCreated(CefFrame frame, CefV8Context context)
        {
            if (!objectCache.ContainsKey(frame.Identifier))
            {
                objectCache.Add(frame.Identifier, new Dictionary<string, CefV8Value>());
            }
        }

        public void OnContextReleased(CefFrame frame, CefV8Context context)
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

        private void SendResponse(RpcResponse<CefValue> response)
        {
            var msg = CefProcessMessage.Create(Messages.RpcResponseMessage);
            var serialized = objectSerializer.Serialize(response, new HashSet<object>());
            msg.Arguments.SetValue(0, serialized);

            browser.SendProcessMessage(CefProcessId.Browser, msg);
        }


        private void HandleBindingResult(RpcRequest<CefValue> response)
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

        private void HandleMethodResult(RpcRequest<CefValue> response)
        {
            if (functionCallPromiseRegistry.Has(response.MethodResult.ExecutionId))
            {
                using (var promise = functionCallPromiseRegistry.Get(response.MethodResult.ExecutionId))
                using (new ContextHelper(promise.Context))
                {
                    if (response.MethodResult.Success)
                    {
                        promise.Resolve((CefV8Value)objectSerializer.Deserialize(response.MethodResult.Result, typeof(CefV8Value)));
                    }
                    else
                    {
                        promise.Reject(response.MethodResult.Error);
                    }
                }
            }
        }

        private void HandledCallbackExecution(RpcRequest<CefValue> response)
        {
            if (callbackRegistry.Has(response.CallbackExecution.FunctionId))
            {
                var callback = callbackRegistry.Get(response.CallbackExecution.FunctionId);
                callback.Execute(response.CallbackExecution);
            }
            else
            {
                var rpcResponse = new RpcResponse<CefValue>
                {
                    CallbackResult = new CallbackResult<CefValue>
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
