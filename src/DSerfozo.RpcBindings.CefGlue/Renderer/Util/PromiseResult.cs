using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Util
{
    public class PromiseResult
    {
        public string Id { get; set; }

        public bool Success { get; set; }

        public CefV8Value Result { get; set; }

        public CefV8Context Context { get; set; }

        public string Error { get; set; }
    }
}
