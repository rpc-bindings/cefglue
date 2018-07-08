using DSerfozo.CefGlue.Contract.Renderer;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Util
{
    public class PromiseResult
    {
        public string Id { get; set; }

        public bool Success { get; set; }

        public ICefV8Value Result { get; set; }

        public ICefV8Context Context { get; set; }

        public string Error { get; set; }
    }
}
